// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Commands.Desktop;

namespace ZeroInstall.Central.WinForms;

public sealed partial class PortableCreatorDialog : Form
{
    public PortableCreatorDialog()
    {
        InitializeComponent();
        Font = DefaultFonts.Modern;
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
    {
        if (string.IsNullOrEmpty(textBoxTarget.Text))
        {
            buttonDeploy.Enabled = false;
            textBoxCommandLine.Text = "";
        }
        else
        {
            buttonDeploy.Enabled = true;
            textBoxCommandLine.Text = new[] {"0install", Self.Name, Self.Deploy.Name, "--portable", textBoxTarget.Text}.JoinEscapeArguments();
        }
    }

    private async void buttonDeploy_Click(object sender, EventArgs e)
    {
        if (Directory.Exists(textBoxTarget.Text) && Directory.GetFileSystemEntries(textBoxTarget.Text).Length != 0)
        {
            if (!Msg.YesNo(this, string.Format(Resources.PortableDirNotEmptyAsk, textBoxTarget.Text), MsgSeverity.Warn))
                return;
        }

        Enabled = false;
        await CommandUtils.RunAsync(Self.Name, Self.Deploy.Name, "--portable", textBoxTarget.Text, "--restart-central");
        Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
        => Close();
}
