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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Runtime.Serialization;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

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
    /// To prevent raceconditions there may only be one desktop integration class instance active at any given time.
    /// This class acquires a mutex upon calling its constructor and releases it upon calling <see cref="IDisposable.Dispose"/>.
    /// </remarks>
    public class SyncIntegrationManager : IntegrationManager
    {
        #region Constants
        /// <summary>
        /// The suffix added to the <see cref="AppList"/> path to store a copy of the state at the last sync point.
        /// </summary>
        public const string AppListLastSyncSuffix = ".last-sync";

        /// <summary>
        /// The public key for HTTPS connections to the default sync server.
        /// </summary>
        public const string DefaultServerPublicKey = "B0041A5ED34423197E295A82557C5FE24A8593304C4F95EB178C51553BBDE65CC7D29DB900515A1885B591DF86909E3E9D7BA40B49E0E4982B61C7150A59135548B370DCB4EE7986630807A3B92908766D55383F9E97B1B9375056A6C1B6A4E65C876FC6313C8F8C5F8EEE65915DAC9EB623C80807979ABB431606A8363A9DEAE37C871A334D4A84A970B156E42B5E7A21FA4A7C6635CC86C2FD0AD6ED428D1BF155994D1EC953771F906E5C3026DA223F05F61C17313362FC47C9B40EC077575DF0DB18A6BBD1FC3392BB8EAAE066BBC062B1E47D68BD0FD6A34651ED3B5E458DD644BF9FE4EB734B32427D16ED8E0BE1DD93F2E2924C2DAC510C9C795250FB";
        #endregion

        #region Variables
        /// <summary>Access information for the sync server..</summary>
        private readonly SyncServer _server;

        /// <summary>The local key used to encrypt data before sending it to the <see cref="_server"/>.</summary>
        private readonly string _cryptoKey;

        /// <summary>Callback method used to retrieve additional <see cref="Feed"/>s on demand.</summary>
        private readonly Converter<FeedUri, Feed> _feedRetriever;

        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        private readonly AppList _appListLastSync;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new sync manager. Performs Mutex-based locking!
        /// </summary>
        /// <param name="server">Access information for the sync server.</param>
        /// <param name="cryptoKey">The local key used to encrypt data before sending it to the <paramref name="server"/>.</param>
        /// <param name="feedRetriever">Callback method used to retrieve additional <see cref="Feed"/>s on demand.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">A problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to the <see cref="AppList"/> file is not permitted or if another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        public SyncIntegrationManager(SyncServer server, [CanBeNull] string cryptoKey, [NotNull] Converter<FeedUri, Feed> feedRetriever, [NotNull] ITaskHandler handler, bool machineWide = false)
            : base(handler, machineWide)
        {
            #region Sanity checks
            if (server.Uri == null) throw new ArgumentNullException("server");
            if (feedRetriever == null) throw new ArgumentNullException("feedRetriever");
            #endregion

            _server = server;
            _cryptoKey = cryptoKey;
            _feedRetriever = feedRetriever;

            if (File.Exists(AppListPath + AppListLastSyncSuffix)) _appListLastSync = XmlStorage.LoadXml<AppList>(AppListPath + AppListLastSyncSuffix);
            else
            {
                _appListLastSync = new AppList();
                _appListLastSync.SaveXml(AppListPath + AppListLastSyncSuffix);
            }
        }

        /// <summary>
        /// Creates a new sync manager for a custom <see cref="AppList"/> file. Used for testing. Uses no mutex!
        /// </summary>
        /// <param name="appListPath">The storage location of the <see cref="AppList"/> file.</param>
        /// <param name="server">Access information for the sync server.</param>
        /// <param name="feedRetriever">Callback method used to retrieve additional <see cref="Feed"/>s on demand.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">A problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to the <see cref="AppList"/> file is not permitted or if another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        public SyncIntegrationManager([NotNull] string appListPath, SyncServer server, [NotNull] Converter<FeedUri, Feed> feedRetriever, [NotNull] ITaskHandler handler, bool machineWide = false)
            : base(appListPath, handler, machineWide)
        {
            #region Sanity checks
            if (server.Uri == null) throw new ArgumentNullException("server");
            if (feedRetriever == null) throw new ArgumentNullException("feedRetriever");
            #endregion

            _server = server;
            _feedRetriever = feedRetriever;

            if (File.Exists(AppListPath + AppListLastSyncSuffix)) _appListLastSync = XmlStorage.LoadXml<AppList>(AppListPath + AppListLastSyncSuffix);
            else
            {
                _appListLastSync = new AppList();
                _appListLastSync.SaveXml(AppListPath + AppListLastSyncSuffix);
            }
        }
        #endregion

        //--------------------//

        #region Sync
        /// <summary>
        /// Synchronize the <see cref="AppList"/> with the sync server and (un)apply <see cref="AccessPoint"/>s accordingly.
        /// </summary>
        /// <param name="resetMode">Controls how synchronization data is reset.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="InvalidDataException">A problem occurred while deserializing the XML data or if the specified crypto key was wrong.</exception>
        /// <exception cref="WebException">A problem occured while communicating with the sync server or while downloading additional data (such as icons).</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        public void Sync(SyncResetMode resetMode = SyncResetMode.None)
        {
            if (!_server.IsValid) throw new InvalidDataException(Resources.PleaseConfigSync);

            var appListUri = new Uri(_server.Uri, new Uri(MachineWide ? "app-list-machine" : "app-list", UriKind.Relative));
            using (var webClient = new WebClientTimeout
            {
                Credentials = _server.Credentials,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
            })
            {
                ExceptionUtils.Retry<WebRaceConditionException>(delegate
                {
                    Handler.CancellationToken.ThrowIfCancellationRequested();

                    byte[] appListData;
                    try
                    {
                        appListData = DownloadAppList(appListUri, webClient, resetMode);
                    }
                        #region Error handling
                    catch (WebException ex)
                    {
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            var response = ex.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                                throw new WebException(Resources.SyncCredentialsInvalid, ex, ex.Status, ex.Response);
                        }
                        throw;
                    }
                    #endregion

                    HandleDownloadedAppList(resetMode, appListData);

                    try
                    {
                        UploadAppList(appListUri, webClient, resetMode);
                    }
                        #region Error handling
                    catch (WebException ex)
                    {
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            var response = ex.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.PreconditionFailed)
                            {
                                Handler.CancellationToken.ThrowIfCancellationRequested();
                                throw new WebRaceConditionException(ex);
                            }
                        }
                        else throw;
                    }
                    #endregion
                }, maxRetries: 3);
            }

            // Save reference point for future syncs
            AppList.SaveXml(AppListPath + AppListLastSyncSuffix);

            Handler.CancellationToken.ThrowIfCancellationRequested();
        }

        [Serializable]
        private class WebRaceConditionException : WebException
        {
            public WebRaceConditionException(WebException ex)
                : base("Race condition: Multiple computers are trying to Sync to the same account at the same time.", ex, ex.Status, ex.Response)
            {}


            #region Serialization
            protected WebRaceConditionException([NotNull] SerializationInfo serializationInfo, StreamingContext streamingContext)
                : base(serializationInfo, streamingContext)
            {}
            #endregion
        }
        #endregion

        #region Helpers
        [NotNull]
        private byte[] DownloadAppList([NotNull] Uri appListUri, [NotNull] WebClientTimeout webClient, SyncResetMode resetMode)
        {
            if (resetMode == SyncResetMode.Server) return new byte[0];

            byte[] data = null;
            Handler.RunTask(new SimpleTask(Resources.SyncDownloading, () => { data = webClient.DownloadData(appListUri); }));
            return data;
        }

        private void HandleDownloadedAppList(SyncResetMode resetMode, [NotNull] byte[] appListData)
        {
            if (appListData.Length == 0) return;

            AppList serverList;
            try
            {
                serverList = AppList.LoadXmlZip(new MemoryStream(appListData), _cryptoKey);
            }
                #region Error handling
            catch (ZipException ex)
            {
                // Wrap exception to add context information
                if (ex.Message == "Invalid password") throw new InvalidDataException(Resources.SyncCryptoKeyInvalid);
                throw new InvalidDataException(Resources.SyncServerDataDamaged, ex);
            }
            #endregion

            Handler.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                MergeData(serverList, resetClient: (resetMode == SyncResetMode.Client));
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
            }

            Handler.CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Upload the encrypted AppList back to the server (unless the client was reset)
        /// </summary>
        private void UploadAppList([NotNull] Uri appListUri, [NotNull] WebClientTimeout webClient, SyncResetMode resetMode)
        {
            if (resetMode == SyncResetMode.Client) return;

            var memoryStream = new MemoryStream();
            AppList.SaveXmlZip(memoryStream, _cryptoKey);

            // Prevent "lost updates" (race conditions) with HTTP ETags
            if (resetMode == SyncResetMode.None && (appListUri.Scheme == "http" || appListUri.Scheme == "https"))
            {
                if (!string.IsNullOrEmpty(webClient.ResponseHeaders[HttpResponseHeader.ETag]))
                    webClient.Headers[HttpRequestHeader.IfMatch] = webClient.ResponseHeaders[HttpResponseHeader.ETag];
            }

            Handler.RunTask(new SimpleTask(Resources.SyncUploading, () => webClient.UploadData(appListUri, "PUT", memoryStream.ToArray())));
        }
        #endregion

        #region Merge data
        /// <summary>
        /// Merges a new <see cref="IntegrationManagerBase.AppList"/> with the existing data.
        /// </summary>
        /// <param name="remoteAppList">The remote <see cref="AppList"/> to merge in.</param>
        /// <param name="resetClient">Set to <c>true</c> to completly replace the contents of <see cref="IIntegrationManager.AppList"/> with <paramref name="remoteAppList"/> instead of merging the two.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="KeyNotFoundException">An <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="InvalidDataException">One of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="ConflictException">One or more new <see cref="AccessPoint"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <remarks>Performs a three-way merge using <see cref="_appListLastSync"/> as base.</remarks>
        private void MergeData([NotNull] AppList remoteAppList, bool resetClient)
        {
            #region Sanity checks
            if (remoteAppList == null) throw new ArgumentNullException("remoteAppList");
            #endregion

            var toAdd = new List<AppEntry>();
            var toRemove = new List<AppEntry>();

            if (resetClient)
            {
                Merge.TwoWay(
                    theirs: remoteAppList.Entries,
                    mine: AppList.Entries,
                    added: toAdd, removed: toRemove);
            }
            else
            {
                Merge.ThreeWay(
                    reference: _appListLastSync.Entries,
                    theirs: remoteAppList.Entries,
                    mine: AppList.Entries,
                    added: toAdd, removed: toRemove);
            }

            Handler.RunTask(new SimpleTask(Resources.ApplyingChanges, () =>
            {
                toRemove.ApplyWithRollback(RemoveAppInternal, AddAppHelper);
                toAdd.ApplyWithRollback(AddAppHelper, RemoveAppInternal);
            }));
        }

        /// <summary>
        /// Creates a new <see cref="AppEntry"/> based on an existing prototype (applying any <see cref="AccessPoint"/>s) and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="prototype">An existing <see cref="AppEntry"/> to use as a prototype.</param>
        private void AddAppHelper([NotNull] AppEntry prototype)
        {
            AddAppInternal(prototype, _feedRetriever);
        }
        #endregion
    }
}
