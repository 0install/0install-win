using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Common;
using ZeroInstall.Launchpad.Properties;

namespace ZeroInstall.Launchpad
{
    static class Program
    {
        #region Properties
        /// <summary>
        /// The directory where the executable file is located.
        /// </summary>
        public static string AppDir
        {
            get { return Path.GetDirectoryName(Application.ExecutablePath); }
        }
        #endregion

        //--------------------//

        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());
        }
        #endregion

        #region Helper applications
        /// <summary>
        /// Attempts to launch a helper application in the installation directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="owner">The parent window error messages are modal to.</param>
        /// <param name="appName">The name of the EXE file to launch</param>
        public static void LaunchHelperApp(IWin32Window owner, string appName)
        {
            try { Process.Start(AppDir + "\\" + appName); }
            catch (Win32Exception)
            {
                Msg.Inform(owner, string.Format(Resources.FailedToRun, appName), MsgSeverity.Error);
            }
            catch (FileNotFoundException)
            {
                Msg.Inform(owner, string.Format(Resources.FailedToRun, appName), MsgSeverity.Error);
            }
        }
        #endregion
    }
}
