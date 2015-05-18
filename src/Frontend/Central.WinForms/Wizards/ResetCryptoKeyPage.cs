/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Central.WinForms.Wizards
{
    internal partial class ResetCryptoKeyPage : SyncPage, IWizardPage
    {
        public SyncServer Server;

        public event Action<string> NewKeySet;

        public ResetCryptoKeyPage(bool machineWide) : base(machineWide)
        {
            InitializeComponent();

            textBoxCryptoKey.Text = StringUtils.GeneratePassword(16);
        }

        public void OnPageShow()
        {
            var parent = Parent as Form;
            if (parent != null) parent.AcceptButton = buttonReset;
        }

        private void textBoxCryptoKey_TextChanged(object sender, EventArgs e)
        {
            buttonReset.Enabled = !string.IsNullOrEmpty(textBoxCryptoKey.Text);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            Parent.Parent.Enabled = buttonReset.Visible = false;
            labelWorking.Visible = true;

            resetWorker.RunWorkerAsync(textBoxCryptoKey.Text);
        }

        private void resetWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var newKey = (string)e.Argument;
            using (var sync = CreateSync(Server, newKey, MachineWide))
                sync.Sync(SyncResetMode.Server);
        }

        private void resetWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            labelWorking.Visible = false;
            Parent.Parent.Enabled = buttonReset.Visible = true;

            if (e.Error == null) NewKeySet(textBoxCryptoKey.Text);
            else if (!(e.Error is OperationCanceledException)) Msg.Inform(this, e.Error.Message, MsgSeverity.Error);
        }
    }
}
