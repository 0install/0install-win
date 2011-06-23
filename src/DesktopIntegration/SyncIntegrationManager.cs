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
using System.IO;
using System.Net;
using Common;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;
using Capability = ZeroInstall.Model.Capabilities.Capability;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Synchronizes the <see cref="AppList"/> with other computers.
    /// </summary>
    public class SyncIntegrationManager : IntegrationManager
    {
        #region Constants
        private const string AppListLastSyncSuffix = ".last-sync";
        #endregion

        #region Variables
        /// <summary>The base URL of the sync server.</summary>
        private readonly Uri _syncServer;

        /// <summary>The username to authenticate with against the <see cref="_syncServer"/>.</summary>
        private readonly string _username;

        /// <summary>The password to authenticate with against the <see cref="_syncServer"/>.</summary>
        private readonly string _password;

        /// <summary>The local key used to encrypt data before sending it to the <see cref="_syncServer"/>.</summary>
        private readonly string _cryptoKey;

        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        private readonly AppList _appListLastSync;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new sync manager.
        /// </summary>
        /// <param name="syncServer">The base URL of the sync server.</param>
        /// <param name="username">The username to authenticate with against the <paramref name="syncServer"/>.</param>
        /// <param name="password">The password to authenticate with against the <paramref name="syncServer"/>.</param>
        /// <param name="cryptoKey">The local key used to encrypt data before sending it to the <paramref name="syncServer"/>.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public SyncIntegrationManager(Uri syncServer, string username, string password, string cryptoKey) : base(false)
        {
            #region Sanity checks
            if (syncServer == null) throw new ArgumentNullException("syncServer");
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if (password == null) throw new ArgumentNullException("password");
            if (cryptoKey == null) throw new ArgumentNullException("cryptoKey");
            #endregion

            _syncServer = syncServer;
            _username = username;
            _password = password;
            _cryptoKey = cryptoKey;

            if (File.Exists(AppListPath + AppListLastSyncSuffix)) _appListLastSync = AppList.Load(AppListPath + AppListLastSyncSuffix);
            else
            {
                _appListLastSync = new AppList();
                _appListLastSync.Save(AppListPath + AppListLastSyncSuffix);
            }
        }
        #endregion

        //--------------------//

        /// <summary>
        /// Synchronize the <see cref="AppList"/> with the sync server and (un)apply <see cref="AccessPoint"/>s accordingly.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as uploads and downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void Sync(ITaskHandler handler)
        {
            // ToDo: Implement
        }
    }
}
