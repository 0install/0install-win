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

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Helper methods for creating instances of <see cref="SyncIntegrationManager"/>.
    /// </summary>
    public static class SyncFactory
    {
        /// <summary>
        /// Creates a new <see cref="SyncIntegrationManager"/> using the default configuration.
        /// </summary>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <param name="policy">The source for configuration information and feed retrieval.</param>
        /// <returns>A new <see cref="SyncIntegrationManager"/> instance.</returns>
        public static SyncIntegrationManager Create(bool machineWide, Policy policy)
        {
            #region Sanity checks
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            return new SyncIntegrationManager(machineWide,
                policy.Config.SyncServer,
                policy.Config.SyncServerUsername,
                policy.Config.SyncServerPassword,
                policy.Config.SyncCryptoKey,
                feedID => policy.FeedManager.GetFeed(feedID, policy),
                policy.Handler);
        }

        /// <summary>
        /// Creates a new <see cref="SyncIntegrationManager"/> using a custom crypto key.
        /// </summary>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <param name="policy">The source for configuration information and feed retrieval.</param>
        /// <param name="cryptoKey">The crypto key to use; overrides <see cref="Config.SyncCryptoKey"/>.</param>
        /// <returns>A new <see cref="SyncIntegrationManager"/> instance.</returns>
        public static SyncIntegrationManager Create(bool machineWide, Policy policy, string cryptoKey)
        {
            #region Sanity checks
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            return new SyncIntegrationManager(machineWide,
                policy.Config.SyncServer,
                policy.Config.SyncServerUsername,
                policy.Config.SyncServerPassword,
                cryptoKey,
                feedID => policy.FeedManager.GetFeed(feedID, policy),
                policy.Handler);
        }
    }
}