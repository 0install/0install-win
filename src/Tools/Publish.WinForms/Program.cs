/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common.Cli;
using NanoByte.Common.Controls;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Trust;

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
        [STAThread] // Required for WinForms
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("http://0install.de/error-report/"));
            NetUtils.ApplyProxy();

            var openPgp = OpenPgpFactory.CreateDefault();

            if (args == null || args.Length == 0) Application.Run(new WelcomeForm(openPgp));
            else
            {
                try
                {
                    var files = ArgumentUtils.GetFiles(args, "*.xml");
                    if (files.Count == 1)
                    {
                        string path = files.First().FullName;
                        Application.Run(new MainForm(FeedEditing.Load(path), openPgp));
                    }
                    else MassSignForm.Show(files);
                }
                    #region Error handling
                catch (ArgumentException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                }
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                }
                catch (InvalidDataException ex)
                {
                    Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Warn);
                }
                #endregion
            }
        }
    }
}
