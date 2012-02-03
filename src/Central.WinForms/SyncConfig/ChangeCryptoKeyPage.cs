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
using Common;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal partial class ChangeCryptoKeyPage : HandlerPage
    {
        public string OldKey;

        public event Action<string> Continue;

        public ChangeCryptoKeyPage()
        {
            InitializeComponent();
        }

        private void textBoxCryptoKey_TextChanged(object sender, EventArgs e)
        {
            buttonChange.Enabled = !string.IsNullOrEmpty(textBoxCryptoKey.Text);
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            Parent.Parent.Enabled = buttonChange.Visible = textBoxCryptoKey.Enabled = false;
            ShowProgressUI();

            resetWorker.RunWorkerAsync(textBoxCryptoKey.Text);
        }

        private void resetWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var newKey = (string)e.Argument;
            var policy = Policy.CreateDefault(this);
            using (var sync = new SyncIntegrationManager(false, policy.Config.SyncServer, policy.Config.SyncServerUsername, policy.Config.SyncServerPassword, OldKey, policy.Handler))
                sync.Sync(SyncResetMode.None, feedID => policy.FeedManager.GetFeed(feedID, policy));
            using (var sync = new SyncIntegrationManager(false, policy.Config.SyncServer, policy.Config.SyncServerUsername, policy.Config.SyncServerPassword, newKey, policy.Handler))
                sync.Sync(SyncResetMode.Server, feedID => policy.FeedManager.GetFeed(feedID, policy));
        }

        private void resetWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CloseProgressUI();
            Parent.Parent.Enabled = buttonChange.Visible = textBoxCryptoKey.Enabled = true;

            if (e.Error == null) Continue(textBoxCryptoKey.Text);
            else if (!(e.Error is OperationCanceledException)) Msg.Inform(this, e.Error.Message, MsgSeverity.Error);
        }
    }
}
