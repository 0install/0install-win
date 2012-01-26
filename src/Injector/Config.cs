/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Threading;
using Common;
using Common.Storage;
using Common.Utils;
using Common.Values.Design;
using IniParser;
using ZeroInstall.Injector.Properties;

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
        private readonly C5.IDictionary<string, PropertyPointer<string>> _metaData;

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

        private static readonly TimeSpan _defaultFreshness = new TimeSpan(7, 0, 0, 0, 0); // 7 days
        private TimeSpan _freshness = _defaultFreshness;

        /// <summary>
        /// The maximum age a cached <see cref="Model.Feed"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        [DefaultValue(typeof(TimeSpan), "7.00:00:00"), DisplayName("Freshness"), Description("The maximum age a cached feed may have until it is considered stale (needs to be updated).")]
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
        public Uri FeedMirror { get { return _feedMirror; } set { _feedMirror = value; } }

        private const string DefaultKeyInfoServer = "https://keylookup.appspot.com";
        private Uri _keyInfoServer = new Uri(DefaultKeyInfoServer);

        /// <summary>
        /// The base URL of a key information server.
        /// </summary>
        [DefaultValue(typeof(Uri), DefaultKeyInfoServer), DisplayName("Key info server"), Description("The base URL of a key information server.")]
        public Uri KeyInfoServer { get { return _keyInfoServer; } set { _keyInfoServer = value; } }

        private bool _autoApproveKeys = true;

        /// <summary>
        /// Automatically approve keys known by the <see cref="KeyInfoServer"/> and seen the first time a feed is fetched.
        /// </summary>
        [DefaultValue(true), DisplayName("Auto approve keys"), Description("Automatically approve keys known by the key info server and seen the first time a feed is fetched.")]
        public bool AutoApproveKeys { get { return _autoApproveKeys; } set { _autoApproveKeys = value; } }

        private const string DefaultSelfUpdateID = "http://0install.de/feeds/ZeroInstall.xml";
        private string _selfUpdateID = DefaultSelfUpdateID;

        /// <summary>
        /// The ID used by the solver to search for updates for Zero Install itself.
        /// </summary>
        [DefaultValue(DefaultSelfUpdateID), DisplayName("Self-update ID"), Description("The ID used by the solver to search for updates for Zero Install itself.")]
        public string SelfUpdateID { get { return _selfUpdateID; } set { _selfUpdateID = value; } }

        private const string DefaultSyncServer = "https://0install.de/sync/";
        private Uri _syncServer = new Uri(DefaultSyncServer);

        /// <summary>
        /// The base URL of the sync server.
        /// </summary>
        /// <seealso cref="SyncServerUsername"/>
        /// <seealso cref="SyncServerPassword"/>
        [DefaultValue(typeof(Uri), DefaultSyncServer), DisplayName("Sync server"), Description("The base URL of the sync server.")]
        public Uri SyncServer { get { return _syncServer; } set { _syncServer = value; } }

        private string _syncServerUsername = "";

        /// <summary>
        /// The username to authenticate with against the <see cref="SyncServer"/>.
        /// </summary>
        /// <seealso cref="SyncServer"/>
        /// <seealso cref="SyncServerPassword"/>
        [DefaultValue(""), DisplayName("Sync server username"), Description("The username to authenticate with against the Sync server.")]
        public string SyncServerUsername { get { return _syncServerUsername; } set { _syncServerUsername = value; } }

        private string _syncServerPassword = "";

        /// <summary>
        /// The password to authenticate with against the <see cref="SyncServer"/>.
        /// </summary>
        /// <seealso cref="SyncServer"/>
        /// <seealso cref="SyncServerUsername"/>
        [DefaultValue(""), PasswordPropertyText(true), DisplayName("Sync server password"), Description("The password to authenticate with against the Sync server.")]
        public string SyncServerPassword { get { return _syncServerPassword; } set { _syncServerPassword = value; } }

        private string _syncCryptoKey = "";

        /// <summary>
        /// The local key used to encrypt data before sending it to the <see cref="SyncServer"/>.
        /// </summary>
        [DefaultValue(""), PasswordPropertyText(true), DisplayName("Sync crypto key"), Description("The local key used to encrypt data before sending it to the Sync server.")]
        public string SyncCryptoKey { get { return _syncCryptoKey; } set { _syncCryptoKey = value; } }

        private bool _allowApiHooking;

        /// <summary>
        /// Controls whether Zero Install may install hooks for operating sytem APIs to improve desktop integration.
        /// </summary>
        [DefaultValue(false), DisplayName("Allow API hooking"), Description("Controls whether Zero Install may install hooks for operating sytem APIs to improve desktop integration.")]
        public bool AllowApiHooking { get { return _allowApiHooking; } set { _allowApiHooking = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new configuration with default values set.
        /// </summary>
        public Config()
        {
            _metaData = new C5.TreeDictionary<string, PropertyPointer<string>>
            {
                {"help_with_testing", PropertyPointer.GetBoolConverter(new PropertyPointer<bool>(() => HelpWithTesting, value => HelpWithTesting = value, false))},
                {"freshness", PropertyPointer.GetTimespanConverter(new PropertyPointer<TimeSpan>(() => Freshness, value => Freshness = value, _defaultFreshness))},
                {"network_use", GetNetworkUseConverter()},
                {"feed_mirror", PropertyPointer.GetUriConverter(new PropertyPointer<Uri>(() => FeedMirror, value => FeedMirror = value, new Uri(DefaultFeedMirror)))},
                {"key_info_server", PropertyPointer.GetUriConverter(new PropertyPointer<Uri>(() => KeyInfoServer, value => KeyInfoServer = value, new Uri(DefaultKeyInfoServer)))},
                {"auto_approve_keys", PropertyPointer.GetBoolConverter(new PropertyPointer<bool>(() => AutoApproveKeys, value => AutoApproveKeys = value, true))},
                {"self_update_id", new PropertyPointer<string>(() => SelfUpdateID, value => SelfUpdateID = value, DefaultSelfUpdateID)},
                {"sync_server", PropertyPointer.GetUriConverter(new PropertyPointer<Uri>(() => SyncServer, value => SyncServer = value, new Uri(DefaultSyncServer)))},
                {"sync_server_user", new PropertyPointer<string>(() => SyncServerUsername, value => SyncServerUsername = value, "")},
                {"sync_server_pw", new PropertyPointer<string>(() => SyncServerPassword, value => SyncServerPassword = value, "", true)},
                {"sync_crypto_key", new PropertyPointer<string>(() => SyncCryptoKey, value => SyncCryptoKey = value, "", true)},
                {"allow_api_hooking", PropertyPointer.GetBoolConverter(new PropertyPointer<bool>(() => AllowApiHooking, value => AllowApiHooking = value, false))},
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
        /// <exception cref="C5.NoSuchItemException">Thrown if <paramref name="key"/> is invalid.</exception>
        public string GetOption(string key)
        {
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
            _metaData[key].Value = value;
        }

        /// <summary>
        /// Resets an option identified by a key to its default value.
        /// </summary>
        /// <param name="key">The key of the option to reset.</param>
        /// <exception cref="C5.NoSuchItemException">Thrown if <paramref name="key"/> is invalid.</exception>
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
            // Locate all applicable config files and order them from least to most important
            var paths = new List<string>(Locations.GetLoadConfigPaths("0install.net", true, "injector", "global"));
            paths.Reverse();

            // Accumulate values from all files
            var config = new Config();
            foreach (var path in paths)
                config.ReadFromIniFile(path);
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

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string tempPath = path + "." + Path.GetRandomFileName() + ".new";
            try
            {
                // Write to temporary file first
                _iniParse.SaveFile(tempPath, _iniData);
                FileUtils.Replace(tempPath, path);
            }
                #region Error handling
            catch (Exception)
            {
                // Clean up failed transactions
                if (File.Exists(tempPath)) File.Delete(tempPath);
                throw;
            }
            #endregion
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
        private static readonly Random _random = new Random();

        /// <summary>
        /// Reads data from an INI file on the disk and transfers it to properties using <see cref="_metaData"/>.
        /// </summary>
        private void ReadFromIniFile(string path)
        {
            try
            {
                _iniData = _iniParse.LoadFile(path, true);
            }
                #region Error handling
            catch (ParsingException)
            {
                // Wait a moment and then retry in case it was just a race condition
                Thread.Sleep(_random.Next(250, 750));
                try
                {
                    _iniData = _iniParse.LoadFile(path, true);
                }
                catch (ParsingException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new InvalidDataException(ex.Message);
                }
                Log.Info("Successfully handled race condition while loading config");
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
                        property.Value.Value = property.Value.NeedsEncoding ? StringUtils.Base64Decode(global[key]) : global[key];
                    }
                        #region Error handling
                    catch (FormatException ex)
                    {
                        // Wrap exception to add context information
                        throw new InvalidDataException(string.Format(Resources.ProblemLoadingConfig, property.Key), ex);
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
                    global.AddKey(key, property.Value.NeedsEncoding ? StringUtils.Base64Encode(property.Value.Value) : property.Value.Value);
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

            foreach (var property in _metaData)
                if (property.Value.Value != other.GetOption(property.Key)) return false;
            return true;
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
                int result = 397;
                foreach (var property in _metaData)
                {
                    // ReSharper disable NonReadonlyFieldInGetHashCode
                    if (property.Value.Value != null)
                    { // Use a commutative folding function (addition) since the order in a hash map is non-deterministic
                        result += property.Value.Value.GetHashCode();
                    }
                    // ReSharper restore NonReadonlyFieldInGetHashCode
                }
                return result;
            }
        }
        #endregion
    }
}
