/*
 * Copyright 2010-2016 Bastian Eicher, Roland Leopold Walkling
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
using System.Threading;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Fetchers
{
    /// <summary>
    /// Downloads <see cref="Implementation"/>s sequentially.
    /// </summary>
    public class SequentialFetcher : FetcherBase
    {
        #region Dependencies
        private readonly Config _config;

        /// <summary>
        /// Creates a new sequential download fetcher.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Implementation"/>s in.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        public SequentialFetcher([NotNull] Config config, [NotNull] IStore store, [NotNull] ITaskHandler handler)
            : base(store, handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException(nameof(config));
            #endregion

            _config = config;
        }
        #endregion

        /// <inheritdoc/>
        public override void Fetch(IEnumerable<Implementation> implementations)
        {
            #region Sanity checks
            if (implementations == null) throw new ArgumentNullException(nameof(implementations));
            #endregion

            foreach (var implementation in implementations)
            {
                Handler.CancellationToken.ThrowIfCancellationRequested();
                FetchOne(implementation);
            }
        }

        /// <summary>
        /// Downloads a single <see cref="Implementation"/> to the <see cref="IStore"/>. Detects concurrent downloads in other processes.
        /// </summary>
        /// <param name="implementation">The implementation to download.</param>
        /// <exception cref="OperationCanceledException">A download or IO task was canceled from another thread.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        private void FetchOne([NotNull] Implementation implementation)
        {
            // Use mutex to detect in-progress download of same implementation in other processes
            using (var mutex = new Mutex(false, "0install-fetcher-" + GetDownloadID(implementation)))
            {
                try
                {
                    while (!mutex.WaitOne(100, exitContext: false)) // NOTE: Might be blocked more than once
                    {
                        // Wait for mutex to be released
                        Handler.RunTask(new WaitTask(Resources.WaitingForDownload, mutex) {Tag = implementation.ManifestDigest});
                    }
                }
                    #region Error handling
                catch (AbandonedMutexException ex)
                {
                    // Abandoned mutexes also get owned, but indicate something may have gone wrong elsewhere
                    Log.Warn(ex.Message);
                }
                #endregion

                try
                {
                    // Check if another process added the implementation in the meantime
                    if (IsCached(implementation)) return;

                    if (implementation.RetrievalMethods.Count == 0) throw new NotSupportedException(string.Format(Resources.NoRetrievalMethod, implementation.ID));
                    Retrieve(implementation);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Returns a unique identifier for an <see cref="Implementation"/>. Usually based on <see cref="ImplementationBase.ManifestDigest"/>.
        /// </summary>
        /// <exception cref="NotSupportedException"><paramref name="implementation"/> does not specify manifest digests in any known formats.</exception>
        [NotNull]
        private static string GetDownloadID([NotNull] Implementation implementation)
        {
            if (implementation.ID.StartsWith(ExternalImplementation.PackagePrefix))
                return implementation.ID;
            else
            {
                var digest = implementation.ManifestDigest.Best;
                if (digest == null) throw new NotSupportedException(string.Format(Resources.NoManifestDigest, implementation.ID));
                return digest;
            }
        }

        /// <inheritdoc/>
        protected override TemporaryFile Download(DownloadRetrievalMethod retrievalMethod, object tag = null)
        {
            #region Sanity checks
            if (retrievalMethod == null) throw new ArgumentNullException(nameof(retrievalMethod));
            #endregion

            retrievalMethod.Validate();

            try
            {
                return base.Download(retrievalMethod, tag);
            }
            catch (WebException ex) when (!retrievalMethod.Href.IsLoopback)
            {
                Log.Warn(ex);
                Log.Info("Trying mirror");

                try
                {
                    var mirrored = (DownloadRetrievalMethod)retrievalMethod.CloneRecipeStep();
                    mirrored.Href = GetMirrorUrl(retrievalMethod.Href);
                    return base.Download(mirrored, tag);
                }
                catch (WebException)
                {
                    // Report the original problem instead of mirror errors
                    throw ex.PreserveStack();
                }
            }
        }

        [NotNull]
        private Uri GetMirrorUrl([NotNull] Uri url)
        {
            return new Uri(_config.FeedMirror.EnsureTrailingSlash().AbsoluteUri + "archive/" + url.Scheme + "/" + url.Host + "/" + string.Concat(url.Segments).TrimStart('/').Replace("/", "%23"));
        }
    }
}
