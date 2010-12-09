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
using System.Diagnostics.CodeAnalysis;
using Common.Utils;
using Common.Properties;

namespace Common.Net
{
    /// <summary>
    /// Controls the executing of multiple <see cref="DownloadJob"/>s (sorting them by priority).
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class DownloadManager
    {
        #region Variables
        /// <summary>Synchronization handle to prevent race conditions with <see cref="DownloadJob"/> scheduling.</summary>
        private readonly object _scheduleLock = new object();
        #endregion

        #region Properties
        private int _maxSimultaneousDownloads = 3;
        /// <summary>
        /// The maximum number of simultaneous the download manager will schedule.
        /// </summary>
        /// <remarks>This value may be exceeded if there are active <see cref="DownloadFile"/>s that can not be paused because their <see cref="DownloadFile.SupportsResume"/> capability is <see langword="false"/>.</remarks>
        [DefaultValue(3), Description("The maximum number of simultaneous the download manager will schedule.")]
        public int MaxSimultaneousDownloads
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

        // Preserve order, duplicate entries are not allowed
        /// <remarks>This variable must only be accessed within <code>lock (_jobsLock) { ... }</code> blocks.</remarks>
        private readonly C5.IList<DownloadJob> _jobs = new C5.HashedLinkedList<DownloadJob>();
        /// <summary>
        /// A priority-sorted snapshot of all currently active <see cref="DownloadJob"/>s.
        /// </summary>
        /// <remarks>The returned list is a snapshot will not be updated.</remarks>
        [ReadOnly(true), Description("A priority-sorted read-only list of all currently active DownloadJobs.")]
        public ICollection<DownloadJob> Jobs
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
        
        // Preserve order, duplicate entries are not allowed
        /// <remarks>This variable must only be accessed within <code>lock (_jobsLock) { ... }</code> blocks.</remarks>
        private readonly C5.IList<DownloadFile> _files = new C5.HashedLinkedList<DownloadFile>();
        /// <summary>
        /// A priority-sorted snapshot of all <see cref="DownloadFile"/>s contained in currently active <see cref="DownloadJob"/>s.
        /// </summary>
        /// <remarks>The returned list is a snapshot will not be updated.</remarks>
        [ReadOnly(true), Description("A priority-sorted snapshot of all currently active DownloadFiles contained in currently active DownloadJobs.")]
        public ICollection<DownloadFile> Files
        {
            get
            {
                lock (_scheduleLock)
                {
                    // Copy to an array for thread-safety
                    var array = new DownloadFile[_files.Count];
                    _files.CopyTo(array, 0);
                    return array;
                }
            }
        }
        #endregion

        //--------------------//

        #region User control
        /// <summary>
        /// Adds a <see cref="DownloadJob"/> and its <see cref="DownloadFile"/>s to the queue.
        /// </summary>
        /// <param name="job">The job to add.</param>
        /// <remarks>Calling this for a <see cref="DownloadJob"/> already in the queue will have no effect.</remarks>
        public void AddJob(DownloadJob job)
        {
            #region Sanity checks
            if (job == null) throw new ArgumentNullException("job");
            #endregion

            lock (_scheduleLock)
            {
                if (_jobs.Add(job))
                {
                    job.PriorityChanged += UpdateJob;
                    UpdateScheduling();
                }
            }
        }

        /// <summary>
        /// Removes a <see cref="DownloadJob"/> and its <see cref="DownloadFile"/>s from the queue.
        /// </summary>
        /// <param name="job">The job to remove.</param>
        /// <remarks>Calling this for a <see cref="DownloadJob"/> not in the queue will have no effect.</remarks>
        public void RemoveJob(DownloadJob job)
        {
            #region Sanity checks
            if (job == null) throw new ArgumentNullException("job");
            #endregion

            lock (_scheduleLock)
            {
                if (_jobs.Remove(job))
                {
                    job.PriorityChanged -= UpdateJob;
                    UpdateScheduling();
                }
            }
        }
        #endregion

        #region Scheduling
        /// <summary>
        /// To be called when a <see cref="DownloadJob.Priority"/> has been changed.
        /// </summary>
        /// <param name="job">The job that was update.</param>
        private void UpdateJob(DownloadJob job)
        {
            lock (_scheduleLock)
            {
                UpdateScheduling();
            }
        }

        /// <summary>
        /// Updates the <see cref="DownloadFile"/> scheduling.
        /// </summary>
        /// <remarks>This method must only be called within <code>lock (_scheduleLock) { ... }</code> blocks.</remarks>
        private void UpdateScheduling()
        {
            // ToDo: Implement
        }
        #endregion
    }
}
