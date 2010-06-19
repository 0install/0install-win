/*
 * Copyright 2010 Bastian Eicher
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
using Common.Archive;
using Common.Download;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using System.Diagnostics;

namespace ZeroInstall.DownloadBroker
{
    public class FetcherException : Exception
    {
        internal FetcherException(string message)
            : base(message)
        { }
    }

    /// <summary>
    /// Manages one or more <see cref="FetcherRequest"/>s and keeps clients informed of the progress.
    /// </summary>
    public class Fetcher
    {
        #region Singleton Properties
        private static readonly Fetcher _defaultFetcher = new Fetcher(StoreProvider.Default);
        /// <summary>
        /// A singleton-instance of the <see cref="Fetcher"/> using <see cref="StoreProvider.Default"/>.
        /// </summary>
        public static Fetcher Default { get { return _defaultFetcher; } }
        #endregion

        #region Properties
        /// <summary>
        /// The location to store the downloaded and unpacked <see cref="Implementation"/>s in.
        /// </summary>
        public IStore Store { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download request. Use <see cref="Default"/> whenever possible instead.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Implementation"/>s in.</param>
        public Fetcher(IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            Store = store;
        }
        #endregion
        
        //--------------------//

        /// <summary>
        /// Execute a complete request and block until it is done.
        /// </summary>
        public void RunSync(FetcherRequest fetcherRequest)
        {
            #region Sanity checks
            if (fetcherRequest == null) throw new ArgumentNullException("fetcherRequest");
            #endregion

            foreach (var implementation in fetcherRequest.Implementations)
            {
                foreach (var archive in implementation.Archives)
                {
                    string tempArchive = Path.GetTempFileName();
                    FetchArchive(archive, tempArchive);
                    Store.AddArchive(new ArchiveFileInfo(tempArchive, archive.MimeType, archive.Extract, archive.StartOffset), implementation.ManifestDigest);
                    File.Delete(tempArchive);
                    return;
                }
                foreach (var recipe in implementation.Recipes)
                {
                    var archives = new List<ArchiveFileInfo>();
                    foreach (var currentArchive in recipe.Archives)
                    {
                        string tempArchive = Path.GetTempFileName();
                        FetchArchive(currentArchive, tempArchive);
                    }
                    Store.AddMultipleArchives(archives, implementation.ManifestDigest);
                    return;
                }
                throw new InvalidOperationException("No working retrieval method.");
            }
        }

        private static void FetchArchive(Archive archive, string destination)
        {
            var downloadFile = new DownloadFile(archive.Location, destination);

            RejectRemoteFileOfDifferentSize(archive, downloadFile);
            try
            {
                downloadFile.RunSync();
                RejectRemoteFileOfDifferentSize(archive, downloadFile);
            }
            catch (FetcherException)
            {
                File.Delete(destination);
                throw;
            }
        }

        /// <summary>
        /// Checks if the size of <paramref name="downloadFile"/> matches the
        /// <paramref name="archive"/>'s size and offset, otherwise throws a
        /// <see cref="FetcherException"/>.
        /// </summary>
        /// <remarks>Does not perform any check if size isn't yet known.</remarks>
        /// <exception cref="FetcherException">Thrown if the remote file is known
        /// to have different size than stated in <paramref name="archive"/>.
        /// </exception>
        private static void RejectRemoteFileOfDifferentSize(Archive archive, DownloadFile downloadFile)
        {
            Debug.Assert(archive != null);
            Debug.Assert(downloadFile != null);

            long actualSize;
            if (downloadFile.State == DownloadState.Complete) actualSize = downloadFile.BytesReceived;
            else if (IsSizeKnown(downloadFile)) actualSize = downloadFile.BytesTotal;
            else return;

            if (actualSize != archive.Size + archive.StartOffset)
                throw new FetcherException("Invalid size of file downloaded from " + archive.Location.AbsolutePath);
        }
        
        /// <summary>
        /// Determines of the size of a <see cref="DownloadFile"/> is know prior to
        /// performing the transmission.
        /// </summary>
        private static bool IsSizeKnown(DownloadFile downloadFile)
        {
            Debug.Assert(downloadFile != null);
            return downloadFile.BytesTotal != -1;
        }
    }
}
