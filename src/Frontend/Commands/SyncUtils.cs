/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Helper methods for creating instances of <see cref="SyncIntegrationManager"/>.
    /// </summary>
    public static class SyncUtils
    {
        /// <summary>
        /// Reads the relevant information from a <see cref="Config"/> in order to construct a <see cref="SyncServer"/> struct.
        /// </summary>
        public static SyncServer ToSyncServer([NotNull] this Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            return new SyncServer {Uri = config.SyncServer, Username = config.SyncServerUsername, Password = config.SyncServerPassword};
        }

        /// <summary>
        /// Writes the data of a <see cref="SyncServer"/> struct back to a <see cref="Config"/>.
        /// </summary>
        public static void FromSyncServer([NotNull] this Config config, SyncServer syncServer)
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
