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
    internal sealed class MicroServer : IDisposable
    {
        #region Variables
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
        /// Creates a new micro webserver.
        /// </summary>
        /// <param name="port">The port the server shall listen on. Should be greater than 1024.</param>
        /// <param name="fileContent">The content of the file to server.</param>
        public MicroServer(int port, Stream fileContent)
        {
            _listener.Prefixes.Add("http://localhost:" + port + "/");
            FileUri = new Uri("http://localhost:" + port + "/file");

            _listenerThread = new Thread(Listen);
            _fileContent = fileContent;
        }
        #endregion

        #region Thread control
        /// <summary>
        /// Starts listening for incoming HTTP connections.
        /// </summary>
        public void Start()
        {
            _listener.Start();
            _listenerThread.Start();
        }

        /// <summary>
        /// Stops listening for incoming HTTP connections.
        /// </summary>
        public void Dispose()
        {
            _listener.Stop();
            _listener.Prefixes.Clear();
            _listener.Close();
            _listenerThread.Join();
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
                try { context = _listener.GetContext(); }
                #region Error handling
                catch (HttpListenerException)
                { return; }
                catch (InvalidOperationException)
                { return; }
                #endregion

                // Only return one specific file
                if (context.Request.RawUrl == "/file")
                    StreamUtils.Copy(_fileContent, context.Response.OutputStream);
                else
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Delay finishing the file transfer if Slow-mode is active
                if (Slow) Thread.Sleep(10000);

                context.Response.OutputStream.Close();
            }
        }
        #endregion
    }
}
