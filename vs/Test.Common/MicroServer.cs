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
using System.IO;
using System.Net;
using System.Threading;
using Common.Streams;

namespace Common
{
    /// <summary>
    /// Provides a minimalistic HTTP webserver that can provide only a single file. Useful for testing download code.
    /// </summary>
    public sealed class MicroServer : IDisposable
    {
        #region Variables
        /// <summary>
        /// A global port counter used to make sure no two instances of the <see cref="MicroServer"/> are listening on the same port.
        /// </summary>
        private static int _port = 50222;

        private readonly HttpListener _listener = new HttpListener();
        private readonly Thread _listenerThread;
        private readonly Stream _fileContent;
        #endregion

        #region Properties
        /// <summary>
        /// The complete URL under which the server provides its file.
        /// </summary>
        public Uri FileUri { get; private set; }

        /// <summary>
        /// Wait for ten seconds every time before finishing a response.
        /// </summary>
        public bool Slow { get; set; }
        #endregion
        
        //--------------------//

        #region Constructor
        /// <summary>
        /// Starts a HTTP webserver that listens on a random port.
        /// </summary>
        /// <param name="fileContent">The content of the file to serve. This stream will be closed when <see cref="Dispose"/> is called.</param>
        public MicroServer(Stream fileContent)
        {
            _fileContent = fileContent;

            // Determine URI to listen for
            string prefix = "http://localhost:" + _port++ + "/";
            _listener.Prefixes.Add(prefix);
            FileUri = new Uri(prefix + "file");

            // Start listening
            _listenerThread = new Thread(Listen);
            _listener.Start();
            _listenerThread.Start();
        }
        #endregion

        #region Thread control
        /// <summary>
        /// Stops listening for incoming HTTP connections.
        /// </summary>
        public void Dispose()
        {
            _listener.Close();
            _fileContent.Dispose();
        }
        #endregion

        #region Thread code
        /// <summary>
        /// Waits for HTTP requests and responds to them if they ask for "file".
        /// </summary>
        private void Listen()
        {
            HttpListenerContext context;
            while (_listener.IsListening)
            {
                try {
                    context = _listener.GetContext();

                    // Only return one specific file
                    if (context.Request.RawUrl == "/file")
                        StreamUtils.Copy(_fileContent, context.Response.OutputStream);
                    else
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                    // Delay finishing the file transfer if Slow-mode is active
                    if (Slow) Thread.Sleep(10000);

                    context.Response.OutputStream.Close();
                }
                #region Error handling
                catch (HttpListenerException)
                { return; }
                catch (InvalidOperationException)
                { return; }
                #endregion
            }
        }
        #endregion
    }
}
