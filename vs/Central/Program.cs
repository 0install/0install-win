using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Common;
using ZeroInstall.Central.Properties;
using ZeroInstall.Central.Storage;

namespace ZeroInstall.Central
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

            Settings.LoadCurrent();

            Application.Run(new MainForm());

            Settings.SaveCurrent();
        }
        #endregion

        #region Helper applications
        /// <summary>
        /// Attempts to launch a helper application in the installation directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="owner">The parent window error messages are modal to.</param>
        /// <param name="appName">The name of the EXE file to launch.</param>
        /// <param name="arguments">The command-line arguments to pass to the application.</param>
        public static void LaunchHelperApp(IWin32Window owner, string appName, string arguments)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            #endregion

            try { Process.Start(AppDir + "\\" + appName, arguments); }
            catch (Win32Exception)
            {
                Msg.Inform(owner, string.Format(Resources.FailedToRun, appName), MsgSeverity.Error);
            }
            catch (FileNotFoundException)
            {
                Msg.Inform(owner, string.Format(Resources.FailedToRun, appName), MsgSeverity.Error);
            }
        }

        /// <summary>
        /// Attempts to launch a helper application in the installation directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="owner">The parent window error messages are modal to.</param>
        /// <param name="appName">The name of the EXE file to launch.</param>
        public static void LaunchHelperApp(IWin32Window owner, string appName)
        {
            LaunchHelperApp(owner, appName, null);
        }
        #endregion
    }
}
