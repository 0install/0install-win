using System;
using System.Windows.Forms;

namespace ZeroInstall.Launchpad
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonAddFeed_Click(object sender, EventArgs e)
        {
            using (var feedUrlForm = new FeedUrlForm())
                feedUrlForm.ShowDialog(this);
        }

        private void buttonManageCache_Click(object sender, EventArgs e)
        {
            Program.LaunchHelperApp(this, "0storew.exe");
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {

        }
    }
}
