/*
 * Copyright 2010-2016 Bastian Eicher
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

using System.IO;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Native;
using ZeroInstall.Central.Properties;
using ZeroInstall.Commands.CliCommands;

namespace ZeroInstall.Central.WinForms
{
    public partial class PortableCreatorDialog : Form
    {
        public PortableCreatorDialog()
        {
            InitializeComponent();
        }

        private void buttonSelectTarget_Click(object sender, System.EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog {SelectedPath = textBoxTarget.Text})
            {
                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                    textBoxTarget.Text = folderBrowserDialog.SelectedPath;
            }

            textBoxTarget.Focus();
        }

        private void textBoxTarget_TextChanged(object sender, System.EventArgs e)
        {
            buttonDeploy.Enabled = !string.IsNullOrEmpty(textBoxTarget.Text);
        }

        private void buttonDeploy_Click(object sender, System.EventArgs e)
        {
            if (Directory.Exists(textBoxTarget.Text) && Directory.GetFileSystemEntries(textBoxTarget.Text).Length != 0)
            {
                if (!Msg.YesNo(this, string.Format(Resources.PortableDirNotEmptyAsk, textBoxTarget.Text), MsgSeverity.Warn))
                    return;
            }

            Program.RunCommand(MaintenanceMan.Name, "deploy", "--portable", textBoxTarget.Text);
            Close();
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
