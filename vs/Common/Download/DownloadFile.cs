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
using System.IO;
using System.Net;
using System.Threading;
using Common.Helpers;
using Common.Properties;

namespace Common.Download
{
    #region Enumerations
    /// <seealso cref="DownloadFile.State"/>
    public enum DownloadState
    {
        /// <summary>The download is ready to begin.</summary>
        Ready,
        /// <summary>The user has chosen not to have the <see cref="DownloadManager"/> dispatch this download thread at this time.</summary>
        Paused,
        /// <summary>The download thread has just been started.</summary>
        Started,
        /// <summary>Getting the header data.</summary>
        GettingHeaders,
        /// <summary>Downloading the actual data.</summary>
        GettingData,
        /// <summary>The download has been completed sucessfully.</summary>
        Complete,
        /// <summary>An error occured during the download.</summary>
        WebError,
        /// <summary>An error occured while writing the file.</summary>
        IOError
    }
    #endregion

    /// <summary>
    /// Downloads a file from a specific internet address to a local file using a background thread.
    /// </summary>
    /// <remarks>
    /// <para>Can be used stand-alone or as a part of a <see cref="DownloadJob"/>.</para>
    /// <para>It may also be shared among several <see cref="DownloadJob"/>s. It will not be downloaded multiple times in that case. Instead it will only be handled by the <see cref="DownloadJob"/> with the highest <see cref="DownloadJob.Priority"/>.</para>
    /// </remarks>
    public class DownloadFile
    {
        #region Events
        /// <summary>
        /// Is raised whenever <see cref="State"/> changes.
        /// </summary>
        /// <remarks>This event is executed in a background thread. It can not be used to directly update UI elements.</remarks>
        public event SimpleEventHandler StateChanged;

        private void OnStateChanged()
        {
            // Copy to local variable to prevent threading issues
            SimpleEventHandler stateChanged = StateChanged;
            if (stateChanged != null) stateChanged();

            DownloadManager.UpdateThreadState(this);
        }

        /// <summary>
        /// Is raised whenever <see cref="BytesReceived"/> changes.
        /// </summary>
        /// <remarks>This event is executed in a background thread. It can not be used to directly update UI elements.</remarks>
        public event SimpleEventHandler BytesReceivedChanged;

        private void OnBytesReceivedChanged()
        {
            // Copy to local variable to prevent threading issues
            SimpleEventHandler bytesReceivedChanged = BytesReceivedChanged;
            if (bytesReceivedChanged != null) bytesReceivedChanged();
        }
        #endregion

        #region Variables
        /// <summary>Synchronization handle to prevent race conditions with thread startup/shutdown.</summary>
        private static readonly object _stateLock = new object();

        /// <summary>The background thread used for downloading.</summary>
        private readonly Thread _thread;
        #endregion

        #region Properties
        // Order unimportant, duplicate entries are not allowed
        private readonly C5.ICollection<Uri> _sources = new C5.HashSet<Uri>();
        /// <summary>
        /// A list of URLs the file can be downloaded from.
        /// </summary>
        /// <remarks>One or more sources are automatically chosen based on their response times.</remarks>
        public ICollection<Uri> Sources { get { return new C5.GuardedCollection<Uri>(_sources); } }

        /// <summary>
        /// The URL the file is actually downloaded from.
        /// This depends on which source was auto-selected from <see cref="Sources"/> as well as any redirections.
        /// </summary>
        /// <remarks>This value is only set once <see cref="DownloadState.GettingData"/> has been reached.</remarks>
        public Uri EffectiveSource { get; private set; }

        /// <summary>
        /// The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.
        /// </summary>
        public string Target { get; private set; }

        private DownloadPriority _priority = DownloadPriority.Foreground;
        /// <summary>
        /// The priority of the file download. Controls how its execution is scheduled.
        /// </summary>
        /// <remarks>
        /// <para>If this <see cref="DownloadFile"/> is used, stand-alone the priority will only have an effect if BITS downloading is used.</para>
        /// <para>If this <see cref="DownloadFile"/> is used as a part of a <see cref="DownloadJob"/>, it will inherit its priority from there.</para>
        /// </remarks>
        public DownloadPriority Priority
        {
            get { return _priority; }
            set { UpdateHelper.Do(ref _priority, value, UpdatePriority); }
        }

        private DownloadState _state;
        /// <summary>
        /// The current status of the download.
        /// </summary>
        public DownloadState State
        {
            get { return _state; }
            private set { UpdateHelper.Do(ref _state, value, OnStateChanged); }
        }

        /// <summary>
        /// Contains an error description if <see cref="State"/> is set to <see cref="DownloadState.WebError"/> or <see cref="DownloadState.IOError"/>.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// The HTTP header data returned by the server for the download request. An empty collection in case of an FTP download.
        /// </summary>
        /// <remarks>This value is always <see langword="null"/> until <see cref="DownloadState.GettingData"/> has been reached.</remarks>
        public WebHeaderCollection Headers { get; private set; }

