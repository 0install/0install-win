/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using IniParser;
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Utils;
using NanoByte.Common.Values.Design;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store
{

    #region Enumerations
    /// <summary>
    /// Controls how liberally network access is attempted.
    /// </summary>
    /// <see cref="Config.NetworkUse"/>
    public enum NetworkLevel
    {
        /// <summary>Do not access network at all.</summary>
        Offline,

        /// <summary>Only access network when there are no safe implementations available.</summary>
        Minimal,

        /// <summary>Always use network to get the newest available versions.</summary>
        Full
    }
    #endregion

    /// <summary>
    /// User settings controlling network behaviour, solving, etc.
    /// </summary>
    [Serializable]
    public sealed class Config : ICloneable, IEquatable<Config>
    {
        #region Constants
        private const string RegistryPolicyPath = @"SOFTWARE\Policies\Zero Install";
        #endregion

        #region Variables
        /// <summary>Provides meta-data for loading and saving settings properties.</summary>
        private readonly Dictionary<string, PropertyPointer<string>> _metaData;

        /// <summary>Stores the original INI data so that unknown values are preserved on re<see cref="Save()"/>ing.</summary>
        [NonSerialized]
        private IniData _iniData;
        #endregion

        #region Properties
        private static readonly TimeSpan _defaultFreshness = new TimeSpan(7, 0, 0, 0, 0); // 7 days
        private TimeSpan _freshness = _defaultFreshness;

        // ReSharper disable LocalizableElement
        /// <summary>
        /// The maximum age a cached <see cref="Store.Model.Feed"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        [DefaultValue(typeof(TimeSpan), "7.00:00:00"), Category("Policy"), DisplayName("Freshness"), Description("The maximum age a cached feed may have until it is considered stale (needs to be updated).")]
        [Editor(typeof(TimeSpanEditor), typeof(UITypeEditor))]
        public TimeSpan Freshness { get { return _freshness; } set { _freshness = value; } }

        /// <summary>
        /// Always prefer the newest versions, even if they have not been marked as <see cref="Store.Model.Stability.Stable"/> yet.
        /// </summary>
        [DefaultValue(false), Category("Policy"), DisplayName("Help with testing"), Description("Always prefer the newest versions, even if they havent been marked as stable yet.")]
        public bool HelpWithTesting { get; set; }

        private NetworkLevel _networkLevel = NetworkLevel.Full;

        /// <summary>
        /// Controls how liberally network access is attempted.
        /// </summary>
        [DefaultValue(typeof(NetworkLevel), "Full"), Category("Policy"), DisplayName("Network use"), Description("Controls how liberally network access is attempted.")]
        public NetworkLevel NetworkUse
        {
            get { return _networkLevel; }
            set
            {
                #region Sanity checks
                if (!Enum.IsDefined(typeof(NetworkLevel), value)) throw new ArgumentOutOfRangeException("value");
                #endregion

                _networkLevel = value;
            }
        }

        private bool _autoApproveKeys = true;

        /// <summary>
        /// Automatically approve keys known by the <see cref="KeyInfoServer"/> and seen the first time a feed is fetched.
        /// </summary>
        [DefaultValue(true), Category("Policy"), DisplayName("Auto approve keys"), Description("Automatically approve keys known by the key info server and seen the first time a feed is fetched.")]
        public bool AutoApproveKeys { get { return _autoApproveKeys; } set { _autoApproveKeys = value; } }

        private bool _allowApiHooking;

        /// <summary>
        /// WARNING! This feature is highly experimental!<br/>
        /// Controls whether Zero Install may install hooks for operating sytem APIs to improve desktop integration.
        /// </summary>
        [DefaultValue(false), Category("Policy"), DisplayName("Allow API hooking"), Description("WARNING! This feature is highly experimental!\nControls whether Zero Install may install hooks for operating sytem APIs to improve desktop integration.")]
        public bool AllowApiHooking { get { return _allowApiHooking; } set { _allowApiHooking = value; } }

        /// <summary>
        /// The default value for <see cref="FeedMirror"/>.
        /// </summary>
        public const string DefaultFeedMirror = "http://roscidus.com/0mirror";

        private Uri _feedMirror = new Uri(DefaultFeedMirror);

        /// <summary>
        /// The base URL of a mirror site for keys and feeds.
        /// </summary>
        [DefaultValue(typeof(Uri), DefaultFeedMirror), Category("Sources"), DisplayName("Feed mirror"), Description("The base URL of a mirror site for keys and feeds.")]
        public Uri FeedMirror { get { return _feedMirror; } set { _feedMirror = value.Sanitize(); } }

        /// <summary>
        /// The default value for <see cref="KeyInfoServer"/>.
        /// </summary>
        public const string DefaultKeyInfoServer = "https://keylookup.appspot.com/";

        private Uri _keyInfoServer = new Uri(DefaultKeyInfoServer);

        /// <summary>
        /// The base URL of a key information server.
        /// </summary>
        [DefaultValue(typeof(Uri), DefaultKeyInfoServer), Category("Sources"), DisplayName("Key info server"), Description("The base URL of a key information server.")]
        public Uri KeyInfoServer { get { return _keyInfoServer; } set { _keyInfoServer = value.Sanitize(); } }

        /// <summary>
        /// The default value for <see cref="SelfUpdateUri"/>.
        /// </summary>
        public const string DefaultSelfUpdateUri = "http://0install.de/feeds/ZeroInstall.xml";

        private Uri _selfUpdateUri = new Uri(DefaultSelfUpdateUri);

        /// <summary>
        /// The ID used by the solver to search for updates for Zero Install itself.
        /// </summary>
        [DefaultValue(typeof(Uri), DefaultSelfUpdateUri), Category("Sources"), DisplayName("Self-update URI"), Description("The URI used by the solver to search for updates for Zero Install itself.")]
        public Uri SelfUpdateUri { get { return _selfUpdateUri; } set { _selfUpdateUri = value.Sanitize(); } }

        /// <summary>
        /// The default value for <see cref="SyncServer"/>.
        /// </summary>
        public const string DefaultSyncServer = "https://0install.de/sync/";

        private Uri _syncServer = new Uri(DefaultSyncServer);

        /// <summary>
        /// The base URL of the sync server.
        /// </summary>
        /// <seealso cref="SyncServerUsername"/>
        /// <seealso cref="SyncServerPassword"/>
        [DefaultValue(typeof(Uri), DefaultSyncServer), Category("Sync"), DisplayName("Server"), Description("The base URL of the sync server.")]
        public Uri SyncServer { get { return _syncServer; } set { _syncServer = value.Sanitize(); } }

        private string _syncServerUsername = "";

        /// <summary>
        /// The username to authenticate with against the <see cref="SyncServer"/>.
        /// </summary>
        /// <seealso cref="SyncServer"/>
        /// <seealso cref="SyncServerPassword"/>
        [DefaultValue(""), Category("Sync"), DisplayName("Username"), Description("The username to authenticate with against the Sync server.")]
        public string SyncServerUsername { get { return _syncServerUsername; } set { _syncServerUsername = value; } }

        private string _syncServerPassword = "";

        /// <summary>
        /// The password to authenticate with against the <see cref="SyncServer"/>.
        /// </summary>
        /// <seealso cref="SyncServer"/>
        /// <seealso cref="SyncServerUsername"/>
        [DefaultValue(""), PasswordPropertyText(true), Category("Sync"), DisplayName("Password"), Description("The password to authenticate with against the Sync server.")]
        public string SyncServerPassword { get { return _syncServerPassword; } set { _syncServerPassword = value; } }

        private string _syncCryptoKey = "";

        /// <summary>
        /// The local key used to encrypt data before sending it to the <see cref="SyncServer"/>.
        /// </summary>
        [DefaultValue(""), PasswordPropertyText(true), Category("Sync"), DisplayName("Crypto key"), Description("The local key used to encrypt data before sending it to the Sync server.")]
        public string SyncCryptoKey { get { return _syncCryptoKey; } set { _syncCryptoKey = value; } }

        // ReSharper restore LocalizableElement
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new configuration with default values set.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Key-value dispatcher")]
        public Config()
        {
            _metaData = new Dictionary<string, PropertyPointer<string>>
            {
                {"freshness", new PropertyPointer<TimeSpan>(() => Freshness, value => Freshness = value, _defaultFreshness).ToStringPointer()},
                {"help_with_testing", new PropertyPointer<bool>(() => HelpWithTesting, value => HelpWithTesting = value).ToStringPointer()},
                {"network_use", GetNetworkUseConverter()},
                {"auto_approve_keys", new PropertyPointer<bool>(() => AutoApproveKeys, value => AutoApproveKeys = value, true).ToStringPointer()},
                {"allow_api_hooking", new PropertyPointer<bool>(() => AllowApiHooking, value => AllowApiHooking = value).ToStringPointer()},
                {"feed_mirror", new PropertyPointer<Uri>(() => FeedMirror, value => FeedMirror = value, new Uri(DefaultFeedMirror)).ToStringPointer()},
                {"key_info_server", new PropertyPointer<Uri>(() => KeyInfoServer, value => KeyInfoServer = value, new Uri(DefaultKeyInfoServer)).ToStringPointer()},
                {"self_update_uri", new PropertyPointer<Uri>(() => SelfUpdateUri, value => SelfUpdateUri = value, new Uri(DefaultSelfUpdateUri)).ToStringPointer()},
                {"sync_server", new PropertyPointer<Uri>(() => SyncServer, value => SyncServer = value, new Uri(DefaultSyncServer)).ToStringPointer()},
                {"sync_server_user", new PropertyPointer<string>(() => SyncServerUsername, value => SyncServerUsername = value, "")},
                {"sync_server_pw", new PropertyPointer<string>(() => SyncServerPassword, value => SyncServerPassword = value, "", true)},
                {"sync_crypto_key", new PropertyPointer<string>(() => SyncCryptoKey, value => SyncCryptoKey = value, "", true)},
            };
        }

        /// <summary>
        /// Creates a <see cref="string"/> pointer referencing <see cref="NetworkUse"/>. Uses hardcoded string lookup tables.
        /// </summary>
        private PropertyPointer<string> GetNetworkUseConverter()
        {
            return new PropertyPointer<string>(
                delegate
                {
                    switch (NetworkUse)
                    {
                        case NetworkLevel.Full:
                            return "full";
                        case NetworkLevel.Minimal:
                            return "minimal";
                        case NetworkLevel.Offline:
                            return "off-line";
                    }
                    return null; // Will never be reached
                },
                delegate(string value)
                {
                    switch (value)
                    {
                        case "full":
                            NetworkUse = NetworkLevel.Full;
                            return;
                        case "minimal":
                            NetworkUse = NetworkLevel.Minimal;
                            return;
                        case "off-line":
                            NetworkUse = NetworkLevel.Offline;
                            return;
                        default:
                            throw new FormatException("Must be 'full', 'minimal' or 'off-line'");
                    }
                },
                "full");
        }
        #endregion

        //--------------------//

        #region Access
        /// <summary>
        /// Retrieves the string representation of an option identified by a key.
        /// </summary>
        /// <param name="key">The key of the option to retrieve.</param>
        /// <returns>The string representation of the the option.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> is invalid.</exception>
        public string GetOption(string key)
        {
            return _metaData[key].Value;
        }

        /// <summary>
        /// Sets an option identified by a key.
        /// </summary>
        /// <param name="key">The key of the option to set.</param>
        /// <param name="value">A string representation of the option.</param>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> is invalid.</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> is invalid.</exception>
        public void SetOption(string key, string value)
        {
            _metaData[key].Value = value;
        }

        /// <summary>
        /// Resets an option identified by a key to its default value.
        /// </summary>
        /// <param name="key">The key of the option to reset.</param>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> is invalid.</exception>
        public void ResetOption(string key)
        {
            var property = _metaData[key];
            property.Value = property.DefaultValue;
        }
        #endregion

        #region Storage
        /// <summary>
        /// Loads the settings from a single INI file.
        /// </summary>
        /// <returns>The loaded <see cref="Config"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the config data.</exception>
        public static Config Load(string path)
        {
            var config = new Config();
            config.ReadFromIniFile(path);
            return config;
        }

        /// <summary>
        /// Aggregates the settings from all applicable INI files listed by <see cref="Locations.GetLoadConfigPaths"/>.
        /// </summary>
        /// <returns>The loaded <see cref="Config"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the config data.</exception>
        public static Config Load()
        {
            // Locate all applicable config files
            var paths = Locations.GetLoadConfigPaths("0install.net", true, "injector", "global");

            // Accumulate values from all files
            var config = new Config();
            foreach (var path in paths.Reverse()) // Read least important first
                config.ReadFromIniFile(path);

            // Apply Windows registry policies (override existing config)
            if (WindowsUtils.IsWindowsNT)
            {
                using (var registryKey = Registry.LocalMachine.OpenSubKey(RegistryPolicyPath, writable: false))
                    if (registryKey != null) config.ReadFromRegistryKey(registryKey);
                using (var registryKey = Registry.CurrentUser.OpenSubKey(RegistryPolicyPath, writable: false))
                    if (registryKey != null) config.ReadFromRegistryKey(registryKey);
            }

            return config;
        }

        /// <summary>
        /// Saves the settings to an INI file.
        /// </summary>
        /// <remarks>This method performs an atomic write operation when possible.</remarks>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            TransferToIni();

            using (var atomic = new AtomicWrite(path))
            using (var writer = new StreamWriter(atomic.WritePath, false, new UTF8Encoding(false)))
            {
                new StreamIniDataParser().WriteData(writer, _iniData);
                atomic.Commit();
            }
        }

        /// <summary>
        /// Saves the settings to an INI file in the default location in the user profile.
        /// </summary>
        /// <remarks>This method performs an atomic write operation when possible.</remarks>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save()
        {
            Save(Locations.GetSaveConfigPath("0install.net", true, "injector", "global"));
        }
        #endregion

        #region Serialization
        /// <summary>
        /// Reads data from an INI file on the disk and transfers it to properties using <see cref="_metaData"/>.
        /// </summary>
        private void ReadFromIniFile(string path)
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

            if (!_iniData.Sections.ContainsSection("global")) return;

            var global = _iniData["global"];
            foreach (var property in _metaData)
            {
                string key = property.Key;
                if (property.Value.NeedsEncoding) key += "_base64";

                if (global.ContainsKey(key))
                {
                    try
                    {
                        property.Value.Value = property.Value.NeedsEncoding ? global[key].Base64Utf8Decode() : global[key];
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

            // Migrate passwords/keys that are not base64-encoded yet
            if (global.ContainsKey("sync_server_pw"))
            {
                SyncServerPassword = global["sync_server_pw"];
                global.RemoveKey("sync_server_pw");
            }
            if (global.ContainsKey("sync_crypto_key"))
            {
                SyncCryptoKey = global["sync_crypto_key"];
                global.RemoveKey("sync_crypto_key");
            }
        }

        /// <summary>
        /// Reads data from a Windows registry key and transfers it to properties using <see cref="_metaData"/>.
        /// </summary>
        private void ReadFromRegistryKey(RegistryKey registryKey)
        {
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
        /// Transfers data from properties to <see cref="_iniData"/> using <see cref="_metaData"/>.
        /// </summary>
        private void TransferToIni()
        {
            if (_iniData == null) _iniData = new IniData();
            _iniData.Sections.RemoveSection("__global__section__"); // Throw away section-less data

            if (!_iniData.Sections.ContainsSection("global")) _iniData.Sections.AddSection("global");
            var global = _iniData["global"];

            foreach (var property in _metaData)
            {
                string key = property.Key;
                if (property.Value.NeedsEncoding) key += "_base64";

                // Remove the old value and only set the new one if it isn't the default value
                global.RemoveKey(key);
                if (!Equals(property.Value.DefaultValue, property.Value.Value))
                    global.AddKey(key, property.Value.NeedsEncoding ? property.Value.Value.Base64Utf8Encode() : property.Value.Value);
            }
        }
        #endregion

        //--------------------//

        #region Clone
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

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
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
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Config other)
        {
            if (other == null) return false;
            return _metaData.All(property => property.Value.Value == other.GetOption(property.Key));
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Config && Equals((Config)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable LoopCanBeConvertedToQuery
                int result = 397;
                foreach (var property in _metaData)

                    if (property.Value.Value != null) result = (result * 397) ^ property.Value.Value.GetHashCode();
                return result;
                // ReSharper restore LoopCanBeConvertedToQuery
            }
        }
        #endregion
    }
}
