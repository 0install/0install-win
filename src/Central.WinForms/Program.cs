/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Forms;
using Common.Controls;
using Common.Storage;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Launches the main WinForms GUI for Zero Install.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "ZeroInstall";

        /// <summary>
        /// The application user model ID used by the Windows 7 taskbar. Encodes <see cref="Locations.InstallBase"/> and the name of this sub-app.
        /// </summary>
        public static readonly string AppUserModelID = "ZeroInstall." + StringUtils.Hash(Locations.InstallBase, MD5.Create()) + ".Central";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            WindowsUtils.SetCurrentProcessAppID(AppUserModelID);

            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + StringUtils.Hash(Locations.InstallBase, MD5.Create());
            if (AppMutex.Probe(mutexName + "-update")) return;
            AppMutex.Create(mutexName);

            // Allow setup to detect Zero Install instances
#if !DEBUG
            AppMutex.Create("Zero Install");
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("http://0install.de/error-report/"));

            // Store installation location in registry to allow other applications or bootstrappers to locate Zero Install
            if (!Locations.IsPortable && WindowsUtils.IsWindows && WindowsUtils.IsAdministrator)
            {
                // Do not store the location if Zero Install itself was launched as a Zero Install implementation
                if (StoreUtils.PathInAStore(Locations.InstallBase))
                {
                    try
                    {
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Zero Install", "InstallLocation", Locations.InstallBase, RegistryValueKind.String);
                        if (WindowsUtils.Is64BitProcess) Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Zero Install", "InstallLocation", Locations.InstallBase, RegistryValueKind.String);
                    }
                        #region Error handling
                    catch (SecurityException)
                    {}
                    #endregion
                }
            }

            Application.Run(new MainForm());
        }

        /// <summary>
        /// Configures the Windows 7 taskbar for a specific window.
        /// </summary>
        /// <param name="form">The window to configure.</param>
        /// <param name="name">The name for the taskbar entry.</param>
        /// <param name="subCommand">The name to add to the <see cref="AppUserModelID"/> as a sub-command; may be <see langword="null"/>.</param>
        /// <param name="arguments">Additional arguments to pass to <see cref="ExeName"/> when restarting to get back to this window; may be <see langword="null"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This method operates only on windows and not on individual controls.")]
        public static void ConfigureTaskbar(Form form, string name, string subCommand, string arguments)
        {
            #region Sanity checks
            if (form == null) throw new ArgumentNullException("form");
            #endregion

            string appUserModelID = AppUserModelID;
            if (!string.IsNullOrEmpty(subCommand)) appUserModelID += "." + subCommand;
            string exePath = Path.Combine(Locations.InstallBase, ExeName + ".exe");
            WindowsUtils.SetWindowAppID(form.Handle, appUserModelID, StringUtils.EscapeArgument(exePath) + " " + arguments, exePath, name);
        }
    }
}
