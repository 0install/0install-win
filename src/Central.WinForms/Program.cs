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
using System.IO;
using System.Windows.Forms;
using Common;

#if !DEBUG
using Common.Controls;
using Common.Storage;
using Common.Utils;
#endif

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
#if !DEBUG
            // Prevent launch during update and allow instance detection
            string mutexName = AppMutex.GenerateName(Locations.InstallationBase);
            if (AppMutex.Probe(mutexName + "-update")) return;
            AppMutex.Create(mutexName);
            AppMutex.Create("Zero Install");
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm form;
            try { form = new MainForm(); }
            catch (IOException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return;
            }

#if DEBUG
            Application.Run(form);
#else
            ErrorReportForm.RunAppMonitored(() => Application.Run(form), new Uri("http://0install.de/error-report/"));
#endif
        }
    }
}
