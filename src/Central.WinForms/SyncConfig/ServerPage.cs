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
        public event SimpleEventHandler OfficialServer;
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
                if (!Msg.YesNo(this, Resources.UnencryptedSyncServer, MsgSeverity.Warn, Resources.YesWizardContinue, Resources.NoWizardReturn))
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
            else Msg.Inform(this, e.Error.Message, MsgSeverity.Error);
        }

        private void CheckServer(Uri syncServer)
        {
            if (!syncServer.ToString().EndsWith("/")) syncServer = new Uri(syncServer + "/"); // Ensure the server URI references a directory

            var request = WebRequest.Create(syncServer);
            request.Method = "HEAD";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            request.Timeout = 10000; // 10 seconds
            request.GetResponse();
        }
    }
}
