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
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Net;
using ZeroInstall.Central.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Central.WinForms.Wizards
{
    internal partial class CredentialsPage : UserControl, IWizardPage
    {
        public Uri ServerUri;

        public event Action<SyncServer> CredentialsSet;

        public CredentialsPage()
        {
            InitializeComponent();
        }

        public void OnPageShow()
        {
            var parent = Parent as Form;
            if (parent != null) parent.AcceptButton = buttonNext;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            buttonNext.Enabled = !string.IsNullOrEmpty(textBoxUsername.Text) && !string.IsNullOrEmpty(textBoxPassword.Text);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            Parent.Parent.Enabled = false;
            credentialsCheckWorker.RunWorkerAsync(new SyncServer
            {
                Uri = ServerUri,
                Username = textBoxUsername.Text,
                Password = textBoxPassword.Text
            });
        }

        private void credentialsCheckWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckCredentials((SyncServer)e.Argument);
        }

        private void credentialsCheckWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Parent.Parent.Enabled = true;
            if (e.Error == null)
            {
                CredentialsSet(new SyncServer
                {
                    Uri = ServerUri,
                    Username = textBoxUsername.Text,
                    Password = textBoxPassword.Text
                });
            }
            else if (!(e.Error is OperationCanceledException)) Msg.Inform(this, e.Error.Message, MsgSeverity.Error);
        }

        private static void CheckCredentials(SyncServer server)
        {
            var appListUri = new Uri(server.Uri, new Uri("app-list", UriKind.Relative));

            var request = WebRequest.Create(appListUri);
            request.Method = "HEAD";
            request.Credentials = server.Credentials;
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            request.Timeout = WebClientTimeout.DefaultTimeout;

            try
            {
                request.GetResponse();
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
            #endregion
        }
    }
}
