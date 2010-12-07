using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Windows.Interop;
using Common;
using Common.Storage;
using Common.Wpf;

namespace ZeroInstall.Central.Wpf
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        #region Unhandled Exceptions
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            (new UnhandledExceptionWindow(e.Exception)).ShowDialog();
            
            // Return exit code
            this.Shutdown(-1);

            e.Handled = true;
        }
        #endregion

        #region Helper applications
        /// <summary>
        /// Attempts to launch a helper application in the installation directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="owner">The parent window error messages are modal to.</param>
        /// <param name="appName">The name of the EXE file to launch.</param>
        /// <param name="arguments">The command-line arguments to pass to the application.</param>
        public static void LaunchHelperApp(Window owner, string appName, string arguments)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            #endregion

            try { Process.Start(Path.Combine(Locations.PortableBase, appName), arguments); }
            catch (Win32Exception)
            {
                Msg.Inform(new Wpf32Window(owner), string.Format(ZeroInstall.Central.Wpf.Properties.Resources.FailedToRun, appName), MsgSeverity.Error);
            }
            catch (FileNotFoundException)
            {
                Msg.Inform(new Wpf32Window(owner), string.Format(ZeroInstall.Central.Wpf.Properties.Resources.FailedToRun, appName), MsgSeverity.Error);
            }
        }

        /// <summary>
        /// Attempts to launch a helper application in the installation directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="owner">The parent window error messages are modal to.</param>
        /// <param name="appName">The name of the EXE file to launch.</param>
        public static void LaunchHelperApp(Window owner, string appName)
        {
            LaunchHelperApp(owner, appName, null);
        }
        #endregion

        public static System.Windows.Forms.IWin32Window NativeWnd;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //NativeWnd = App.Current.MainWindow.GetNativeWnd();
        }
    }
}
