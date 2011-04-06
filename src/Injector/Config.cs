/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Text;
using Common;
using Common.Storage;
using Common.Values.Design;
using IniParser;

namespace ZeroInstall.Injector
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
    public sealed class Config : IEquatable<Config>, ICloneable
    {
        #region Variables
        /// <summary>Provides meta-data for loading and saving settings properties.</summary>
        private readonly C5.HashDictionary<string, PropertyPointer<string>> _metaData;

        /// <summary>Singleton used for reading and writing INI files.</summary>
        private static readonly FileIniDataParser _iniParse = new FileIniDataParser();

        /// <summary>Stores the original INI data so that unknown values are preserved on re<see cref="Save()"/>ing.</summary>
        [NonSerialized]
        private IniData _iniData;
        #endregion

        #region Properties
        /// <summary>
        /// Always prefer the newest versions, even if they havent been marked as <see cref="Model.Stability.Stable"/> yet.
        /// </summary>
        [DefaultValue(false), DisplayName("Help with testing"), Description("Always prefer the newest versions, even if they havent been marked as stable yet.")]
        public bool HelpWithTesting { get; set; }

        private static readonly TimeSpan _defaultFreshness = new TimeSpan(30, 0, 0, 0, 0); // 1 month
        private TimeSpan _freshness = _defaultFreshness;
        /// <summary>
        /// The maximum age a cached <see cref="Model.Feed"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        [DefaultValue(typeof(TimeSpan), "30.00:00:00"), DisplayName("Freshness"), Description("The maximum age a cached feed may have until it is considered stale (needs to be updated).")]
        [EditorAttribute(typeof(TimeSpanEditor), typeof(UITypeEditor))]
        public TimeSpan Freshness { get { return _freshness; } set { _freshness = value; } }

        private NetworkLevel _networkLevel = NetworkLevel.Full;
        /// <summary>
        /// Controls how liberally network access is attempted.
        /// </summary>
        [DefaultValue(typeof(NetworkLevel), "Full"), DisplayName("Network use"), Description("Controls how liberally network access is attempted.")]
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

        private const string DefaultFeedMirror = "http://roscidus.com/0mirror";
        private Uri _feedMirror = new Uri(DefaultFeedMirror);
        /// <summary>
        /// The base URL of a mirror site for keys and feeds.
        /// </summary>
        [DefaultValue(typeof(Uri), DefaultFeedMirror), DisplayName("Feed mirror"), Description("The base URL of a mirror site for keys and feeds.")]
        [Browsable(false)] // This option is not supported yet
        public Uri FeedMirror { get { return _feedMirror; } set { _feedMirror = value; } }

        private const string DefaultKeyInfoServer = "https://keylookup.appspot.com";
        private Uri _keyInfoServer = new Uri(DefaultKeyInfoServer);
        /// <summary>
        /// The base URL of a key information server.
        /// </summary>
        [DefaultValue(typeof(Uri), DefaultKeyInfoServer), DisplayName("Key info server"), Description("The base URL of a key information server.")]
        [Browsable(false)] // This option is not supported yet
        public Uri KeyInfoServer { get { return _keyInfoServer; } set { _keyInfoServer = value; } }

        private bool _autoApproveKeys = true;
        /// <summary>
        /// Automatically approve keys known by the <see cref="KeyInfoServer"/> and seen the first time a feed is fetched.
        /// </summary>
        [DefaultValue(true), DisplayName("Auto approve keys"), Description("Automatically approve keys known by the key info server and seen the first time a feed is fetched.")]
        public bool AutoApproveKeys { get { return _autoApproveKeys; } set { _autoApproveKeys = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new configuration with default values set.
        /// </summary>
        public Config()
        {
            _metaData = new C5.HashDictionary<string, PropertyPointer<string>>
            {
                {"help_with_testing", PropertyPointer.GetBoolConverter(new PropertyPointer<bool>(() => HelpWithTesting, value => HelpWithTesting = value, false))},
                {"freshness", PropertyPointer.GetTimespanConverter(new PropertyPointer<TimeSpan>(() => Freshness, value => Freshness = value, _defaultFreshness))},
                {"network_use", GetNetworkUseConverter()},
                //{"feed_mirror", PropertyPointer.GetUriConverter(new PropertyPointer<Uri>(() => FeedMirror, value => FeedMirror = value, new Uri(DefaultFeedMirror)))},
                //{"key_info_server", PropertyPointer.GetUriConverter(new PropertyPointer<Uri>(() => KeyInfoServer, value => KeyInfoServer = value, new Uri(DefaultKeyInfoServer)))},
                {"auto_approve_keys", PropertyPointer.GetBoolConverter(new PropertyPointer<bool>(() => AutoApproveKeys, value => AutoApproveKeys = value, true))}
            };
        }

        /// <summary>
        /// Creates a <see cref="string"/> pointer referencing <see cref="NetworkUse"/>. Uses hardcoded string lookup tables.
        /// </summary>
        /// <returns></returns>
        private PropertyPointer<string> GetNetworkUseConverter()
        {
            return new PropertyPointer<string>(
                delegate
                {
                    switch (NetworkUse)
                    {
                        case NetworkLevel.Full: return "full";
                        case NetworkLevel.Minimal: return "minimal";
                        case NetworkLevel.Offline: return "off-line";
                    }
                    return null; // Will never be reached
                },
                delegate(string value)
                {
                    switch (value)
                    {
                        case "full": NetworkUse = NetworkLevel.Full; return;
                        case "minimal": NetworkUse = NetworkLevel.Minimal; return;
                        case "off-line": NetworkUse = NetworkLevel.Offline; return;
                        default: throw new ArgumentException("Must be 'full', 'minimal' or 'off-line'", "value");
                    }
                },
                "full");
        }
        #endregion

        //--------------------//

        #region Access
        /// <summary>
        /// Retreives the string representation of an option identified by a key.
        /// </summary>
        /// <param name="key">The key of the option to retreive.</param>
        /// <returns>The string representation of the the option.</returns>
        /// <exception cref="C5.NoSuchItemException">Thrown if <paramref name="key"/> is invalid.</exception>
        public string GetOption(string key)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            #endregion

            return _metaData[key].Value;
        }

        /// <summary>
        /// Sets an option identified by a key.
        /// </summary>
        /// <param name="key">The key of the option to set.</param>
        /// <param name="value">A string representation of the option.</param>
        /// <exception cref="C5.NoSuchItemException">Thrown if <paramref name="key"/> is invalid.</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> is invalid.</exception>
        public void SetOption(string key, string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            _metaData[key].Value = value;
        }
        #endregion

        #region Storage
        /// <summary>
        /// Loads the settings from a single INI file.
        /// </summary>
        /// <returns>The loaded <see cref="Config"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the config data.</exception>
        public static Config Load(string path)
        {
            var iniData = _iniParse.LoadFile(path, true);
            var config = new Config {_iniData = iniData};

            FromIniToConfig(iniData, config);
            return config;
        }

        /// <summary>
        /// Aggregates the settings from all applicable INI files listed by <see cref="Locations.GetLoadConfigPaths"/>.
        /// </summary>
        /// <returns>The loaded <see cref="Config"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the config data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Problems loading generic configuration files shouldn't block the application")]
        public static Config Load()
        {
            // Locate all applicable config files and order them from least to most important
            var paths = new List<string>(Locations.GetLoadConfigPaths("0install.net", Path.Combine("injector", "global"), false));
            paths.Reverse();

            // Accumulate values from all files
            var config = new Config();
            foreach (var path in paths)
            {
                try { FromIniToConfig(_iniParse.LoadFile(path, true), config); }
                catch (Exception ex)
                {
                    // In case of failure log and continue with default value
                    Log.Warn("Failed to load: " + path + "\n" + ex.Message);
                    Log.Warn("Reverting to default values");
                    return new Config();
                }
            }
            return config;
        }

        /// <summary>
        /// Saves the settings to an INI file.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            if (_iniData == null) _iniData = new IniData();
            FromConfigToIni(this, _iniData);
            _iniParse.SaveFile(path, _iniData);
        }

        /// <summary>
        /// Saves the settings to an INI file in the default location in the user profile.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save()
        {
            Save(Locations.GetSaveConfigPath("0install.net", Path.Combine("injector", "global"), false));
        }
        #endregion

        #region Serialization
        /// <summary>
        /// Transfers parsed <see cref="IniData"/> to <see cref="Config"/> properties using <see cref="_metaData"/>.
        /// </summary>
        private static void FromIniToConfig(IniData iniData, Config config)
        {
            if (!iniData.Sections.ContainsSection("global")) return;

            var global = iniData["global"];
            foreach (var property in config._metaData)
            {
                if (global.ContainsKey(property.Key))
                    property.Value.Value = global[property.Key];
            }
        }

        /// <summary>
        /// Transfers <see cref="Config"/> properties to <see cref="IniData"/> using <see cref="_metaData"/>.
        /// </summary>
        private static void FromConfigToIni(Config config, IniData iniData)
        {
            if (!iniData.Sections.ContainsSection("global")) iniData.Sections.AddSection("global");
            var global = iniData["global"];

            foreach (var property in config._metaData)
            {
                // Remove the old value and only set the new one if it isn't the default value
                global.RemoveKey(property.Key);
                if (!Equals(property.Value.DefaultValue, property.Value.Value))
                    global.AddKey(property.Key, property.Value.Value);
            }
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Config"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Config"/>.</returns>
        public Config CloneConfig()
        {
            return new Config {HelpWithTesting = HelpWithTesting, Freshness = Freshness, NetworkUse = NetworkUse, FeedMirror = FeedMirror, KeyInfoServer = KeyInfoServer, AutoApproveKeys = AutoApproveKeys};
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Config"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Config"/>.</returns>
        public object Clone()
        {
            return CloneConfig();
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
                builder.AppendLine(property.Key + " = " + property.Value.Value);
            return builder.ToString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Config other)
        {
            if (other == null) return false;

            return other.HelpWithTesting == HelpWithTesting && other.Freshness == Freshness && other.NetworkUse == NetworkUse && other.FeedMirror == FeedMirror && other.KeyInfoServer == KeyInfoServer && other.AutoApproveKeys == AutoApproveKeys;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Config) && Equals((Config)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = NetworkUse.GetHashCode();
                result = (result * 397) ^ HelpWithTesting.GetHashCode();
                result = (result * 397) ^ Freshness.GetHashCode();
                result = (result * 397) ^ NetworkUse.GetHashCode();
                result = (result * 397) ^ (FeedMirror != null ? FeedMirror.GetHashCode() : 0);
                result = (result * 397) ^ (KeyInfoServer != null ? KeyInfoServer.GetHashCode() : 0);
                result = (result * 397) ^ AutoApproveKeys.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
