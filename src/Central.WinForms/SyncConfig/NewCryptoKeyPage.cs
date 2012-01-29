using System;
using System.Windows.Forms;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal partial class NewCryptoKeyPage : UserControl
    {
        public event Action<string> Continue;

        public NewCryptoKeyPage()
        {
            InitializeComponent();
        }

        private void textBoxCryptoKey_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = !string.IsNullOrEmpty(textBoxCryptoKey.Text);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Continue(textBoxCryptoKey.Text);
        }
    }
}
