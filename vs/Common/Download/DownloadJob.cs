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
using System.IO;
using Common.Utils;
using Common.Properties;

namespace Common.Download
{
    #region Delegates
    /// <summary>
    /// Generic delegate for handling an event without passing any parameters.
    /// </summary>
    public delegate void DownloadJobEventHandler(DownloadJob sender);
    #endregion

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
        /// Occurs when all contained <see cref="Files"/> have been downloaded. Blocks the download thread, so handle quickly!
        /// </summary>
        /// <remarks>This event is executed in a background thread. It must not be used to directly update UI elements.</remarks>
        public event DownloadJobEventHandler Completed;

        private void OnCompleted()
        {
            // Copy to local variable to prevent threading issues
            DownloadJobEventHandler completed = Completed;
            if (completed != null) completed(this);
        }

        /// <summary>
        /// Occurs if any of the contained <see cref="Files"/> could not be downloaded.
        /// </summary>
        /// <remarks>This event is executed in a background thread. It must not be used to directly update UI elements.</remarks>
        public event DownloadJobEventHandler Failed;

        private void OnFailed()
        {
            // Copy to local variable to prevent threading issues
            DownloadJobEventHandler failed = Failed;
            if (failed != null) failed(this);
        }

        /// <summary>
        /// Occurs whenever <see cref="Priority"/> has been changed.
        /// </summary>
        [Description("Occurs whenever Priority has been changed.")]
        public event DownloadJobEventHandler PriorityChanged;

        private void OnPriorityChanged()
        {
            // Copy to local variable to prevent threading issues
            DownloadJobEventHandler priorityChanged = PriorityChanged;
            if (priorityChanged != null) priorityChanged(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// A name that describes this download job for the user.
        /// </summary>
        [Description("A name that describes this download job for the user.")]
        public string Name { get; private set; }

        /// <summary>
        /// A read-only ordered list of files to download.
        /// </summary>
        [ReadOnly(true), Description("A read-only ordered list of files to download.")]
        public ICollection<DownloadFile> Files { get; private set; }

        private DownloadPriority _priority;
        /// <summary>
        /// The priority of the job. Controls how its execution is scheduled.
        /// </summary>
        [Description("The priority of the job. Controls how its execution is scheduled.")]
        public DownloadPriority Priority
        {
            get { return _priority; }
            set
            {
                #region Sanity checks
                if (value < DownloadPriority.Low || value > DownloadPriority.Foreground)
                    throw new ArgumentException(Resources.InvalidPriority, "value");
                #endregion

                UpdateHelper.Do(ref _priority, value, OnPriorityChanged);
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

        private void UpdateFileState(IProgress sender)
        {
            switch (sender.State)
            {
                case ProgressState.Complete:
                    // Check if all file downloads have been completed
                    foreach (DownloadFile file in Files)
                    {
                        // No event if any of the files aren't completed yet
                        if (file.State != ProgressState.Complete) return;
                    }
                    OnCompleted();
                    break;

                case ProgressState.WebError:
                case ProgressState.IOError:
                    // Cancel all other file downloads in this job
                    foreach (DownloadFile file in Files)
                    {
                        if (file != sender)
                        {
                            file.Cancel();
                            try { if (File.Exists(file.Target)) File.Delete(file.Target); }
                            catch (UnauthorizedAccessException) {}
                        }
                    }
                    OnFailed();
                    break;
            }
        }
    }
}
