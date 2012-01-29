using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;
using Common;
using Common.Storage;
using ICSharpCode.SharpZipLib.Zip;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal partial class ExistingCryptoKeyPage : UserControl
    {
        public Uri SyncServer;
        public SyncCredentials SyncCredentials;

        public event Action<string> Continue;

        public ExistingCryptoKeyPage()
        {
            InitializeComponent();
        }

        private void textBoxCryptoKey_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = !string.IsNullOrEmpty(textBoxCryptoKey.Text);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Parent.Parent.Enabled = false;
            keyCheckWorker.RunWorkerAsync(textBoxCryptoKey.Text);
        }

        private void keyCheckWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckCryptoKey(SyncServer, SyncCredentials, (string)e.Argument);
        }

        private void keyCheckWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Parent.Parent.Enabled = true;
            if (e.Error == null) Continue(textBoxCryptoKey.Text);
            else
            {
                if (Msg.YesNo(this, e.Error.Message + "\n" + Resources.ContinueAndSolveLater, MsgSeverity.Warn, Resources.YesWizardContinue, Resources.NoWizardReturn))
                    Continue(textBoxCryptoKey.Text);
            }
        }

        private static void CheckCryptoKey(Uri syncServer, SyncCredentials syncCredentials, string key)
        {
            if (!syncServer.ToString().EndsWith("/")) syncServer = new Uri(syncServer + "/"); // Ensure the server URI references a directory
            var appListUri = new Uri(syncServer, new Uri("app-list", UriKind.Relative));

            using (var webClient = new WebClient
            {
                Credentials = new NetworkCredential(syncCredentials.Username, syncCredentials.Password),
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore),
            })
            {
                try
                {
                    XmlStorage.FromZip<AppList>(new MemoryStream(webClient.DownloadData(appListUri)), key, null);
                }
                    #region Error handling
                catch (WebException ex)
                {
                    // Wrap exception to add context information
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                            throw new WebException(Resources.SyncCredentialsInvalid, ex);
                    }

                    throw;
                }
                catch (ZipException ex)
                {
                    // Wrap exception to add context information
                    if (ex.Message == "Invalid password") throw new InvalidDataException(Resources.SyncCryptoKeyInvalid);
                    throw new InvalidDataException(Resources.SyncServerDataDamaged, ex);
                }
                catch (InvalidDataException ex)
                {
                    // Wrap exception to add context information
                    throw new InvalidDataException(Resources.SyncServerDataDamaged, ex);
                }
                #endregion
            }
        }
    }
}
