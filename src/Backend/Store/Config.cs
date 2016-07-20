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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using NanoByte.Common;
using NanoByte.Common.Net;

namespace ZeroInstall.Store
{
    /// <summary>
    /// User settings controlling network behaviour, solving, etc.
    /// </summary>
    [Serializable]
    public sealed partial class Config : ICloneable, IEquatable<Config>
    {
        private static readonly TimeSpan _defaultFreshness = new TimeSpan(7, 0, 0, 0, 0); // 7 days
        private TimeSpan _freshness = _defaultFreshness;

        /// <summary>
        /// The maximum age a cached <see cref="Store.Model.Feed"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        [DefaultValue(typeof(TimeSpan), "7.00:00:00"), Category("Policy"), DisplayName(@"Freshness"), Description("The maximum age a cached feed may have until it is considered stale (needs to be updated).")]
        public TimeSpan Freshness { get { return _freshness; } set { _freshness = value; } }

        /// <summary>
        /// Always prefer the newest versions, even if they have not been marked as <see cref="Store.Model.Stability.Stable"/> yet.
        /// </summary>
        [DefaultValue(false), Category("Policy"), DisplayName(@"Help with testing"), Description("Always prefer the newest versions, even if they have not been marked as stable yet.")]
        public bool HelpWithTesting { get; set; }

        private NetworkLevel _networkLevel = NetworkLevel.Full;

        /// <summary>
        /// Controls how liberally network access is attempted.
        /// </summary>
        [DefaultValue(typeof(NetworkLevel), "Full"), Category("Policy"), DisplayName(@"Network use"), Description("Controls how liberally network access is attempted.")]
        public NetworkLevel NetworkUse
        {
            get { return _networkLevel; }
            set
            {
                #region Sanity checks
                if (!Enum.IsDefined(typeof(NetworkLevel), value)) throw new ArgumentOutOfRangeException(nameof(value));
                #endregion

                _networkLevel = value;
            }
        }

        private bool _autoApproveKeys = true;

        /// <summary>
        /// Automatically approve keys known by the <see cref="KeyInfoServer"/> and seen the first time a feed is fetched.
        /// </summary>
        [DefaultValue(true), Category("Policy"), DisplayName(@"Auto approve keys"), Description("Automatically approve keys known by the key info server and seen the first time a feed is fetched.")]
        public bool AutoApproveKeys { get { return _autoApproveKeys; } set { _autoApproveKeys = value; } }

        private bool _allowApiHooking;

        /// <summary>
        /// WARNING! This feature is highly experimental!<br/>
        /// Controls whether Zero Install may install hooks for operating sytem APIs to improve desktop integration.
        /// </summary>
        [DefaultValue(false), Category("Policy"), DisplayName(@"Allow API hooking"), Description("WARNING! This feature is highly experimental!\r\nControls whether Zero Install may install hooks for operating sytem APIs to improve desktop integration.")]
        public bool AllowApiHooking { get { return _allowApiHooking; } set { _allowApiHooking = value; } }

        /// <summary>
        /// The default value for <see cref="FeedMirror"/>.
        /// </summary>
        public const string DefaultFeedMirror = "http://roscidus.com/0mirror";

        private Uri _feedMirror = new Uri(DefaultFeedMirror);

        /// <summary>
        /// The mirror server used to provide feeds when the original server is unavailable.
        /// </summary>
        [DefaultValue(typeof(Uri), DefaultFeedMirror), Category("Sources"), DisplayName(@"Feed mirror"), Description("The mirror server used to provide feeds when the original server is unavailable.")]
        public Uri FeedMirror { get { return _feedMirror; } set { _feedMirror = value?.ReparseAsAbsolute(); } }

        /// <summary>
        /// The default value for <see cref="KeyInfoServer"/>.
        /// </summary>
        public const string DefaultKeyInfoServer = "https://keylookup.appspot.com/";

        private Uri _keyInfoServer = new Uri(DefaultKeyInfoServer);

        /// <summary>
        /// The key information server used to get information about who signed a feed.
        /// </summary>
        [DefaultValue(typeof(Uri), DefaultKeyInfoServer), Category("Sources"), DisplayName(@"Key info server"), Description("The key information server used to get information about who signed a feed.")]
        public Uri KeyInfoServer { get { return _keyInfoServer; } set { _keyInfoServer = value?.ReparseAsAbsolute(); } }

        /// <summary>
        /// The default value for <see cref="SelfUpdateUri"/>.
        /// </summary>
        public const string DefaultSelfUpdateUri = "http://0install.de/feeds/ZeroInstall.xml";

        private FeedUri _selfUpdateUri = new FeedUri(DefaultSelfUpdateUri);

        /// <summary>
        /// The feed URI used by the solver to search for updates for Zero Install itself.
        /// </summary>
        [DefaultValue(typeof(FeedUri), DefaultSelfUpdateUri), Category("Sources"), DisplayName(@"Self-update URI"), Description("The feed URI used by the solver to search for updates for Zero Install itself.")]
        public FeedUri SelfUpdateUri { get { return _selfUpdateUri; } set { _selfUpdateUri = value; } }

        /// <summary>
        /// The default value for <see cref="ExternalSolverUri"/>.
        /// </summary>
        public const string DefaultExternalSolverUri = "http://0install.net/tools/0install.xml";

