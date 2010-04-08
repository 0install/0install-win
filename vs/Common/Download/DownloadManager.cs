/*
 * Copyright 2006-2010 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Common.Helpers;
using Common.Properties;

namespace Common.Download
{
    /// <summary>
    /// Controls the executing of <see cref="DownloadJob"/>s sorting them by priority.
    /// </summary>
    public static class DownloadManager
    {
        #region Variables
        /// <summary>Synchronization handle to prevent race conditions with <see cref="DownloadJob"/> scheduling.</summary>
        private static readonly object _scheduleLock = new object();

        // Sort by priority, duplicate entries are allowed
        private static readonly C5.ISorted<DownloadJob> _jobs = new C5.TreeSet<DownloadJob>();

        // Preserve order, duplicate entries are not allowed
        private static readonly C5.IList<DownloadFile> _files = new C5.HashedLinkedList<DownloadFile>();
        #endregion

        #region Properties
        private static int _maxSimultaneousDownloads = 2;
        /// <summary>
        /// The maximum number of simultaneous the download manager will schedule.
        /// </summary>
        /// <remarks>This value may be exceeded if there are active <see cref="DownloadFile"/>s that can not be paused because their <see cref="DownloadFile.SupportsResume"/> capability is <see langword="false"/>.</remarks>
        [DefaultValue(2)]
        public static int MaxSimultaneousDownloads
        {
            get { return _maxSimultaneousDownloads; }
            set
            {
                if (_maxSimultaneousDownloads < 0) throw new ArgumentOutOfRangeException("value", Resources.ArgMustNotBeNegative);
                _maxSimultaneousDownloads = value;
                UpdateHelper.Do(ref _maxSimultaneousDownloads, value, delegate
                {
                    lock (_scheduleLock)
                    {
                        UpdateScheduling();
                    }
                });
            }
        }

        /// <summary>
        /// A priority-sorted read-only list of currently active <see cref="DownloadJob"/>s.
        /// </summary>
        /// <remarks>The returned list will not be updated.</remarks>
        public static ICollection<DownloadJob> Jobs
        {
            get
            {
                lock (_scheduleLock)
                {
                    // Copy to an array for thread-safety
                    var array = new DownloadJob[_jobs.Count];
                    _jobs.CopyTo(array, 0);
                    return array;
                }
            }
        }
        #endregion

        //--------------------//

        #region Internal Job communication
        /// <summary>
        /// Adds a <see cref="DownloadJob"/> and its <see cref="DownloadFile"/>s to the queue.
        /// </summary>
        /// <param name="job">The job to add.</param>
        /// <remarks>Calling this for a <see cref="DownloadJob"/> already in the queue will have no effect.</remarks>
        internal static void AddJob(DownloadJob job)
        {
            lock (_scheduleLock)
            {
                _jobs.Add(job);
                UpdateScheduling();
            }
        }

        /// <summary>
        /// Removes a <see cref="DownloadJob"/> and its <see cref="DownloadFile"/>s from the queue.
        /// </summary>
        /// <param name="job">The job to remove.</param>
        /// <remarks>Calling this for a <see cref="DownloadJob"/> not in the queue will have no effect.</remarks>
        internal static void RemoveJob(DownloadJob job)
        {
            lock (_scheduleLock)
            {
                _jobs.Remove(job);
                UpdateScheduling();
            }
        }

        /// <summary>
        /// To be called after a <see cref="DownloadJob"/>s <see cref="DownloadJob.Priority"/> has been changed.
        /// </summary>
        /// <param name="job">The job to update.</param>
        /// <remarks>Calling this for a <see cref="DownloadJob"/> not in the queue will have no effect.</remarks>
        internal static void UpdateJobPriority(DownloadJob job)
        {
            lock (_scheduleLock)
            {
                // Remove and then re-add the job to perform a new sorting comparison
                if (_jobs.Remove(job)) _jobs.Add(job);

                UpdateScheduling();
            }
        }

        /// <summary>
        /// To be called after a <see cref="DownloadFile"/>s <see cref="DownloadFile.State"/> has changed.
        /// </summary>
        /// <param name="thread">The updated thread.</param>
        internal static void UpdateThreadState(DownloadFile thread)
        {
            lock (_scheduleLock)
            {
                UpdateScheduling();
            }
        }
        #endregion

        #region Scheduling
        /// <summary>
        /// Updates the <see cref="DownloadFile"/> scheduling.
        /// </summary>
        /// <remarks>This method must only be called within <code>lock (_jobsLock) { ... }</code> blocks.</remarks>
        private static void UpdateScheduling()
        {
            // ToDo
        }
        #endregion
    }
}
