/*
 * Copyright 2010-2013 Bastian Eicher
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
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Store;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Helper methods for creating instances of <see cref="SyncIntegrationManager"/>.
    /// </summary>
    public static class SyncUtils
    {
        /// <summary>
        /// Creates a new <see cref="SyncIntegrationManager"/> using the default configuration.
        /// </summary>
        /// <param name="resolver">The source for configuration information and feed retrieval.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <returns>A new <see cref="SyncIntegrationManager"/> instance.</returns>
        public static SyncIntegrationManager CreateSync(Resolver resolver, bool machineWide)
        {
            #region Sanity checks
            if (resolver == null) throw new ArgumentNullException("resolver");
            #endregion

            return new SyncIntegrationManager(machineWide, ToSyncServer(resolver.Config), resolver.Config.SyncCryptoKey,
                feedID => resolver.FeedManager.GetFeed(feedID),
                resolver.Handler);
        }

        /// <summary>
        /// Creates a new <see cref="SyncIntegrationManager"/> using a custom crypto key.
        /// </summary>
        /// <param name="resolver">The source for configuration information and feed retrieval.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <param name="cryptoKey">The crypto key to use; overrides <see cref="Config.SyncCryptoKey"/>.</param>
        /// <returns>A new <see cref="SyncIntegrationManager"/> instance.</returns>
        public static SyncIntegrationManager CreateSync(Resolver resolver, bool machineWide, string cryptoKey)
        {
            #region Sanity checks
            if (resolver == null) throw new ArgumentNullException("resolver");
            #endregion

            return new SyncIntegrationManager(machineWide, ToSyncServer(resolver.Config), cryptoKey,
                feedID => resolver.FeedManager.GetFeed(feedID),
                resolver.Handler);
        }

        /// <summary>
        /// Creates a new <see cref="SyncIntegrationManager"/> using a custom server and credentials.
        /// </summary>
        /// <param name="resolver">The source for configuration information and feed retrieval.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <param name="server">Access information for the sync server; overrides <see cref="Config"/>.</param>
        /// <param name="cryptoKey">The crypto key to use; overrides <see cref="Config.SyncCryptoKey"/>; overrides <see cref="Config.SyncCryptoKey"/>.</param>
        /// <returns>A new <see cref="SyncIntegrationManager"/> instance.</returns>
        public static SyncIntegrationManager CreateSync(Resolver resolver, bool machineWide, SyncServer server, string cryptoKey)
        {
            #region Sanity checks
            if (resolver == null) throw new ArgumentNullException("resolver");
            #endregion

            return new SyncIntegrationManager(machineWide, server, cryptoKey,
                feedID => resolver.FeedManager.GetFeed(feedID),
                resolver.Handler);
        }

        /// <summary>
        /// Reads the relevant information from a <see cref="Config"/> in order to construct a <see cref="SyncServer"/> struct.
        /// </summary>
        public static SyncServer ToSyncServer(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            return new SyncServer {Uri = config.SyncServer, Username = config.SyncServerUsername, Password = config.SyncServerPassword};
        }

        /// <summary>
        /// Writes the data of a <see cref="SyncServer"/> struct back to a <see cref="Config"/>.
        /// </summary>
        public static void ToConfig(SyncServer syncServer, Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            config.SyncServer = syncServer.Uri;
            config.SyncServerUsername = syncServer.Username;
            config.SyncServerPassword = syncServer.Password;
        }
    }
}