        private FeedUri _externalSolverUri = new FeedUri(DefaultExternalSolverUri);

        /// <summary>
        /// The feed URI used to get the external solver.
        /// </summary>
        [DefaultValue(typeof(FeedUri), DefaultExternalSolverUri), Category("Sources"), DisplayName(@"External Solver URI"), Description("The feed URI used to get the external solver.")]
        public FeedUri ExternalSolverUri { get { return _externalSolverUri; } set { _externalSolverUri = value; } }

        /// <summary>
        /// The default value for <see cref="SyncServer"/>.
        /// </summary>
        public const string DefaultSyncServer = "https://0install.de/sync/";

        private Uri _syncServer = new Uri(DefaultSyncServer);

        /// <summary>
        /// The sync server used to synchronize your app list between multiple computers.
        /// </summary>
        /// <seealso cref="SyncServerUsername"/>
        /// <seealso cref="SyncServerPassword"/>
        [DefaultValue(typeof(Uri), DefaultSyncServer), Category("Sync"), DisplayName(@"Server"), Description("The sync server used to synchronize your app list between multiple computers.")]
        public Uri SyncServer { get { return _syncServer; } set { _syncServer = value?.ReparseAsAbsolute(); } }

        private string _syncServerUsername = "";

        /// <summary>
        /// The username to authenticate with against the <see cref="SyncServer"/>.
        /// </summary>
        /// <seealso cref="SyncServer"/>
        /// <seealso cref="SyncServerPassword"/>
        [DefaultValue(""), Category("Sync"), DisplayName(@"Username"), Description("The username to authenticate with against the Sync server.")]
        public string SyncServerUsername { get { return _syncServerUsername; } set { _syncServerUsername = value; } }

        private string _syncServerPassword = "";

        /// <summary>
        /// The password to authenticate with against the <see cref="SyncServer"/>.
        /// </summary>
        /// <seealso cref="SyncServer"/>
        /// <seealso cref="SyncServerUsername"/>
        [DefaultValue(""), PasswordPropertyText(true), Category("Sync"), DisplayName(@"Password"), Description("The password to authenticate with against the Sync server.")]
        public string SyncServerPassword { get { return _syncServerPassword; } set { _syncServerPassword = value; } }

        private string _syncCryptoKey = "";

        /// <summary>
        /// The local key used to encrypt data before sending it to the <see cref="SyncServer"/>.
        /// </summary>
        [DefaultValue(""), PasswordPropertyText(true), Category("Sync"), DisplayName(@"Crypto key"), Description("The local key used to encrypt data before sending it to the Sync server.")]
        public string SyncCryptoKey { get { return _syncCryptoKey; } set { _syncCryptoKey = value; } }

        /// <summary>Provides meta-data for loading and saving settings properties.</summary>
        private readonly Dictionary<string, PropertyPointer<string>> _metaData;

        /// <summary>
        /// Creates a new configuration with default values set.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Key-value dispatcher")]
        public Config()
        {
            _metaData = new Dictionary<string, PropertyPointer<string>>
            {
                {"freshness", new PropertyPointer<TimeSpan>(() => Freshness, value => Freshness = value, defaultValue: _defaultFreshness).ToStringPointer()},
                {"help_with_testing", new PropertyPointer<bool>(() => HelpWithTesting, value => HelpWithTesting = value).ToStringPointer()},
                {"network_use", GetNetworkUseConverter()},
                {"auto_approve_keys", new PropertyPointer<bool>(() => AutoApproveKeys, value => AutoApproveKeys = value, defaultValue: true).ToStringPointer()},
                {"allow_api_hooking", new PropertyPointer<bool>(() => AllowApiHooking, value => AllowApiHooking = value).ToStringPointer()},
                {"feed_mirror", new PropertyPointer<Uri>(() => FeedMirror, value => FeedMirror = value, defaultValue: new Uri(DefaultFeedMirror)).ToStringPointer()},
                {"key_info_server", new PropertyPointer<Uri>(() => KeyInfoServer, value => KeyInfoServer = value, defaultValue: new Uri(DefaultKeyInfoServer)).ToStringPointer()},
                {"self_update_uri", new PropertyPointer<FeedUri>(() => SelfUpdateUri, value => SelfUpdateUri = value, defaultValue: new FeedUri(DefaultSelfUpdateUri)).ToStringPointer()},
                {"external_solver_uri", new PropertyPointer<FeedUri>(() => ExternalSolverUri, value => ExternalSolverUri = value, defaultValue: new FeedUri(DefaultExternalSolverUri)).ToStringPointer()},
                {"sync_server", new PropertyPointer<Uri>(() => SyncServer, value => SyncServer = value, defaultValue: new Uri(DefaultSyncServer)).ToStringPointer()},
                {"sync_server_user", new PropertyPointer<string>(() => SyncServerUsername, value => SyncServerUsername = value, defaultValue: "")},
                {"sync_server_pw", new PropertyPointer<string>(() => SyncServerPassword, value => SyncServerPassword = value, defaultValue: "", needsEncoding: true)},
                {"sync_crypto_key", new PropertyPointer<string>(() => SyncCryptoKey, value => SyncCryptoKey = value, defaultValue: "", needsEncoding: true)},
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
    }
}
