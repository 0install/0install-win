/*
 * Copyright 2006-2011 Bastian Eicher
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
using Common.Properties;

namespace Common.Net
{
    /// <summary>
    /// Downloads a file from a specific internet address to a local file (optionally as a background task).
    /// </summary>
    /// <remarks>Can be used stand-alone or as a part of a <see cref="DownloadJob"/>.</remarks>
    // ToDo: Fix resuming of downloads
    // ToDo: Set local last-changed time
    public class DownloadFile : TaskBase
    {
        #region Variables
        /// <summary>Flag that indicates the current process should be canceled.</summary>
        private volatile bool _cancelRequest;
        #endregion

        #region Properties
        /// <inheritdoc />
        public override string Name { get { return string.Format(Resources.Downloading, Source); } }

        /// <summary>
        /// The URL the file is to be downloaded from.
        /// </summary>
        /// <remarks>This value may change once <see cref="TaskState.Data"/> has been reached, based on HTTP redirections.</remarks>
        [Description("The URL the file is to be downloaded from.")]
        public Uri Source { get; private set; }

        /// <summary>
        /// The HTTP header data returned by the server for the download request. An empty collection in case of an FTP download.
        /// </summary>
        /// <remarks>This value is always <see langword="null"/> until <see cref="TaskState.Data"/> has been reached.</remarks>
        public WebHeaderCollection Headers { get; private set; }

        /// <summary>
        /// Indicates whether this download can be resumed without having to start from the beginning again.
        /// </summary>
        /// <remarks>This value is always <see langword="true"/> until <see cref="TaskState.Data"/> has been reached.</remarks>
        [Description("Indicates whether this download can be resumed without having to start from the beginning again.")]
        public bool SupportsResume { get; private set; }

        /// <summary>
        /// The local path to save the file to.
        /// </summary>
        [Description("The local path to save the file to.")]
        public string Target { get; protected set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download task with a predefined file size.
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
        }

        /// <summary>
        /// Creates a new download task with no fixed file size.
        /// </summary>
        /// <param name="source">The URL the file is to be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file is treated as partial download and attempted to be resumed.</param>
        public DownloadFile(Uri source, string target) : this(source, target, -1)
        {}
        #endregion

        //--------------------//

        #region Control
        /// <inheritdoc />
        public override void Cancel()
        {
            lock (StateLock)
            {
                if (_cancelRequest || State == TaskState.Ready || State >= TaskState.Complete) return;

                _cancelRequest = true;
                Thread.Join();

                // Reset the state so the task can be started again
                State = TaskState.Ready;
                _cancelRequest = false;
            }
        }
        #endregion

        #region Thread code
        /// <inheritdoc />
        protected override void RunTask()
        {
            try
            {
                var request = WebRequest.Create(Source);

                // Open the target file for writing
                using (FileStream fileStream = File.Open(Target, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    // Configure the request to continue the file transfer where the file ends
                    if (fileStream.Length != 0) SetResumePoint(request, fileStream);

                    if (_cancelRequest) return;
                    lock (StateLock) State = TaskState.Header;

                    // Start the server request, allowing for cancellation
                    var responseRequest = request.BeginGetResponse(null, null);
                    while (!responseRequest.AsyncWaitHandle.WaitOne(100))
                    {
                        if (_cancelRequest) return;
                    }

                    // Process the response
                    using (WebResponse response = request.EndGetResponse(responseRequest))
                    {
                        if (_cancelRequest) return;
                        lock (StateLock)
                        {
                            if (!ReadHeader(response)) return;

                            // If a partial file exists locally...
                            if (fileStream.Length != 0)
                            {
                                // ... make sure resuming worked on the server side
                                if (EnsureResumePoint(response))
                                {
                                    // Update the download progress to reflect preexisting data and move the file pointer to the end
                                    BytesProcessed = fileStream.Position = fileStream.Length;
                                }
                                else
                                {
                                    // Delete the preexisiting content and start over
                                    fileStream.SetLength(0);
                                }
                            }

                            State = TaskState.Data;
                        }

                        // Start writing data to the file
                        if (response != null) WriteStreamToTarget(response.GetResponseStream(), fileStream);
                        if (_cancelRequest) return;
                    }
                }
            }
            #region Error handling
            catch (WebException ex)
            {
                lock (StateLock)
                {
                    ErrorMessage = ex.Message;
                    State = TaskState.WebError;
                }
                return;
            }
            catch (IOException ex)
            {
                lock (StateLock)
                {
                    ErrorMessage = ex.Message;
                    State = TaskState.IOError;
                }
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                lock (StateLock)
                {
                    ErrorMessage = ex.Message;
                    State = TaskState.IOError;
                }
                return;
            }
            #endregion

            if (_cancelRequest) return;
            lock (StateLock) State = TaskState.Complete;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Reads the header information in the <paramref name="response"/> and stores it the object properties.
        /// </summary>
        /// <returns><see langword="true"/> if everything is ok; <see langword="false"/> if there was an error.</returns>
        private bool ReadHeader(WebResponse response)
        {
            Headers = response.Headers;

            // Update the source URL to reflect changes made by HTTP redirection
            Source = response.ResponseUri;

            // Determine file size and make sure predetermined sizes are valid
            if (BytesTotal == -1 || response.ContentLength == -1) BytesTotal = response.ContentLength;
            else if (BytesTotal != response.ContentLength)
            {
                ErrorMessage = string.Format(Resources.FileNotExpectedSize, Source, BytesTotal, response.ContentLength);
                State = TaskState.WebError;
                return false;
            }

            // HTTP servers with range-support and FTP servers support resuming downloads
            SupportsResume = (Headers[HttpResponseHeader.AcceptRanges] == "bytes") || response is FtpWebResponse;
            return true;
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
                // Handle resuming of verly large files by simply trimming part of the content
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

            // Write the response data to the file, allowing for cancellation
            while ((length = webStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, length);
                if (_cancelRequest) return;
                lock (StateLock) BytesProcessed += length;
            }
        }
        #endregion
    }
}
