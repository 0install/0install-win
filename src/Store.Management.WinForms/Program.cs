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
using Common.Storage;
using Common.Utils;
#if !DEBUG
using Common.Controls;
#endif

namespace ZeroInstall.Store.Management.WinForms
{
    /// <summary>
    /// Launches a WinForms tool for managing caches of Zero Install implementations.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "0store-win";

        /// <summary>
        /// The application user model ID used by the Windows 7 taskbar. Encodes <see cref="Locations.InstallBase"/> and the name of this sub-app.
        /// </summary>
        public static readonly string AppUserModelID = "ZeroInstall." + AppMutex.GenerateName(Locations.InstallBase) + ".Store.Management";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            WindowsUtils.SetCurrentProcessAppID(AppUserModelID);

#if !DEBUG
            // Prevent launch during update and allow instance detection
            string mutexName = AppMutex.GenerateName(Locations.InstallBase);
            if (AppMutex.Probe(mutexName + "-update")) return;
            AppMutex.Create(mutexName);
            AppMutex.Create("Zero Install");
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
            Application.Run(new MainForm());
#else
            ErrorReportForm.RunAppMonitored(() => Application.Run(new MainForm()), new Uri("http://0install.de/error-report/"));
#endif
        }

        /// <summary>
        /// Configures the Windows 7 taskbar for a specific window.
        /// </summary>
        /// <param name="form">The window to configure.</param>
        /// <param name="name">The name for the taskbar entry.</param>
        /// <param name="subCommand">The name to add to the <see cref="AppUserModelID"/> as a sub-command; may be <see langword="null"/>.</param>
        /// <param name="arguments">Additional arguments to pass to <see cref="ExeName"/> when restarting to get back to this window; may be <see langword="null"/>.</param>
        public static void ConfigureTaskbar(Form form, string name, string subCommand, string arguments)
        {
            string appUserModelID = AppUserModelID;
            if (!string.IsNullOrEmpty(subCommand)) appUserModelID += "." + subCommand;
            string exePath = Path.Combine(Locations.InstallBase, ExeName + ".exe");
            WindowsUtils.SetWindowAppID(form.Handle, appUserModelID, StringUtils.EscapeWhitespace(exePath) + " " + arguments, exePath, name);
        }
    }
}
