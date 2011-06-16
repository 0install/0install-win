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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Common;
using ZeroInstall.Updater.WinForms.Properties;
#if !DEBUG
using Common.Controls;
#endif

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
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 4 )
            {
                Msg.Inform(null, Resources.WrongNoArguments, MsgSeverity.Error);
                return;
            }

            UpdateProcess updateProcess;
            try { updateProcess = new UpdateProcess(args[0], args[1], args[2]); }
            #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (NotSupportedException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            bool rerun = ((ICollection<string>)args).Contains("--rerun");

#if DEBUG
            Application.Run(new MainForm(updateProcess, rerun));
#else
            ErrorReportForm.RunAppMonitored(() => Application.Run(new MainForm(updateProcess, rerun)), new Uri("http://0install.de/error-report/"));
#endif
        }
    }
}
