/*
 * Copyright 2010-2013 Bastian Eicher, Roland Leopold Walkling
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
using Common;
using Common.Tasks;
using ZeroInstall.Fetchers.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Downloads <see cref="Implementation"/>s sequentially.
    /// </summary>
    public class SequentialFetcher : FetcherBase
    {
        /// <summary>
        /// Creates a new sequential download fetcher.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Model.Implementation"/>s in.</param>
        public SequentialFetcher(IStore store) : base(store)
        {}

        /// <inheritdoc/>
        public override void FetchImplementations(IEnumerable<Implementation> implementations, ITaskHandler handler)
        {
            #region Sanity checks
            if (implementations == null) throw new ArgumentNullException("implementations");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            foreach (var implementation in implementations)
            {
                handler.CancellationToken.ThrowIfCancellationRequested();

                // Use mutex to detect concurrent download of same implementation in other processes
                using (var mutex = new Mutex(false, "0install-fetcher-" + implementation.ManifestDigest.AvailableDigests.First()))
                {
                    try
                    {
                        while (!mutex.WaitOne(100, false)) // NOTE: Might be blocked more than once
                        {
                            // Wait for mutex to be released
                            handler.RunTask(new WaitTask(Resources.DownloadInAnotherWindow, mutex), implementation.ManifestDigest);
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
                        if (Store.Contains(implementation.ManifestDigest)) return;

                        FetchImplementation(implementation, handler);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }
    }
}
