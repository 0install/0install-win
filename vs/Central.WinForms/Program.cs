/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Central.WinForms.Properties;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Launches the main WinForms GUI for Zero Install.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ErrorReportForm.RunAppMonitored(delegate
            {
                //Settings.LoadCurrent();
                Application.Run(new MainForm());
                //Settings.SaveCurrent();
            });
        }

        #region Helper applications
        /// <summary>
        /// Attempts to launch a .NET helper assembly in the application's base directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="owner">The parent window to which error messages are modal.</param>
        /// <param name="assembly">The name of the assembly to launch (without the file ending).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        public static void LaunchHelperAssembly(IWin32Window owner, string assembly, string arguments)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (string.IsNullOrEmpty(assembly)) throw new ArgumentNullException("assembly");
            #endregion

            try { ProcessUtils.LaunchHelperAssembly(assembly, arguments); }
            #region Sanity checks
            catch (FileNotFoundException ex)
            {
                Msg.Inform(owner, string.Format(Resources.FailedToRun + "\n" + ex.Message, assembly), MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(owner, string.Format(Resources.FailedToRun + "\n" + ex.Message, assembly), MsgSeverity.Error);
            }
            #endregion
        }
        #endregion
    }
}
