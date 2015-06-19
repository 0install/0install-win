/*
 * Copyright 2010-2015 Bastian Eicher
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
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Net;
using ZeroInstall.Updater.Properties;

namespace ZeroInstall.Updater.WinForms
{
    /// <summary>
    /// Launches the update GUI for Zero Install.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "0update-win";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Required for WinForms
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("https://0install.de/error-report/"));
            NetUtils.ApplyProxy();

            if (args == null) args = new string[0];
            if (args.Length < 3 || args.Length > 4)
            {
                Msg.Inform(null, string.Format(Resources.WrongNoArguments, ExeName + " SOURCE-PATH NEW-VERSION TARGET-PATH [--rerun|--restart-central]"), MsgSeverity.Error);
                return;
            }
            bool rerun = args.Contains("--rerun");
            bool restartCentral = args.Contains("--restart-central");

            try
            {
                var updateProcess = new UpdateProcess(source: args[0], newVersion: args[1], target: args[2]);
                Application.Run(new MainForm(updateProcess, rerun, restartCentral));
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                Log.Error(ex);
                if (Environment.UserName != "SYSTEM") Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (IOException ex)
            {
                Log.Error(ex);
                if (Environment.UserName != "SYSTEM") Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
    }
}
