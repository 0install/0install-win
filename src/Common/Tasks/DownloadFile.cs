/*
 * Copyright 2006-2014 Bastian Eicher
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
using NanoByte.Common.Properties;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Downloads a file from a specific internet address to a local file (optionally as a background task).
    /// </summary>
    public class DownloadFile : TaskBase
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return string.Format(Resources.Downloading, Source); } }

        /// <inheritdoc/>
        protected override bool UnitsByte { get { return true; } }

        /// <summary>
        /// The URL the file is to be downloaded from.
        /// </summary>
        /// <remarks>This value may change once <see cref="TaskStatus.Data"/> has been reached, based on HTTP redirections.</remarks>
        [Description("The URL the file is to be downloaded from.")]
        public Uri Source { get; private set; }

        /// <summary>
        /// The HTTP header data returned by the server for the download request. An empty collection in case of an FTP download.
        /// </summary>
        /// <remarks>This value is always <see langword="null"/> until <see cref="TaskStatus.Data"/> has been reached.</remarks>
        public WebHeaderCollection Headers { get; private set; }

        /// <summary>
        /// The local path to save the file to.
        /// </summary>
        [Description("The local path to save the file to.")]
        public string Target { get; protected set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download task.
        /// </summary>
        /// <param name="source">The URL the file is to be downloaded from.</param>
        /// <param name="target">The local path to save the file to. A preexisting file will be overwritten.</param>
        /// <param name="bytesTotal">The number of bytes the file to be downloaded is long. The file will be rejected if it does not have this length. -1 if the size is unknown.</param>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="source"/> contains an unsupported protocol (usually should be HTTP or FTP).</exception>
        public DownloadFile(Uri source, string target, long bytesTotal = -1)
        {
            #region Sanity checks
            if (source == null) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            Source = source;
            Target = target;
            UnitsTotal = bytesTotal;
        }
        #endregion

        //--------------------//

        #region Thread code
        /// <inheritdoc/>
        protected override void Execute()
        {
            var request = WebRequest.Create(Source);

            // Open the target file for writing
            using (FileStream fileStream = File.Open(Target, FileMode.OpenOrCreate, FileAccess.Write))
            {
                Status = TaskStatus.Header;

                // ReSharper disable AssignNullToNotNullAttribute
                var responseRequest = request.BeginGetResponse(null, null);
                // ReSharper restore AssignNullToNotNullAttribute

                // Wait for the download request to complete or a cancel request to arrive
                if (WaitHandle.WaitAny(new[] {responseRequest.AsyncWaitHandle, CancellationToken.WaitHandle}) == 1)
                    throw new OperationCanceledException();

                // Process the response
                using (WebResponse response = request.EndGetResponse(responseRequest))
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    ReadHeader(response);
                    Status = TaskStatus.Data;

                    // Start writing data to the file
                    if (response != null) WriteStreamToTarget(response.GetResponseStream(), fileStream);
                }
            }

            Status = TaskStatus.Complete;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Reads the header information in the <paramref name="response"/> and stores it the object properties.
        /// </summary>
        /// <returns><see langword="true"/> if everything is ok; <see langword="false"/> if there was an error.</returns>
        private void ReadHeader(WebResponse response)
        {
            Headers = response.Headers;

            // Update the source URL to reflect changes made by HTTP redirection
            Source = response.ResponseUri;

            // Determine file size and make sure predetermined sizes are valid
            if (UnitsTotal == -1 || response.ContentLength == -1) UnitsTotal = response.ContentLength;
            else if (UnitsTotal != response.ContentLength)
                throw new WebException(string.Format(Resources.FileNotExpectedSize, Source, UnitsTotal, response.ContentLength));
        }

        /// <summary>
        /// Writes the content of <paramref name="webStream"/> to <paramref name="fileStream"/>.
        /// </summary>
        private void WriteStreamToTarget(Stream webStream, Stream fileStream)
        {
            var buffer = new byte[8 * 1024];
            long bytesDownloaded = 0;
            var lastProgressReport = new DateTime();

            // Write the response data to the file, allowing for cancellation
            int length;
            while ((length = webStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, length);
                bytesDownloaded += length;
                CancellationToken.ThrowIfCancellationRequested();

                // Only report progress once every 250ms
                if (DateTime.UtcNow - lastProgressReport >= new TimeSpan(0, 0, 0, 0, 250))
                {
                    lastProgressReport = DateTime.UtcNow;
                    UnitsProcessed = bytesDownloaded;
                }
            }
        }
        #endregion
    }
}
