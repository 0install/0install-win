/*
 * Copyright 2010 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Common;
using ZeroInstall.Central.Properties;
using ZeroInstall.Central.Storage;

namespace ZeroInstall.Central
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Settings.LoadCurrent();

            Application.Run(new MainForm());

            Settings.SaveCurrent();
        }

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

            try { Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appName), arguments); }
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
