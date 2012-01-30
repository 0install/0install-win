/*
 * Copyright 2010-2012 Bastian Eicher
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
    internal partial class CredentialsPage : UserControl
    {
        public Uri SyncServer;

        public event Action<SyncCredentials> Continue;

        public CredentialsPage()
        {
            InitializeComponent();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            buttonContinue.Enabled = !string.IsNullOrEmpty(textBoxUsername.Text) && !string.IsNullOrEmpty(textBoxPassword.Text);
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            Parent.Parent.Enabled = false;
            credentialsCheckWorker.RunWorkerAsync(new SyncCredentials(textBoxUsername.Text, textBoxPassword.Text));
        }

        private void credentialsCheckWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckCredentials(SyncServer, (SyncCredentials)e.Argument);
        }

        private void credentialsCheckWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Parent.Parent.Enabled = true;
            if (e.Error == null) Continue(new SyncCredentials(textBoxUsername.Text, textBoxPassword.Text));
            else Msg.Inform(this, e.Error.Message, MsgSeverity.Error);
        }

        private static void CheckCredentials(Uri syncServer, SyncCredentials credentials)
        {
            if (!syncServer.ToString().EndsWith("/")) syncServer = new Uri(syncServer + "/"); // Ensure the server URI references a directory
            var appListUri = new Uri(syncServer, new Uri("app-list", UriKind.Relative));

            var request = WebRequest.Create(appListUri);
            request.Method = "HEAD";
            request.Credentials = new NetworkCredential(credentials.Username, credentials.Password);
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            request.Timeout = 10000; // 10 seconds

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
