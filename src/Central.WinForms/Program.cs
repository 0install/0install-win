// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Threading;
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
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Required for WinForms
        private static int Main(string[] args)
        {
            ProgramUtils.Init();
            WindowsUtils.SetCurrentProcessAppID("ZeroInstall");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));
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

        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static Task RunCommandAsync(params string[] args) => RunCommandAsync(false, args);

        /// <summary>
        /// Executes a "0install-win" command in-process in a new thread. Returns immediately.
        /// </summary>
        /// <param name="machineWide">Appends --machine to <paramref name="args"/> if <c>true</c>.</param>
        /// <param name="args">Command name with arguments to execute.</param>
        internal static Task<ExitCode> RunCommandAsync(bool machineWide, params string[] args)
        {
            if (machineWide) args = args.Append("--machine");

            return Task.Run(() => ThreadUtils.RunSta(() =>
            {
                Log.Debug($"Launching 0install-win in-process with arguments: {args.JoinEscapeArguments()}");
                using var handler = new GuiCommandHandler();
                return ProgramUtils.Run("0install-win", args, handler);
            }));
        }
    }
}
