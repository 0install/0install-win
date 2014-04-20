/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
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
    /// To prevent raceconditions there may only be one desktop integration class instance per machine active at any given time.
    /// This class acquires a mutex upon calling its constructor and releases it upon calling <see cref="IntegrationManager.Dispose()"/>.
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
        public const string DefaultServerPublicKey = "3082010A0282010100A04A36D32B480E4F178877DD190BD955F4480986DE3DE7DF4E5C925180C72D189138C1CA38AC0AFEFD95E839DC80E1F3ED02BEC326ACA25D81802DDEB5E6B4CF7E7905FBBD8BDEA367B43CC8790183B668B01EF1173500E15C739BACC6C0CCD2183620FF16915B50C640BCAF7E7EA4948608558B07D78DEB1C30B44A567BE489F04FD09B7C874D9FFA2F188168B1F55887DE9EE241D286E1B70A3A9E85C1B2AC1C0E58C0AAD59DBD78A252B6D1981ECA15747112D8B83C5C6C6983548F547D6087FB00D5CEC3B3E0DB64C7951225FCF74054F31CAA4BE8917C985DE2F27BE9865E60F6799F2A0EBFA019F87A4A6D73CE8A36867F69665CA54856DD45597683130203010001";
        #endregion

        #region Variables
        /// <summary>Access information for the sync server..</summary>
        private readonly SyncServer _server;

        /// <summary>The local key used to encrypt data before sending it to the <see cref="_server"/>.</summary>
        private readonly string _cryptoKey;

        /// <summary>Callback method used to retrieve additional <see cref="Feed"/>s on demand.</summary>
        private readonly Converter<string, Feed> _feedRetriever;

        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        private readonly AppList _appListLastSync;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new sync manager.
        /// </summary>
        /// <param name="server">Access information for the sync server.</param>
        /// <param name="cryptoKey">The local key used to encrypt data before sending it to the <paramref name="server"/>.</param>
        /// <param name="feedRetriever">Callback method used to retrieve additional <see cref="Feed"/>s on demand.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted or if another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public SyncIntegrationManager(SyncServer server, string cryptoKey, Converter<string, Feed> feedRetriever, ITaskHandler handler, bool machineWide = false)
            : base(handler, machineWide)
        {
            #region Sanity checks
            if (server.Uri == null) throw new ArgumentNullException("server");
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
        /// Creates a new sync manager for a custom <see cref="AppList"/> file. Used for testing.
        /// </summary>
        /// <param name="appListPath">The storage location of the <see cref="AppList"/> file.</param>
        /// <param name="server">Access information for the sync server.</param>
        /// <param name="feedRetriever">Callback method used to retrieve additional <see cref="Feed"/>s on demand.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted or if another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public SyncIntegrationManager(string appListPath, SyncServer server, Converter<string, Feed> feedRetriever, ITaskHandler handler, bool machineWide = false)
            : base(appListPath, handler, machineWide)
        {
            #region Sanity checks
            if (server.Uri == null) throw new ArgumentNullException("server");
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
        private static readonly Random _random = new Random();

        /// <summary>
        /// Synchronize the <see cref="AppList"/> with the sync server and (un)apply <see cref="AccessPoint"/>s accordingly.
        /// </summary>
        /// <param name="resetMode">Controls how synchronization data is reset.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data or if the specified crypto key was wrong.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void Sync(SyncResetMode resetMode)
        {
            var appListUri = new Uri(_server.Uri, new Uri(MachineWide ? "app-list-machine" : "app-list", UriKind.Relative));
            using (var webClient = new WebClientTimeout
            {
                Credentials = _server.Credentials,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
            })
            {
                bool timeoutRetryPerformed = false;
                Retry:
                byte[] appListData;
                try
                {
                    appListData = (resetMode == SyncResetMode.Server) ? new byte[0] : webClient.DownloadData(appListUri);
                }
                    #region Error handling
                catch (WebException ex)
                {
                    switch (ex.Status)
                    {
                        case WebExceptionStatus.ProtocolError:
                            // Wrap exception to add context information
                            var response = ex.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                                throw new WebException(Resources.SyncCredentialsInvalid, ex);
                            else throw;

                        case WebExceptionStatus.Timeout:
                            // In case of timeout retry exactly once
                            if (!timeoutRetryPerformed)
                            {
                                timeoutRetryPerformed = true;
                                Handler.CancellationToken.ThrowIfCancellationRequested();
                                goto Retry;
                            }
                            else throw;

                        default:
                            if (ex.InnerException != null && ex.InnerException.InnerException is FileNotFoundException) appListData = new byte[0];
                            else throw;
                            break;
                    }
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
                        { // Precondition failure indicates a "lost update" (race condition)
                            // Wait for a randomized interval before retrying
                            Thread.Sleep(_random.Next(250, 1500));
                            Handler.CancellationToken.ThrowIfCancellationRequested();
                            goto Retry;
                        }
                    }

                    throw;
                }
                #endregion
            }

            // Save reference point for future syncs
            AppList.SaveXml(AppListPath + AppListLastSyncSuffix);

            Handler.CancellationToken.ThrowIfCancellationRequested();
        }
        #endregion

        #region Helpers
        private void HandleDownloadedAppList(SyncResetMode resetMode, byte[] appListData)
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
                MergeData(serverList, Handler, resetClient: (resetMode == SyncResetMode.Client));
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
        private void UploadAppList(Uri appListUri, WebClientTimeout webClient, SyncResetMode resetMode)
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

            webClient.UploadData(appListUri, "PUT", memoryStream.ToArray());
        }
        #endregion

        #region Merge data
        /// <summary>
        /// Merges a new <see cref="IntegrationManagerBase.AppList"/> with the existing data.
        /// </summary>
        /// <param name="remoteAppList">The remote <see cref="AppList"/> to merge in.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="resetClient">Set to <see langword="true"/> to completly replace the contents of <see cref="IIntegrationManager.AppList"/> with <paramref name="remoteAppList"/> instead of merging the two.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="InvalidOperationException">Thrown if one or more new <see cref="AccessPoint"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <remarks>Performs a three-way merge using <see cref="_appListLastSync"/> as base.</remarks>
        private void MergeData(AppList remoteAppList, ITaskHandler handler, bool resetClient)
        {
            #region Sanity checks
            if (remoteAppList == null) throw new ArgumentNullException("remoteAppList");
            if (handler == null) throw new ArgumentNullException("handler");
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

            // Apply changes with rollback protection
            toRemove.ApplyWithRollback(RemoveAppInternal, AddAppHelper);
            toAdd.ApplyWithRollback(AddAppHelper, RemoveAppInternal);
        }

        /// <summary>
        /// Creates a new <see cref="AppEntry"/> based on an existing prototype (applying any <see cref="AccessPoint"/>s) and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="prototype">An existing <see cref="AppEntry"/> to use as a prototype.</param>
        private void AddAppHelper(AppEntry prototype)
        {
            AddAppInternal(prototype, _feedRetriever);
        }
        #endregion
    }
}
