/*
 * Copyright 2010-2014 Bastian Eicher, Roland Leopold Walkling
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
using System.Linq;
using System.Threading;
using NanoByte.Common;
using NanoByte.Common.Tasks;
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
        public SequentialFetcher(IStore store, ITaskHandler handler) : base(store, handler)
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
                Fetch(implementation);
            }
        }

        private void Fetch(Implementation implementation)
        {
            var manifestDigest = implementation.ManifestDigest;
            if (!manifestDigest.AvailableDigests.Any()) throw new NotSupportedException(string.Format(Resources.NoManifestDigest, implementation.ID));

            // Use mutex to detect concurrent download of same implementation in other processes
            using (var mutex = new Mutex(false, "0install-fetcher-" + manifestDigest.AvailableDigests.First()))
            {
                try
                {
                    while (!mutex.WaitOne(100, exitContext: false)) // NOTE: Might be blocked more than once
                    {
                        // Wait for mutex to be released
                        Handler.RunTask(new WaitTask(Resources.DownloadInAnotherWindow, mutex) {Tag = manifestDigest});
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
                    Store.Flush();
                    if (Store.Contains(manifestDigest)) return;

                    FetchImplementation(implementation);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
