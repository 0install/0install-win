// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Native;
using ZeroInstall.Central.Properties;
using ZeroInstall.Commands.Desktop;

namespace ZeroInstall.Central.WinForms
{
    public partial class PortableCreatorDialog : Form
    {
        public PortableCreatorDialog()
        {
            InitializeComponent();
        }

        private void PortableCreatorDialog_Load(object sender, EventArgs e) => this.CenterOnParent();

        private void buttonSelectTarget_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog {SelectedPath = textBoxTarget.Text})
            {
                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                    textBoxTarget.Text = folderBrowserDialog.SelectedPath;
            }

            textBoxTarget.Focus();
        }

        private void textBoxTarget_TextChanged(object sender, EventArgs e)
            => buttonDeploy.Enabled = !string.IsNullOrEmpty(textBoxTarget.Text);

        private void buttonDeploy_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxTarget.Text) && Directory.GetFileSystemEntries(textBoxTarget.Text).Length != 0)
            {
                if (!Msg.YesNo(this, string.Format(Resources.PortableDirNotEmptyAsk, textBoxTarget.Text), MsgSeverity.Warn))
                    return;
            }

            Program.RunCommandAsync(Self.Name, Self.Deploy.Name, "--portable", textBoxTarget.Text);
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
            => Close();
    }
}
