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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using Common.Helpers;
using Common.Properties;

namespace Common.Download
{
    #region Delegates
    /// <summary>
    /// Generic delegate for handling an event without passing any parameters.
    /// </summary>
    public delegate void DownloadFileEventHandler(DownloadFile sender);
    #endregion

    #region Enumerations
    /// <seealso cref="DownloadFile.State"/>
    public enum DownloadState
    {
        /// <summary>The download is ready to begin.</summary>
        Ready,
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
    /// <remarks>Can be used stand-alone or as a part of a <see cref="DownloadJob"/>.</remarks>
    public class DownloadFile
    {
        #region Events
        /// <summary>
        /// Occurs whenever <see cref="State"/> changes. Blocks the download thread, so handle quickly!
        /// </summary>
        /// <remarks>This event is executed in a background thread. It must not be used to directly update UI elements.</remarks>
        public event DownloadFileEventHandler StateChanged;

        private void OnStateChanged()
        {
            // Copy to local variable to prevent threading issues
            DownloadFileEventHandler stateChanged = StateChanged;
            if (stateChanged != null) stateChanged(this);
        }

        /// <summary>
        /// Occurs whenever <see cref="BytesReceived"/> changes. Blocks the download thread, so handle quickly!
        /// </summary>
        /// <remarks>This event is executed in a background thread. It must not be used to directly update UI elements.</remarks>
        public event DownloadFileEventHandler BytesReceivedChanged;

        private void OnBytesReceivedChanged()
        {
            // Copy to local variable to prevent threading issues
            DownloadFileEventHandler bytesReceivedChanged = BytesReceivedChanged;
            if (bytesReceivedChanged != null) bytesReceivedChanged(this);
        }
        #endregion

        #region Variables
        /// <summary>Synchronization handle to prevent race conditions with thread startup/shutdown or <see cref="State"/> switched.</summary>
        private readonly object _stateLock = new object();
        
        /// <summary>The background thread used for downloading.</summary>
        private readonly Thread _thread;
        #endregion

        #region Properties
        /// <summary>
        /// The URL the file is to be downloaded from.
        /// </summary>
        /// <remarks>This value may change once <see cref="DownloadState.GettingData"/> has been reached, based on HTTP redirections.</remarks>
        [Description("The URL the file is to be downloaded from.")]
        public Uri Source { get; private set; }

        /// <summary>
        /// The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.
        /// </summary>
        [Description("The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.")]
        public string Target { get; private set; }

        private DownloadState _state;
        /// <summary>
        /// The current status of the download.
        /// </summary>
        [Description("The current status of the download.")]
        public DownloadState State
        {
            get { return _state; }
            private set { UpdateHelper.Do(ref _state, value, OnStateChanged); }
        }

        /// <summary>
        /// Contains an error description if <see cref="State"/> is set to <see cref="DownloadState.WebError"/> or <see cref="DownloadState.IOError"/>.
        /// </summary>
        [Description("Contains an error description if State is set to WebError or IOError.")]
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
        [Description("Indicates whether this download can be resumed without having to start from the beginning again.")]
        public bool SupportsResume { get; private set; }

        private long _bytesReceived;
        /// <summary>
        /// The number of bytes that have been downloaded so far.
        /// </summary>
        [Description("The number of bytes that have been downloaded so far.")]
        public long BytesReceived
        {
            get { return _bytesReceived; }
            private set { UpdateHelper.Do(ref _bytesReceived, value, OnBytesReceivedChanged); }
        }

        /// <summary>
        /// The number of bytes the file to be downloaded is long; -1 for unknown.
        /// </summary>
        /// <remarks>If this value is set to -1 in the constructor, the size be automatically set after <see cref="DownloadState.GettingData"/> has been reached.</remarks>
        [Description("The number of bytes the file to be downloaded is long; -1 for unknown.")]
        public long BytesTotal { get; private set; }

        /// <summary>
        /// The progress of the download as a value between 0 and 1; -1 when unknown.
        /// </summary>
        public double Progress
        {
            get
            {
                switch (BytesTotal)
                {
                    case -1: return -1;
                    case 0: return 0;
                    default: return BytesReceived / (float)BytesTotal;
                }
            }
        }

