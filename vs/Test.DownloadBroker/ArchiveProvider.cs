using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;

namespace ZeroInstall.DownloadBroker
{
    public class ArchiveProvider : IDisposable
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
            var context = _listener.GetContext();
            context.Response.ContentLength64 = _archive.Length;
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            using (var archiveStream = _archive.OpenRead())
            using (var responseStream = context.Response.OutputStream)
            {
                int length = (int)_archive.Length;
                byte[] data = new byte[length];
                archiveStream.Read(data, 0, length);
                responseStream.Write(data, 0, length);
            }
        }

        public void Dispose()
        {
            _listenerThread.Abort();
            _listener.Stop();
        }
    }
}
