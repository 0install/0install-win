/*
 * Copyright 2010-2016 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store
{
    partial class Config
    {
        /// <summary>
        /// Retrieves the string representation of an option identified by a key.
        /// </summary>
        /// <param name="key">The key of the option to retrieve.</param>
        /// <returns>The string representation of the the option.</returns>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> is invalid.</exception>
        public string GetOption(string key)
        {
            return _metaData[key].Value;
        }

        /// <summary>
        /// Sets an option identified by a key.
        /// </summary>
        /// <param name="key">The key of the option to set.</param>
        /// <param name="value">A string representation of the option.</param>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> is invalid.</exception>
        /// <exception cref="FormatException"><paramref name="value"/> is invalid.</exception>
        public void SetOption(string key, string value)
        {
            _metaData[key].Value = value;
        }

        /// <summary>
        /// Resets an option identified by a key to its default value.
        /// </summary>
        /// <param name="key">The key of the option to reset.</param>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> is invalid.</exception>
        public void ResetOption(string key)
        {
            var property = _metaData[key];
            property.Value = property.DefaultValue;
        }

        private const string RegistryPolicyPath = @"SOFTWARE\Policies\Zero Install";

        /// <summary>
        /// Aggregates the settings from all applicable INI files listed by <see cref="Locations.GetLoadConfigPaths"/>.
        /// </summary>
        /// <returns>The loaded <see cref="Config"/>.</returns>
        /// <exception cref="IOException">A problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the config data.</exception>
        [NotNull]
        public static Config Load()
        {
            var config = new Config();

            config.ReadFromAppSettings();
            config.ReadFromIniFiles();
            if (WindowsUtils.IsWindowsNT)
                config.ReadFromRegistry();

            return config;
        }

        /// <summary>
        /// Loads the settings from a single INI file.
        /// </summary>
        /// <returns>The loaded <see cref="Config"/>.</returns>
        /// <exception cref="IOException">A problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the config data.</exception>
        [NotNull]
        public static Config Load([NotNull] string path)
        {
            Log.Debug("Loading Config from: " + path);

            var config = new Config();
            config.ReadFromIniFile(path);
            return config;
        }

        /// <summary>
        /// Saves the settings to an INI file in the default location in the user profile.
        /// </summary>
        /// <remarks>This method performs an atomic write operation when possible.</remarks>
        /// <exception cref="IOException">A problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        public void Save()
        {
            Save(Locations.GetSaveConfigPath("0install.net", true, "injector", "global"));
        }

        /// <summary>
        /// Saves the settings to an INI file.
        /// </summary>
        /// <remarks>This method performs an atomic write operation when possible.</remarks>
        /// <exception cref="IOException">A problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        public void Save([NotNull] string path)
        {
            TransferToIni();

            Log.Debug("Saving Config to: " + path);

            using (var atomic = new AtomicWrite(path))
            using (var writer = new StreamWriter(atomic.WritePath, append: false, encoding: FeedUtils.Encoding))
            {
                new StreamIniDataParser().WriteData(writer, _iniData);
                atomic.Commit();
            }
        }

        /// <summary>
        /// Reads settings from INI files on the disk and transfers them to properties.
        /// </summary>
        private void ReadFromIniFiles()
        {
            var paths = Locations.GetLoadConfigPaths("0install.net", true, "injector", "global");
            foreach (var path in paths.Reverse()) // Read least important first
                ReadFromIniFile(path);
        }

        private const string GlobalSection = "global";
        private const string Base64Suffix = "_base64";

        /// <summary>Stores the original INI data so that unknown values are preserved on re<see cref="Save()"/>ing.</summary>
        [NonSerialized]
        private IniData _iniData;

        /// <summary>
        /// Reads settings from an INI file on the disk and transfers them to properties.
        /// </summary>
        private void ReadFromIniFile([NotNull] string path)
        {
            try
            {
                using (var reader = new StreamReader(path, Encoding.UTF8))
                    _iniData = new StreamIniDataParser().ReadData(reader);
            }
                #region Error handling
            catch (ParsingException ex)
            {
                // Wrap exception to add context information
                throw new InvalidDataException(string.Format(Resources.ProblemLoadingConfigFile, path), ex);
            }
            #endregion

            if (!_iniData.Sections.ContainsSection(GlobalSection)) return;
            var global = _iniData[GlobalSection];
            foreach (var property in _metaData)
            {
                string key = property.Key;
                if (property.Value.NeedsEncoding) key += Base64Suffix;

                if (global.ContainsKey(key))
                {
                    try
                    {
                        property.Value.Value = property.Value.NeedsEncoding
                            ? global[key].Base64Utf8Decode()
                            : global[key];
                    }
                        #region Error handling
                    catch (FormatException ex)
                    {
                        // Wrap exception to add context information
                        throw new InvalidDataException(string.Format(Resources.ProblemLoadingConfigValue, property.Key, path), ex);
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Transfers settings from properties to <see cref="_iniData"/>.
        /// </summary>
        private void TransferToIni()
        {
            if (_iniData == null) _iniData = new IniData();
            _iniData.Sections.RemoveSection("__global__section__"); // Throw away section-less data

            if (!_iniData.Sections.ContainsSection(GlobalSection)) _iniData.Sections.AddSection(GlobalSection);
            var global = _iniData[GlobalSection];

            foreach (var property in _metaData)
            {
                string key = property.Key;
                if (property.Value.NeedsEncoding) key += Base64Suffix;

                if (property.Value.IsDefaultValue || property.Value.Value == null)
                    global.RemoveKey(key);
                else
                {
                    global[key] = property.Value.NeedsEncoding
                        ? property.Value.Value.Base64Utf8Encode()
                        : property.Value.Value;
                }
            }
        }

        /// <summary>
        /// Reads settings from <see cref="ConfigurationManager.AppSettings"/> and transfers them to properties.
        /// </summary>
        private void ReadFromAppSettings()
        {
            foreach (var property in _metaData)
            {
                string value = ConfigurationManager.AppSettings[property.Key];
                if (!string.IsNullOrEmpty(value))
                    property.Value.Value = value;
            }
        }

        /// <summary>
        /// Reads settings from Windows policy registry keys and transfers them to properties.
        /// </summary>
        private void ReadFromRegistry()
        {
            using (var registryKey = Registry.LocalMachine.OpenSubKey(RegistryPolicyPath, writable: false))
                if (registryKey != null) ReadFromRegistry(registryKey);
            using (var registryKey = Registry.CurrentUser.OpenSubKey(RegistryPolicyPath, writable: false))
                if (registryKey != null) ReadFromRegistry(registryKey);
        }

        /// <summary>
        /// Reads settings from a Windows registry key and transfers them to properties.
        /// </summary>
        private void ReadFromRegistry([NotNull] RegistryKey registryKey)
        {
            Log.Debug("Loading config from: " + registryKey);

            foreach (var property in _metaData)
            {
                string key = property.Key;
                object data = registryKey.GetValue(key);
                if (data != null)
                {
                    string value = data.ToString();
                    try
                    {
                        property.Value.Value = value;
                    }
                        #region Error handling
                    catch (FormatException ex)
                    {
                        // Wrap exception to add context information
                        throw new InvalidDataException(string.Format(Resources.ProblemLoadingConfigValue, property.Key, registryKey.Name), ex);
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Config"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Config"/>.</returns>
        public Config Clone()
        {
            var newConfig = new Config();
            foreach (var property in _metaData)
                newConfig.SetOption(property.Key, GetOption(property.Key));
            return newConfig;
        }

        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Returns the keys and values of all contained setings.
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var property in _metaData)
                builder.AppendLine(property.Key + " = " + (property.Value.NeedsEncoding ? "***" : property.Value.Value));
            return builder.ToString();
        }

        /// <inheritdoc/>
        public bool Equals(Config other)
        {
            if (other == null) return false;
            return _metaData.All(property => property.Value.Value == other.GetOption(property.Key));
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            var config = obj as Config;
            return config != null && Equals(config);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = 397;
                foreach (var property in _metaData)
                {
                    if (property.Value.Value != null)
                        result = (result * 397) ^ property.Value.Value.GetHashCode();
                }
                return result;
            }
        }
    }
}
