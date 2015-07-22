/*
 * Copyright 2010-2015 Bastian Eicher, Roland Leopold Walkling
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
using System.Linq;
using System.Net;
using System.Threading;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Services.Properties;
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
        /// <summary>
        /// Creates a new sequential download fetcher.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Store.Model.Implementation"/>s in.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        public SequentialFetcher([NotNull] IStore store, [NotNull] ITaskHandler handler) : base(store, handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override void Fetch(IEnumerable<Implementation> implementations)
        {
            #region Sanity checks
            if (implementations == null) throw new ArgumentNullException("implementations");
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
        /// <exception cref="DigestMismatchException">An <see cref="Store.Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
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
            if (implementation.ID.StartsWith(ExternalImplementation.PackagePrefix)) return implementation.ID;
            else
            {
                try
                {
                    return implementation.ManifestDigest.AvailableDigests.First();
                }
                    #region Error handling
                catch (InvalidOperationException)
                {
                    throw new NotSupportedException(string.Format(Resources.NoManifestDigest, implementation.ID));
                }
                #endregion
            }
        }
    }
}
