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
        private readonly FileInfo _archive;
        private HttpListener _listener;
        private Thread _listenerThread;

        public ArchiveProvider(string archive)
        {
            this._archive = new FileInfo(archive);
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
            context.Response.ContentLength64 = _archive.Length;
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            using (var responseStream = context.Response.OutputStream)
            using (var archiveStream = new BinaryReader(_archive.OpenRead()))
            {
                int length = (int)_archive.Length;
                byte[] data = archiveStream.ReadBytes(length);
                var responseWriter = new BinaryWriter(responseStream);
                responseWriter.Write(data);
            }
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
