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
    /// <summary>
    /// Describes the priority of a download.
    /// </summary>
    /// <remarks>
    /// Background priorities should be used for automatic updates.
    /// Foreground priorities should be used when the user is actively waiting for completion.
    /// </remarks>
    /// <seealso cref="DownloadJob.Priority"/>
    public enum DownloadPriority
    {
        /// <summary>The lowest background priority.</summary>
        Low,
        /// <summary>The default background priority.</summary>
        Medium,
        /// <summary>The highest background priority.</summary>
        High,
        /// <summary>The highest priroty. Will be downloaded before everything else.</summary>
        Foreground,
    }
    #endregion

    /// <summary>
    /// Groups multiple <see cref="DownloadFile"/>s together for scheduling by <see cref="DownloadManager"/>.
    /// </summary>
    /// <remarks>Scheduling is internally handled by <see cref="DownloadManager"/>.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "IComparable was implemented to allow priority sorting, not for use as a pseudo-mathematical object")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    public sealed class DownloadJob
    {
        #region Events
        /// <summary>
        /// Is raised when all contained <see cref="Files"/> have been downloaded. Blocks the download thread, so handle quickly!
        /// </summary>
        /// <remarks>This event is executed in a background thread. It must not be used to directly update UI elements.</remarks>
        public event SimpleEventHandler Completed;

        private void OnCompleted()
        {
            // Copy to local variable to prevent threading issues
            SimpleEventHandler completed = Completed;
            if (completed != null) completed();
        }

        /// <summary>
        /// Is raised if any of the contained <see cref="Files"/> could not be downloaded.
        /// </summary>
        /// <remarks>This event is executed in a background thread. It must not be used to directly update UI elements.</remarks>
        public event SimpleEventHandler Failed;

        private void OnFailed()
        {
            // Copy to local variable to prevent threading issues
            SimpleEventHandler failed = Failed;
            if (failed != null) failed();
        }
        #endregion

        #region Properties
        /// <summary>
        /// A name that describes this download for the user.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A list of all file names in the arguments.
        /// </summary>
        public ICollection<DownloadFile> Files { get; private set; }

        private DownloadPriority _priority;
        /// <summary>
        /// The priority of the job. Controls how its execution is scheduled.
        /// </summary>
        public DownloadPriority Priority
        {
            get { return _priority; }
            set
            {
                #region Sanity checks
                if (value < DownloadPriority.Low || value > DownloadPriority.Foreground)
                    throw new ArgumentException(Resources.InvalidPriority, "value");
                #endregion

                // Check if the new value is actually different
                if (value != _priority)
                {
                    _priority = value;

                    DownloadManager.UpdateJob(this);
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download job with multiple <see cref="DownloadFile"/>s.
        /// </summary>
        /// <param name="name">A name that describes this download for the user.</param>
        /// <param name="files">An ordered list of files to download. Must not be empty.</param> 
        /// <param name="priority">The priority of the job.</param>
        public DownloadJob(string name, IEnumerable<DownloadFile> files, DownloadPriority priority)
        {
            #region Sanity checks
            if (files == null) throw new ArgumentNullException("files");
            #endregion

            Name = name;

            // Order unimportant, duplicate entries are not allowed
            var filesTemp = new C5.HashBag<DownloadFile>();
            foreach (DownloadFile file in files)
            {
                file.StateChanged += UpdateFileState;
                filesTemp.Add(file);
            }
            if (filesTemp.IsEmpty) throw new ArgumentException(Resources.CollectionIsEmpty, "files");
            
            // Make the collections immutable
            Files = new C5.GuardedCollection<DownloadFile>(filesTemp);

            Priority = priority;
        }

        /// <summary>
        /// Creates a new download job with a single <see cref="DownloadFile"/>.
        /// </summary>
        /// <param name="name">A name that describes this download for the user.</param>
        /// <param name="file">The file to download.</param> 
        /// <param name="priority">The priority of the job.</param>
        public DownloadJob(string name, DownloadFile file, DownloadPriority priority) : this(name, new [] {file}, priority)
        {}
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

        private void UpdateFileState()
        {
            // ToDo: Raise Completed and Failed events
        }
    }
}
