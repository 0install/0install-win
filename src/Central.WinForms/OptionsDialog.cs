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
using Common;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Central.WinForms.Properties;

namespace ZeroInstall.Central.WinForms
{
    public partial class OptionsDialog : OKCancelDialog
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Attempts to launch a .NET helper assembly in the application's base directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        private void LaunchHelperAssembly(string assembly, string arguments)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(assembly)) throw new ArgumentNullException("assembly");
            #endregion

            try
            {
                ProcessUtils.LaunchHelperAssembly(assembly, arguments);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, assembly), MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, assembly), MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(Program.CommandsExe, "config");
            Close();
        }
    }
}
