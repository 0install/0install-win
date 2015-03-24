/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Net;
using ZeroInstall.Central.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Central.WinForms.Wizards
{
    internal partial class ExistingCryptoKeyPage : UserControl, IWizardPage
    {
        public SyncServer Server;

        public event Action<string> OldKeySet;
        public event Action ResetKey;

        public ExistingCryptoKeyPage()
        {
            InitializeComponent();
            try
            {
                textBoxCryptoKey.Text = Config.Load().SyncCryptoKey;
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex);
            }
            #endregion
        }

        public void OnPageShow()
        {
            var parent = Parent as Form;
            if (parent != null) parent.AcceptButton = buttonNext;
        }

        private void textBoxCryptoKey_TextChanged(object sender, EventArgs e)
        {
            buttonNext.Enabled = !string.IsNullOrEmpty(textBoxCryptoKey.Text);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            Parent.Parent.Enabled = false;
            keyCheckWorker.RunWorkerAsync(textBoxCryptoKey.Text);
        }

        private void buttonForgotKey_Click(object sender, EventArgs e)
        {
            ResetKey();
        }

        private void keyCheckWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckCryptoKey(Server, (string)e.Argument);
        }

        private void keyCheckWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Parent.Parent.Enabled = true;
            if (e.Error == null) OldKeySet(textBoxCryptoKey.Text);
            else Msg.Inform(this, e.Error.Message, MsgSeverity.Warn);
        }

        private static void CheckCryptoKey(SyncServer server, string key)
        {
            var appListUri = new Uri(server.Uri, new Uri("app-list", UriKind.Relative));

            using (var webClient = new WebClientTimeout
            {
                Credentials = server.Credentials,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
            })
            {
                try
                {
                    AppList.LoadXmlZip(new MemoryStream(webClient.DownloadData(appListUri)), key);
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
                #endregion
            }
        }
    }
}
