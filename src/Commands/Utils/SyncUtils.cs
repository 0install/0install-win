// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using JetBrains.Annotations;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.Utils
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
            if (config == null) throw new ArgumentNullException(nameof(config));
            #endregion

            return new SyncServer {Uri = config.SyncServer, Username = config.SyncServerUsername, Password = config.SyncServerPassword};
        }

        /// <summary>
        /// Writes the data of a <see cref="SyncServer"/> struct back to a <see cref="Config"/>.
        /// </summary>
        public static void FromSyncServer([NotNull] this Config config, SyncServer syncServer)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException(nameof(config));
            #endregion

            config.SyncServer = syncServer.Uri;
            config.SyncServerUsername = syncServer.Username;
            config.SyncServerPassword = syncServer.Password;
        }
    }
}
