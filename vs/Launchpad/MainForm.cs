using System;
using System.Diagnostics;
using System.Windows.Forms;
using ZeroInstall.Backend.Model;

namespace ZeroInstall.Launchpad
{
    public partial class MainForm : Form
    {
        #region Constructor
        public MainForm()
        {
            InitializeComponent();

            browserNewApps.CanGoBackChanged += delegate { toolStripButtonBack.Enabled = browserNewApps.CanGoBack; };
        }
        #endregion

        #region Feed management
        private void buttonAddFeed_Click(object sender, EventArgs e)
        {
            InterfaceInformationDialog.ShowDialog(this, new Interface());
            
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
        #endregion

        #region New apps browser
        private void toolStripButtonBack_Click(object sender, EventArgs e)
        {
            browserNewApps.GoBack();
        }

        private const string UrlPostfixFeed = "#0install-feed";
        private const string UrlPostfixBrowser = "#external-browser";

        private void browserNewApps_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            switch (e.Url.Fragment)
            {
                case UrlPostfixFeed:
                    MessageBox.Show(e.Url.OriginalString.Replace(UrlPostfixFeed, ""));
                    e.Cancel = true;
                    break;
                case UrlPostfixBrowser:
                    Process.Start(e.Url.OriginalString.Replace(UrlPostfixBrowser, ""));
                    e.Cancel = true;
                    break;
            }
        }

        private void browserNewApps_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent any popups
            e.Cancel = true;
        }
        #endregion

        private void browserNewApps_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
