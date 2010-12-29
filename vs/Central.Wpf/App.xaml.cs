/*
 * Copyright 2010 Dennis Keil
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using Common;
using Common.Storage;
using Common.Wpf;

namespace ZeroInstall.Central.Wpf
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App
    {
        #region Unhandled Exceptions
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            (new UnhandledExceptionWindow(e.Exception)).ShowDialog();
            
            // Return exit code
            Shutdown(-1);

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
                Msg.Inform(new Wpf32Window(owner), string.Format(Wpf.Properties.Resources.FailedToRun, appName), MsgSeverity.Error);
            }
            catch (FileNotFoundException)
            {
                Msg.Inform(new Wpf32Window(owner), string.Format(Wpf.Properties.Resources.FailedToRun, appName), MsgSeverity.Error);
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
