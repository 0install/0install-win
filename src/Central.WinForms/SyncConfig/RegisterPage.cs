using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Common;
using ZeroInstall.Injector;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal partial class RegisterPage : UserControl
    {
        public event SimpleEventHandler Continue;

        public RegisterPage()
        {
            InitializeComponent();
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenInBrowser(Config.DefaultSyncServer + "register");
        }

        private void buttonContinue_Click(object sender, System.EventArgs e)
        {
            Continue();
        }

        #region Helpers
        /// <summary>
        /// Opens a URL in the system's default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenInBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion
    }
}
