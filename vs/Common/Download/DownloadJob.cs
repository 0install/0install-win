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
using System.Diagnostics.CodeAnalysis;
using Common.Helpers;
using Common.Properties;

namespace Common.Download
{
    #region Enumerations
    /// <seealso cref="DownloadJob.Priority"/>
    public enum DownloadPriority
    {
        /// <summary>The highest priority. For use when the user is actively waiting for completion.</summary>
        Foreground,
        /// <summary>The highest background priority. If BITS downloading is used, this will only use idle bandwith.</summary>
        High,
        /// <summary>The default background priority. If BITS downloading is used, this will only use idle bandwith.</summary>
        Normal,
        /// <summary>The lowest possible background priority. If BITS downloading is used, this will only use idle bandwith.</summary>
        Low
    }
    #endregion

    /// <summary>
    /// Describes a download job containing one or more <see cref="DownloadFile"/>s.
    /// </summary>
    /// <remarks>Scheduling is internally handled by <see cref="DownloadManager"/>.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "IComparable was implemented to allow priority sorting, not for use as a pseudo-mathematical object")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be diposed when using snapshots")]
    public sealed class DownloadJob : IComparable<DownloadJob>
    {
        #region Properties
        // Preserve order, duplicate entries are not allowed
        private readonly C5.IList<DownloadFile> _files = new C5.HashedLinkedList<DownloadFile>();
        /// <summary>
        /// An ordered list of files to download.
        /// </summary>
        public ICollection<DownloadFile> Files { get { return new C5.GuardedList<DownloadFile>(_files); } }

        private DownloadPriority _priority = DownloadPriority.Foreground;
        /// <summary>
        /// The priority of the job. Controls how its execution is scheduled.
        /// </summary>
        public DownloadPriority Priority
        {
            get { return _priority; }
            set { UpdateHelper.Do(ref _priority, value, () => DownloadManager.UpdateJobPriority(this)); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download job with multiple <see cref="DownloadFile"/>s.
        /// </summary>
        /// <param name="files">An ordered list of files to download. Must not be empty.</param> 
        /// <param name="priority">The priority of the job.</param>
        public DownloadJob(IEnumerable<DownloadFile> files, DownloadPriority priority)
        {
            #region Sanity checks
            if (files == null) throw new ArgumentNullException("files");
            #endregion

            _files.AddAll(files);
            if (_files.IsEmpty) throw new ArgumentException(Resources.CollectionIsEmpty, "files");

            Priority = priority;
        }

        /// <summary>
        /// Creates a new download job with a single <see cref="DownloadFile"/>.
        /// </summary>
        /// <param name="file">The file to download.</param> 
        /// <param name="priority">The priority of the job.</param>
        public DownloadJob(DownloadFile file, DownloadPriority priority)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            _files.Add(file);
            Priority = priority;
        }
        #endregion

        //--------------------//

        #region User control
        /// <summary>
        /// Adds the job's and its <see cref="DownloadFile"/>s to the <see cref="DownloadManager"/>'s queue.
        /// </summary>
        /// <remarks>Calling this on an already running job has no effect.</remarks>
        public void Start()
        {
            DownloadManager.AddJob(this);
        }

        /// <summary>
        /// Removes the job's and its <see cref="DownloadFile"/>s from the <see cref="DownloadManager"/>'s queue.
        /// </summary>
        /// <remarks>Calling this on a not running job has no effect.</remarks>
        public void Stop()
        {
            DownloadManager.RemoveJob(this);
        }
        #endregion

        #region Comparison
        public int CompareTo(DownloadJob other)
        {
            // Sort based on priority
            return Priority - other.Priority;
        }
        #endregion
    }
}
