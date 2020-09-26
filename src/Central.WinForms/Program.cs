// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using ZeroInstall.Commands;
using ZeroInstall.Commands.WinForms;

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
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Required for WinForms
        private static int Main(string[] args)
        {
            ProgramUtils.Init();
            WindowsUtils.SetCurrentProcessAppID("ZeroInstall");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("https://0install.de/error-report/"));
            return Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        [STAThread] // Required for WinForms
        public static int Run(string[] args)
        {
            try
            {
                Application.Run(new MainForm(machineWide: args.Contains("-m") || args.Contains("--machine")));
            }
            #region Error handling
            catch (IOException ex)
            {
                Log.Error(ex);
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return -1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return -1;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex);
                Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : Environment.NewLine + ex.InnerException.Message), MsgSeverity.Error);
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
        internal static void RunCommand(params string[] args) => RunCommand(false, args);

        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="machineWide">Appends --machine to <paramref name="args"/> if <c>true</c>.</param>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static void RunCommand(bool machineWide, params string[] args) => RunCommand(null, machineWide, args);

        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="callback">A callback method to be raised once the command has finished executing. Uses <see cref="SynchronizationContext"/> of calling thread. Can be <c>null</c>.</param>
        /// <param name="machineWide">Appends --machine to <paramref name="args"/> if <c>true</c>.</param>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static void RunCommand(Action? callback, bool machineWide, params string[] args)
        {
            if (machineWide) args = args.Append("--machine");

            var context = SynchronizationContext.Current;
            ThreadUtils.StartAsync(
                () =>
                {
                    Log.Debug($"Launching {Commands.WinForms.Program.ExeName} in-process with arguments: {args.JoinEscapeArguments()}");
                    using (var handler = new GuiCommandHandler())
                        ProgramUtils.Run(Commands.WinForms.Program.ExeName, args, handler);

                    if (callback != null)
                    {
                        try
                        {
                            context.Send(state => callback(), null);
                        }
                        catch (InvalidAsynchronousStateException)
                        {
                            // Ignore callback if UI was closed in the meantime
                        }
                    }
                },
                "0install-win (" + args.JoinEscapeArguments() + ")");
        }
        #endregion
    }
}
