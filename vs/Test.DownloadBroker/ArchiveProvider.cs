using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;

namespace ZeroInstall.DownloadBroker
{
    public sealed class ArchiveProvider : IDisposable
    {
        private readonly Dictionary<string, string> _hostedFiles;
        private HttpListener _listener;
        private Thread _listenerThread;

        public ArchiveProvider(string archive)
        {
            _hostedFiles = new Dictionary<string, string>();
            _hostedFiles.Add("test.zip", archive);
        }

        public void Add(string name, string archive)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(archive)) throw new ArgumentNullException("archive");

            if (!File.Exists(archive)) throw new IOException(archive + ": File not found");

            _hostedFiles.Add(name, archive);
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:50222/archives/");
            _listener.Start();

            _listenerThread = new Thread(Listen);
            _listenerThread.Start();
        }

        private void Listen()
        {
            HttpListenerContext context;
            try
            {
                context = _listener.GetContext();
            }
            catch (HttpListenerException)
            { return; }
            catch (InvalidOperationException)
            { return; }

            string name = context.Request.Url.LocalPath.Substring("/archives/".Length);
            string archivePath;
            _hostedFiles.TryGetValue(name, out archivePath);
            if (archivePath != null)
            {
                AnswerWithFileData(context, archivePath);
            }
            else
            {
                AnswerWithNotFound(context);
            }
        }

        private static void AnswerWithFileData(HttpListenerContext context, string archivePath)
        {
            byte[] data = File.ReadAllBytes(archivePath);
            context.Response.ContentLength64 = data.LongLength;
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            using (var responseStream = context.Response.OutputStream)
            {
                var responseWriter = new BinaryWriter(responseStream);
                responseWriter.Write(data);
            }
        }

        private static void AnswerWithNotFound(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.OutputStream.Close();
        }

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.Stop();
            }
            if (_listenerThread != null && _listenerThread.ThreadState == ThreadState.Running)
            {
                _listenerThread.Join();
            }
            if (_listener != null)
            {
                _listener.Close();
            }
        }
    }
}