        /// <summary>
        /// <see langword="true"/> if <see cref="RunSync"/> was called.
        /// </summary>
        public bool RunningSync { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download thread with a predefined file size.
        /// </summary>
        /// <param name="source">The URL the file is to be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        /// <param name="bytesTotal">The number of bytes the file to be downloaded is long. The file will be rejected if it does not have this length.</param>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="source"/> contains an unsupported protocol (usually should be HTTP or FTP).</exception>
        public DownloadFile(Uri source, string target, long bytesTotal)
        {
            #region Sanity checks
            if (source == null) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            Source = source;
            Target = target;
            BytesTotal = bytesTotal;

            // Prepare the background thread for later execution
            _thread = new Thread(RunDownload) {Name = "Download: " + target, IsBackground = true};
        }
        
        /// <summary>
        /// Creates a new download thread with a predefined file size.
        /// </summary>
        /// <param name="source">The URL the file is to be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        /// <param name="bytesTotal">The number of bytes the file to be downloaded is long. The file will be rejected if it does not have this length.</param>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="source"/> contains an unsupported protocol (usually should be HTTP or FTP).</exception>
        public DownloadFile(string source, string target, long bytesTotal) : this(new Uri(source), target, bytesTotal)
        {}

        /// <summary>
        /// Creates a new download thread with no fixed file size.
        /// </summary>
        /// <param name="source">The URL the file is to be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        public DownloadFile(Uri source, string target) : this(source, target, -1)
        {}

        /// <summary>
        /// Creates a new download thread with no fixed file size.
        /// </summary>
        /// <param name="source">The URL the file is to be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        public DownloadFile(string source, string target) : this(new Uri(source), target)
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
                if (State != DownloadState.Ready || RunningSync) return;

                State = DownloadState.Started; 
                _thread.Start();
            }
        }

