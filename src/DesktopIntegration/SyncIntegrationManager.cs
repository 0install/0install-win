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
using Common.Collections;
using Common.Storage;
using Common.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{

    #region Enumerations
    /// <summary>
    /// Controls how synchronization data is reset.
    /// </summary>
    /// <seealso cref="SyncIntegrationManager.Sync"/>
    public enum SyncResetMode
    {
        /// <summary>Merge data from client and server normally.</summary>
        None,

        /// <summary>Replace all data on the client with data from the server.</summary>
        Client,

        /// <summary>Replace all data on the server with data from the client.</summary>
        Server
    }
    #endregion

    /// <summary>
    /// Synchronizes the <see cref="AppList"/> with other computers.
    /// </summary>
    /// <remarks>
    /// To prevent raceconditions there may only be one desktop integration class active at any given time.
    /// This class aquires a mutex upon calling its constructor and releases it upon calling <see cref="IntegrationManager.Dispose()"/>.
    /// </remarks>
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
        /// <param name="systemWide">Apply operations system-wide instead of just for the current user.</param>
        /// <param name="syncServer">The base URL of the sync server.</param>
        /// <param name="username">The username to authenticate with against the <paramref name="syncServer"/>.</param>
        /// <param name="password">The password to authenticate with against the <paramref name="syncServer"/>.</param>
        /// <param name="cryptoKey">The local key used to encrypt data before sending it to the <paramref name="syncServer"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted or if another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public SyncIntegrationManager(bool systemWide, Uri syncServer, string username, string password, string cryptoKey, ITaskHandler handler)
            : base(systemWide, handler)
        {
            #region Sanity checks
            if (syncServer == null) throw new ArgumentNullException("syncServer");
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

        #region Sync
        /// <summary>
        /// Synchronize the <see cref="AppList"/> with the sync server and (un)apply <see cref="AccessPoint"/>s accordingly.
        /// </summary>
        /// <param name="resetMode">Controls how synchronization data is reset.</param>
        /// <param name="feedRetreiver">Callback method used to retreive additional <see cref="Feed"/>s on demand.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as uploads and downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data or if the specified crypto key was wrong.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void Sync(SyncResetMode resetMode, Converter<string, Feed> feedRetreiver, ITaskHandler handler)
        {
            #region Sanity checks
            if (feedRetreiver == null) throw new ArgumentNullException("feedRetreiver");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var appListUri = new Uri(_syncServer, new Uri("app-list", UriKind.Relative));
            var webClient = new WebClient {Credentials = new NetworkCredential(_username, _password)};

            // Download and merge the current AppList from the server (unless the server is to be reset)
            var appListData = (resetMode == SyncResetMode.Server)
                ? new byte[0]
                // ToDo: Use ITask to allow for cancellation
                : webClient.DownloadData(appListUri);

            if (appListData.Length > 0)
            {
                AppList serverList;
                try
                {
                    serverList = XmlStorage.FromZip<AppList>(new MemoryStream(appListData), _cryptoKey, null);
                }
                    #region Error handling
                catch (ZipException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new InvalidDataException(ex.Message, ex);
                }
                #endregion

                MergeData(serverList, (resetMode == SyncResetMode.Client), feedRetreiver, handler);
                Complete();
            }

            // Upload the encrypted AppList back to the server (unless the client was reset)
            if (resetMode != SyncResetMode.Client)
            {
                var memoryStream = new MemoryStream();
                XmlStorage.ToZip(memoryStream, AppList, _cryptoKey, null);
                // ToDo: Use ITask to allow for cancellation
                webClient.UploadData(appListUri, "PUT", memoryStream.ToArray());
            }

            // Save reference point for future syncs
            AppList.Save(AppListPath + AppListLastSyncSuffix);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Merges a new <see cref="AppList"/> with the existing data.
        /// </summary>
        /// <param name="newData">The new <see cref="AppList"/> to merge in.</param>
        /// <param name="resetClient">Set to <see langword="true"/> to completly replace the contents of <see cref="IntegrationManager.AppList"/> with <paramref name="newData"/> instead of merging the two.</param>
        /// <param name="feedRetreiver">Callback method used to retreive additional <see cref="Feed"/>s on demand.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="InvalidOperationException">Thrown if one or more new <see cref="AccessPoint"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        /// <remarks>Performs a three-way merge using <see cref="_appListLastSync"/> as base.</remarks>
        private void MergeData(AppList newData, bool resetClient, Converter<string, Feed> feedRetreiver, ITaskHandler handler)
        {
            #region Sanity checks
            if (newData == null) throw new ArgumentNullException("newData");
            if (feedRetreiver == null) throw new ArgumentNullException("feedRetreiver");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            ICollection<AppEntry> toAdd = new LinkedList<AppEntry>();
            ICollection<AppEntry> toRemove = new LinkedList<AppEntry>();
            EnumerableUtils.Merge((resetClient ? AppList : _appListLastSync).Entries, newData.Entries, AppList.Entries, toAdd.Add, toRemove.Add);

            foreach (var appEntry in toRemove)
                RemoveAppEntry(appEntry);

            foreach (var appEntry in toAdd)
            {
                // Clone the AppEntry without the access points
                var newAppEntry = new AppEntry {InterfaceID = appEntry.InterfaceID, Name = appEntry.Name, AutoUpdate = appEntry.AutoUpdate, Timestamp = DateTime.UtcNow};
                foreach (var capabilityList in appEntry.CapabilityLists)
                    newAppEntry.CapabilityLists.Add(capabilityList.CloneCapabilityList());
                AppList.Entries.Add(newAppEntry);

                // Add and apply the access points
                AddAccessPoints(appEntry.AccessPoints.Entries, newAppEntry, new InterfaceFeed(appEntry.InterfaceID, feedRetreiver(appEntry.InterfaceID)));
            }
        }
        #endregion
    }
}
