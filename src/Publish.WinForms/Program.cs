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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Cli;

#if !DEBUG
using Common.Controls;
#endif

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// Launches a WinForms-based editor for Zero Install feed XMLs.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
            Run(args);
#else
            ErrorReportForm.RunAppMonitored(() => Run(args), new Uri("http://0install.de/error-report/"));
#endif
        }

        private static void Run(string[] args)
        {
            if (args.Length == 0) Application.Run(new MainForm());
            else
            {
                ICollection<FileInfo> files;
                try
                {
                    files = ArgumentUtils.GetFiles(args, "*.xml");
                }
                    #region Error handling
                catch (FileNotFoundException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                #endregion

                //if (files.Count == 1)
                //{
                //    // ToDo: Open file for editing
                //}
                //else
                {
                    MassSignDialog.Show(files);
                }
            }
        }
    }
}