        /// <summary>
        /// Stops executing the download.
        /// </summary>
        /// <param name="keepPartial">Set to <see langword="true"/> to keep partially downloaded files; <see langword="false"/> to delete them.</param>
        /// <remarks>Calling this on a not running thread will have no effect.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if called while a synchronous download is running (launched via <see cref="RunSync"/>).</exception>
        public void Cancel(bool keepPartial)
        {
            lock (_stateLock)
            {
                if (RunningSync) throw new InvalidOperationException(Resources.CannotCancelSync);
                if (State == DownloadState.Ready || State >= DownloadState.Complete) return;

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
        /// Blocks until the download is completed or terminated.
        /// </summary>
        /// <remarks>Calling this on a not running thread will return immediately.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if called while a synchronous download is running (launched via <see cref="RunSync"/>).</exception>
        public void Join()
        {
            lock (_stateLock)
            {
                if (RunningSync) throw new InvalidOperationException(Resources.CannotJoinSync);
                if (_thread == null || !_thread.IsAlive) return;
            }

            _thread.Join();
        }

        /// <summary>
        /// Runs the download code in the current thread instead of a background thread.
        /// </summary>
        /// <exception cref="WebException">Thrown if the downloaded ended with <see cref="DownloadState.WebError"/>.</exception>
        /// <exception cref="IOException">Thrown if the downloaded ended with <see cref="DownloadState.IOError"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="State"/> is not <see cref="DownloadState.Ready"/>.</exception>
        public void RunSync()
        {
            lock (_stateLock)
            {
                if (State != DownloadState.Ready) throw new InvalidOperationException(Resources.StateMustBeReady);
                RunningSync = true;
            }

            RunDownload();

            lock (_stateLock)
            {
                RunningSync = false;
                if (State == DownloadState.WebError) throw new WebException(ErrorMessage);
                if (State == DownloadState.IOError) throw new IOException(ErrorMessage);
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Reads the header information in the <paramref name="response"/> and stores it the object properties.
        /// </summary>
        private void ReadHeader(WebResponse response)
        {
            Headers = response.Headers;

            // Update the source URL to reflect changes made by HTTP redirection
            Source = response.ResponseUri;

            // Determine file size and make sure predetermined sizes are valid
            if (BytesTotal == -1) BytesTotal = response.ContentLength;
            else if (BytesTotal != response.ContentLength)
            {
                lock (_stateLock)
                {
                    ErrorMessage = Resources.FileNotExpectedSize;
                    State = DownloadState.WebError;
                }
            }

            // HTTP servers with range-support and FTP servers support resuming downloads
            SupportsResume = (Headers[HttpResponseHeader.AcceptRanges] == "bytes") || response is FtpWebResponse;
        }

        /// <summary>
        /// Configures the <paramref name="request"/> t start downloading at the of a <paramref name="fileStream"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if the request use an unsupported protocol (usually should be HTTP or FTP).</exception>
        private static void SetResumePoint(WebRequest request, Stream fileStream)
        {
            // Use Range header for HTTP resuming
            var httpWebRequest = request as HttpWebRequest;
            if (httpWebRequest != null)
            {
                // Handle reuming of verly large files by simply trimming part of the content
                if (fileStream.Length > int.MaxValue) fileStream.SetLength(int.MaxValue);

                httpWebRequest.AddRange((int)fileStream.Length);
            }
            else
            {
                var ftpWebRequest = request as FtpWebRequest;
                if (ftpWebRequest != null) ftpWebRequest.ContentOffset = fileStream.Position;
                else throw new NotSupportedException(Resources.HttpAndFtpOnly);
            }
        }

        /// <summary>
        /// Ensures a <paramref name="response"/> is actually using a resume point set by <see cref="SetResumePoint"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if the request use an unsupported protocol (usually should be HTTP or FTP).</exception>
        private static bool EnsureResumePoint(WebResponse response)
        {
            var httpWebResponse = response as HttpWebResponse;
            if (httpWebResponse != null)
            {
                // Check whether an HTTP server ignored a range header
                return (httpWebResponse.StatusCode == HttpStatusCode.PartialContent);
            }
            
            // Assume FTP resuming always works
            if (response is FtpWebResponse) return true;

            throw new NotSupportedException(Resources.HttpAndFtpOnly);
        }

        /// <summary>
        /// Writes the content of <paramref name="webStream"/> to <paramref name="fileStream"/>.
        /// </summary>
        private void WriteStreamToTarget(Stream webStream, Stream fileStream)
        {
            int length;
            var buffer = new byte[1024];
            // Detect the end of the stream via a 0-write
            while ((length = webStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, length);
                lock (_stateLock) BytesReceived += length;
            }
        }
        #endregion

        #region Thread code
        /// <summary>
        /// The actual download code to be executed by a background thread.
        /// </summary>
        private void RunDownload()
        {
            try
            {
                var request = WebRequest.Create(Source);

                // Open the target file for writing
                using (FileStream fileStream = File.Open(Target, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    // Configure the request to continue the file transfer where the file ends
                    if (fileStream.Length != 0) SetResumePoint(request, fileStream);

                    lock (_stateLock) State = DownloadState.GettingHeaders;

                    // Start the server request
                    using (WebResponse response = request.GetResponse())
                    {
                        lock (_stateLock)
                        {
                            ReadHeader(response);

                            // If  a partial file exists locally...
                            if (fileStream.Length != 0)
                            {
                                // ... make sure resuming worked on the server side
                                if (EnsureResumePoint(response))
                                {
                                    // Update the download progress to reflect preexisting data and move the file pointer to the end
                                    BytesReceived = fileStream.Position = fileStream.Length;
                                }
                                else
                                {
                                    // Delete the preexisiting content and start over
                                    fileStream.SetLength(0);
                                }
                            }

                            State = DownloadState.GettingData;
                        }

                        // Start writing data to the file
                        WriteStreamToTarget(response.GetResponseStream(), fileStream);
                    }
                }
            }
            #region Error handling
            catch (WebException ex)
            {
                lock (_stateLock)
                {
                    ErrorMessage = ex.Message;
                    State = DownloadState.WebError;
                }
                return;
            }
            catch (IOException ex)
            {
                lock (_stateLock)
                {
                    ErrorMessage = ex.Message;
                    State = DownloadState.IOError;
                }
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                lock (_stateLock)
                {
                    ErrorMessage = ex.Message;
                    State = DownloadState.IOError;
                }
                return;
            }
            #endregion

            State = DownloadState.Complete;
        }
        #endregion
    }
}
