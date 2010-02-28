using System;
using System.Windows.Forms;

namespace ZeroInstall.Launchpad
{
    public partial class FeedUrlForm : Form
    {
        public FeedUrlForm()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Program.LaunchHelperApp(this, "0launch.exe", textBoxURL.Text);

            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
