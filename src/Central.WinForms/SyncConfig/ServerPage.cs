/*
 * Copyright 2010-2013 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;
using Common;
using ZeroInstall.Central.WinForms.Properties;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal partial class ServerPage : UserControl
    {
        public event Action OfficialServer;
        public event Action<Uri> CustomServer;

        public ServerPage()
        {
            InitializeComponent();
        }

        private void textBoxCustomServer_TextChanged(object sender, EventArgs e)
        {
            buttonCustomServer.Enabled = textBoxCustomServer.IsValid && !string.IsNullOrEmpty(textBoxCustomServer.Text);
        }

        private void buttonOfficalServer_Click(object sender, EventArgs e)
        {
            OfficialServer();
        }

        private void buttonCustomServer_Click(object sender, EventArgs e)
        {
            if (textBoxCustomServer.Uri.Scheme != "https")
            {
                if (!Msg.YesNo(this, Resources.UnencryptedSyncServer, MsgSeverity.Warn))
                    return;
            }

            Parent.Parent.Enabled = false;
            serverCheckWorker.RunWorkerAsync(textBoxCustomServer.Uri);
        }

        private void serverCheckWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckServer((Uri)e.Argument);
        }

        private void serverCheckWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Parent.Parent.Enabled = true;
            if (e.Error == null) CustomServer(textBoxCustomServer.Uri);
            else if (!(e.Error is OperationCanceledException)) Msg.Inform(this, e.Error.Message, MsgSeverity.Error);
        }

        private static void CheckServer(Uri syncServer)
        {
            if (!syncServer.ToString().EndsWith("/")) syncServer = new Uri(syncServer + "/"); // Ensure the server URI references a directory

            var request = WebRequest.Create(syncServer);
            request.Method = "HEAD";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            request.Timeout = WebClientTimeout.DefaultTimeout;
            request.GetResponse();
        }
    }
}