        /// <summary>
        /// Indicates whether this download can be resumed without having to start from the beginning again.
        /// </summary>
        /// <remarks>This value is always <see langword="true"/> until <see cref="DownloadState.GettingData"/> has been reached.</remarks>
        public bool SupportsResume { get; private set; }

        private long _bytesReceived;
        /// <summary>
        /// The number of bytes that have been downloaded so far.
        /// </summary>
        public long BytesReceived
        {
            get { return _bytesReceived; }
            private set { UpdateHelper.Do(ref _bytesReceived, value, OnBytesReceivedChanged); }
        }

        /// <summary>
        /// The number of bytes the file to be downloaded is long.
        /// </summary>
        /// <remarks>If this value is se to -1 in the constructor, the size be automatically set after <see cref="DownloadState.GettingData"/> has been reached.</remarks>
        public long BytesTotal { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download thread with multiple sources and a predefined file size.
        /// </summary>
        /// <param name="sources">A list of URLs the file can be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        /// <param name="bytesTotal">The number of bytes the file to be downloaded is long. The file will be rejected if it does not have this length.</param>
        public DownloadFile(IEnumerable<Uri> sources, string target, long bytesTotal)
        {
            #region Sanity checks
            if (sources == null) throw new ArgumentNullException("sources");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion
            
            // Check each sources entry is a valid URL
            foreach (Uri uri in sources)
            {
                if (uri == null) continue;
                if (uri.Scheme != "http" && uri.Scheme != "ftp") throw new ArgumentException(Resources.HttpAndFtpOnly);
                _sources.Add(uri);
            }
            if (_sources.IsEmpty) throw new ArgumentException(Resources.CollectionIsEmpty, "sources");

            Target = target;
            BytesTotal = bytesTotal;

            // Prepare the background thread for later execution
            _thread = new Thread(RunDownload) {IsBackground = true};
        }

        /// <summary>
        /// Creates a new download thread with multiple sources and no predefined file size.
        /// </summary>
        /// <param name="sources">A list of URLs the file can be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        public DownloadFile(IEnumerable<Uri> sources, string target) : this(sources, target, -1)
        {}

        /// <summary>
        /// Creates a new download thread with a single source and a predefined file size.
        /// </summary>
        /// <param name="source">The URL the file can be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        /// <param name="bytesTotal">The number of bytes the file to be downloaded is long. The file will be rejected if it does not have this length.</param>
        public DownloadFile(Uri source, string target, long bytesTotal) : this(new[] { source }, target, bytesTotal)
        {}

        /// <summary>
        /// Creates a new download thread with a single source and no predefined file size.
        /// </summary>
        /// <param name="source">The URL the file can be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        public DownloadFile(Uri source, string target) : this(new[] {source}, target)
        {}
        #endregion

        //--------------------//

        #region User control
        /// <summary>
        /// Starts executing the download in a background thread.
        /// </summary>
        /// <remarks>Calling this on a not <see cref="DownloadState.Ready"/> thread will have no effect.</remarks>
        public void Start()
        {
            lock (_stateLock)
            {
                if (State != DownloadState.Ready) return;

                State = DownloadState.Started; 
                _thread.Start();
            }
        }

        /// <summary>
        /// Stops executing the download.
        /// </summary>
        /// <param name="keepPartial">Set to <see langword="true"/> to keep partially downloaded files. Otherwise they will be deleted.</param>
        /// <remarks>Calling this on a not running thread will have no effect.</remarks>
        public void Cancel(bool keepPartial)
        {
            lock (_stateLock)
            {
                if (State != DownloadState.Started && State != DownloadState.GettingHeaders && State != DownloadState.GettingData) return;

                _thread.Abort();
                _thread.Join();
                State = DownloadState.Ready;

                if (keepPartial && File.Exists(Target))
                {
                    try { File.Delete(Target); }
                    #region Error handling
                    catch (IOException ex)
                    {
                        Log.Write("Unable to delete: " + Target + " (" + ex.Message + ")");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Write("Unable to delete: " + Target + " (" + ex.Message + ")");
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Changes the <see cref="State"/> from <see cref="DownloadState.Ready"/>, <see cref="DownloadState.GettingHeaders"/> or <see cref="DownloadState.GettingData"/> to <see cref="DownloadState.Paused"/>.
        /// </summary>
        /// <remarks>Calling this on a completed or failed thread will have no effect. This method is only relevant when using the <see cref="DownloadFile"/> as a part of a <see cref="DownloadJob"/>.</remarks>
        public void Pause()
        {
            lock (_stateLock)
            {
                if (State >= DownloadState.Complete) return;

                if (_thread.IsAlive)
                {
                    _thread.Abort();
                    _thread.Join();
                }
                State = DownloadState.Paused;
            }
        }

        /// <summary>
        /// Changes the <see cref="State"/> from <see cref="DownloadState.Paused"/> back to ready <see cref="DownloadState.Ready"/>. 
        /// </summary>
        /// <remarks>This method is only relevant when using the <see cref="DownloadFile"/> as a part of a <see cref="DownloadJob"/>.</remarks>
        public void Resume()
        {
            lock (_stateLock)
            {
                if (State != DownloadState.Paused) return;

                State = DownloadState.Ready;
            }
        }

        /// <summary>
        /// Blocks until the download is completed or terminated.
        /// </summary>
        /// <remarks>Calling this on a not running thread will return immediately.</remarks>
        public void Join()
        {
            lock (_stateLock)
            {
                if (!_thread.IsAlive) return;

                _thread.Join();
            }
        }

        /// <summary>
        /// Runs the download code in the current thread instead of a background thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="State"/> is not <see cref="DownloadState.Ready"/>.</exception>
        public void RunSync()
        {
            lock (_stateLock)
            {
                if (State != DownloadState.Ready) throw new InvalidOperationException("Sate must be ready.");

                RunDownload();
            }
        }
        #endregion

        #region Thread code
        /// <summary>
        /// The actual download code to be executed by a background thread.
        /// </summary>
        private void RunDownload()
        {
            // Select a specific download source
            Uri source;
            if (_sources.Count == 1) source = _sources.Choose();
            else
            {
                // ToDo: Select best source by response speed
                source = _sources.Choose();
            }

            // ToDo: Check if BITS downloading is available

            // Prepare download request
            var webRequest = WebRequest.Create(source);
            
            // Check whether a preexisting file shall be continued
            bool resume = File.Exists(Target);
            
            try
            {
                // Open the target file for writing
                using (FileStream fileStream = File.Open(Target, resume ? FileMode.Append : FileMode.Create, FileAccess.Write))
                {
                    #region Download Resume handling
                    if (resume)
                    {
                        // Use Range header for HTTP resuming
                        var httpWebRequest = webRequest as HttpWebRequest;
                        if (httpWebRequest != null)
                        {
                            // Handle reuming of verly large files
                            if (fileStream.Position > int.MaxValue) fileStream.Position = int.MaxValue;
                            httpWebRequest.AddRange((int)fileStream.Position);
                        }
                        else
                        {
                            // Use ContentOffset header for FTP resuming
                            var ftpWebRequest = webRequest as FtpWebRequest;
                            if (ftpWebRequest != null) ftpWebRequest.ContentOffset = fileStream.Position;
                            else throw new InvalidOperationException(Resources.HttpAndFtpOnly);
                        }
                    }
                    #endregion

                    State = DownloadState.GettingHeaders;

                    // Start the server request
                    using (WebResponse response = webRequest.GetResponse())
                    {
                        // Get the header information
                        Headers = response.Headers;
                        EffectiveSource = response.ResponseUri;

                        // Determine file size and make sure predetermined sizes are valid
                        if (BytesTotal == -1) BytesTotal = response.ContentLength;
                        else if (BytesTotal != response.ContentLength)
                        {
                            ErrorMessage = Resources.FileNotExpectedSize;
                            State = DownloadState.WebError;
                        }

                        // HTTP servers with range-support and FTP servers support resuming downloads
                        SupportsResume = (Headers[HttpResponseHeader.AcceptRanges] == "bytes") || response is FtpWebResponse;

                        #region Download Resume handling
                        if (resume)
                        {
                            // Check whether an HTTP server ignored a range header
                            var httpWebResponse = response as HttpWebResponse;
                            if (httpWebResponse != null)
                            {
                                if (httpWebResponse.StatusCode != HttpStatusCode.PartialContent)
                                {
                                    // Clear the file and start from the beginning
                                    fileStream.SetLength(0);
                                }
                            }

                            // Update the download progress to reflect preexisting data
                            BytesReceived = fileStream.Position;
                        }
                        #endregion

                        State = DownloadState.GettingData;

                        // Start writing data to the file
                        int length;
                        Stream downloadStream = response.GetResponseStream();
                        var buffer = new byte[1024];
                        // Detect the end of the stream via a 0-write
                        while ((length = downloadStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, length);
                            BytesReceived += length;
                        }
                    }
                }
            }
            #region Error handling
            catch (WebException ex)
            {
                ErrorMessage = ex.Message;
                State = DownloadState.WebError;
                return;
            }
            catch (IOException ex)
            {
                ErrorMessage = ex.Message;
                State = DownloadState.IOError;
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMessage = ex.Message;
                State = DownloadState.IOError;
                return;
            }
            #endregion

            State = DownloadState.Complete;
        }
        #endregion

        //--------------------//

        #region BITS control
        /// <summary>
        /// To be called when <see cref="Priority"/> has changed to inform the BITS service.
        /// </summary>
        private void UpdatePriority()
        {
            // ToDo: Implement
        }
        #endregion
    }
}
