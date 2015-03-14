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
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Commands;

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
        public static readonly string AppUserModelID = "ZeroInstall." + Locations.InstallBase.GetHashCode() + ".Central";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Required for WinForms
        private static int Main(string[] args)
        {
            WindowsUtils.SetCurrentProcessAppID(AppUserModelID);

            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.GetHashCode();
            if (AppMutex.Probe(mutexName + "-update")) return 99;
            AppMutex.Create(mutexName);

            // Allow setup to detect Zero Install instances
#if !DEBUG
            AppMutex.Create("Zero Install");
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("https://0install.de/error-report/"));

            ProgramUtils.Startup();
            return Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        [STAThread] // Required for WinForms
        public static int Run(string[] args)
        {
            bool machineWide = args.Any(arg => arg == "-m" || arg == "--machine");
            if (machineWide && WindowsUtils.IsWindowsNT && !WindowsUtils.IsAdministrator) return ProcessUtils.RunAssemblyAsAdmin("ZeroInstall", args.JoinEscapeArguments());

            try
            {
                Application.Run(new MainForm(machineWide));
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return -1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return -1;
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return -1;
            }
            #endregion

            return 0;
        }

        #region Embed
        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static void RunCommand([NotNull] params string[] args)
        {
            RunCommand(false, args);
        }

        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="machineWide">Appends --machine to <paramref name="args"/> if <see langword="true"/>.</param>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static void RunCommand(bool machineWide, [NotNull] params string[] args)
        {
            RunCommand(null, machineWide, args);
        }

        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="callback">A callback method to be raised once the command has finished executing. Uses <see cref="SynchronizationContext"/> of calling thread. Can be <see langword="null"/>.</param>
        /// <param name="machineWide">Appends --machine to <paramref name="args"/> if <see langword="true"/>.</param>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static void RunCommand([CanBeNull] Action callback, bool machineWide, [NotNull] params string[] args)
        {
            var context = SynchronizationContext.Current;
            ProcessUtils.RunAsync(
                () =>
                {
                    Commands.WinForms.Program.Run(machineWide ? args.Append("--machine").ToArray() : args);
                    if (callback != null) context.Send(state => callback(), null);
                },
                "0install-win (" + args.JoinEscapeArguments() + ")");
        }
        #endregion

        #region Browser
        /// <summary>
        /// Opens a URL in the system's default browser.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to; can be <see langword="null"/>.</param>
        /// <param name="url">The URL to open.</param>
        internal static void OpenInBrowser([CanBeNull] IWin32Window owner, [NotNull] string url)
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
        /// <param name="subCommand">The name to add to the <see cref="AppUserModelID"/> as a sub-command; can be <see langword="null"/>.</param>
        /// <param name="arguments">Additional arguments to pass to <see cref="ExeName"/> when restarting to get back to this window; can be <see langword="null"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This method operates only on windows and not on individual controls.")]
        internal static void ConfigureTaskbar([NotNull] Form form, [NotNull] string name, [CanBeNull] string subCommand = null, [CanBeNull] string arguments = null)
        {
            #region Sanity checks
            if (form == null) throw new ArgumentNullException("form");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            string appUserModelID = AppUserModelID;
            if (!string.IsNullOrEmpty(subCommand)) appUserModelID += "." + subCommand;
            string exePath = Path.Combine(Locations.InstallBase, ExeName + ".exe");
            WindowsTaskbar.SetWindowAppID(form.Handle, appUserModelID, exePath.EscapeArgument() + " " + arguments, exePath, name);
        }
        #endregion
    }
}
