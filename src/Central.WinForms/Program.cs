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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Storage;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// The main GUI for Zero Install, for discovering and installing new applications, managing and launching installed applications, etc.
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
        public static readonly string AppUserModelID = "ZeroInstall." + Locations.InstallBase.Hash(MD5.Create()) + ".Central";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Required for WinForms
        private static int Main(string[] args)
        {
            WindowsUtils.SetCurrentProcessAppID(AppUserModelID);

            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.Hash(MD5.Create());
            if (AppMutex.Probe(mutexName + "-update")) return 99;
            AppMutex.Create(mutexName);

            // Allow setup to detect Zero Install instances
#if !DEBUG
            AppMutex.Create("Zero Install");
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("http://0install.de/error-report/"));

            UpdateRegistry();
            NetUtils.TrustCertificates(SyncIntegrationManager.DefaultServerPublicKey);
            return Run(args);
        }

        /// <summary>
        /// Store installation location in registry to allow other applications or bootstrappers to locate Zero Install.
        /// </summary>
        private static void UpdateRegistry()
        {
            if (Locations.IsPortable || !WindowsUtils.IsWindows || !StoreUtils.PathInAStore(Locations.InstallBase)) return;

            try
            {
                if (WindowsUtils.IsAdministrator)
                {
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Zero Install", "InstallLocation", Locations.InstallBase, RegistryValueKind.String);
                    if (WindowsUtils.Is64BitProcess) Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Zero Install", "InstallLocation", Locations.InstallBase, RegistryValueKind.String);
                }
                else Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Zero Install", "InstallLocation", Locations.InstallBase, RegistryValueKind.String);
            }
                #region Error handling
            catch (SecurityException)
            {}
            #endregion
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        [STAThread] // Required for WinForms
        public static int Run(string[] args)
        {
            bool machineWide = args.Any(arg => arg == "-m" || arg == "--machine");
            if (machineWide && WindowsUtils.IsWindowsNT && !WindowsUtils.IsAdministrator) return ProcessUtils.RunAssemblyAsAdmin("ZeroInstall", args.JoinEscapeArguments());

            Application.Run(new MainForm(machineWide));
            return 0;
        }

        #region Embed
        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static void RunCommand(params string[] args)
        {
            RunCommand(false, args);
        }

        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="machineWide">Appends --machine to <paramref name="args"/> if <see langword="true"/>.</param>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static void RunCommand(bool machineWide, params string[] args)
        {
            RunCommand(null, machineWide, args);
        }

        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="callback">A callback method to be raised once the command has finished executing. Uses <see cref="SynchronizationContext"/> of calling thread.</param>
        /// <param name="machineWide">Appends --machine to <paramref name="args"/> if <see langword="true"/>.</param>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static void RunCommand(Action callback, bool machineWide, params string[] args)
        {
            var context = SynchronizationContext.Current;
            ProcessUtils.RunAsync(
                () =>
                {
                    Commands.WinForms.Program.Run(machineWide ? args.Concat("--machine").ToArray() : args);
                    if (callback != null) context.Send(state => callback(), null);
                },
                "0install-win (" + args.JoinEscapeArguments() + ")");
        }
        #endregion

        #region Browser
        /// <summary>
        /// Opens a URL in the system's default browser.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to; may be <see langword="null"/>.</param>
        /// <param name="url">The URL to open.</param>
        internal static void OpenInBrowser(IWin32Window owner, string url)
        {
            try
            {
                Process.Start(url);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(owner, ex.Message, MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(owner, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion

        #region Taskbar
        /// <summary>
        /// Configures the Windows 7 taskbar for a specific window.
        /// </summary>
        /// <param name="form">The window to configure.</param>
        /// <param name="name">The name for the taskbar entry.</param>
        /// <param name="subCommand">The name to add to the <see cref="AppUserModelID"/> as a sub-command; may be <see langword="null"/>.</param>
        /// <param name="arguments">Additional arguments to pass to <see cref="ExeName"/> when restarting to get back to this window; may be <see langword="null"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This method operates only on windows and not on individual controls.")]
        internal static void ConfigureTaskbar(Form form, string name, string subCommand = null, string arguments = null)
        {
            #region Sanity checks
            if (form == null) throw new ArgumentNullException("form");
            #endregion

            string appUserModelID = AppUserModelID;
            if (!string.IsNullOrEmpty(subCommand)) appUserModelID += "." + subCommand;
            string exePath = Path.Combine(Locations.InstallBase, ExeName + ".exe");
            WindowsUtils.SetWindowAppID(form.Handle, appUserModelID, exePath.EscapeArgument() + " " + arguments, exePath, name);
        }
        #endregion
    }
}
