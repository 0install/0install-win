using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ZeroInstall.Launchpad.Properties;

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
            try { Process.Start(Program.AppDir + "\\0launch.exe", textBoxURL.Text); }
            catch (Win32Exception)
            {
                MessageBox.Show(string.Format(Resources.FailedToRun, "0launch.exe"), @"Zero Install", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(string.Format(Resources.FailedToRun, "0launch.exe"), @"Zero Install", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
