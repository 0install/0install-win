/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Common;
using Common.Controls;
using ZeroInstall.Updater.WinForms.Properties;

namespace ZeroInstall.Updater.WinForms
{
    /// <summary>
    /// Launches the update GUI for Zero Install.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Required for WinForms
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("http://0install.de/error-report/"));

            if (args == null) args = new string[0];
            if (args.Length < 3 || args.Length > 4)
            {
                Msg.Inform(null, string.Format(Resources.WrongNoArguments, "0update-win SOURCE-PATH NEW-VERSION TARGET-PATH"), MsgSeverity.Error);
                return;
            }

            try
            {
                var updateProcess = new UpdateProcess(args[0], args[1], args[2]);
                Application.Run(new MainForm(updateProcess, args.Contains("--rerun")));
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (NotSupportedException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
    }
}
