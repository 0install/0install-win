// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;

namespace ZeroInstall
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            ProgramUtils.Init();

            if (WindowsUtils.IsWindows)
            {
                if (WindowsUtils.AttachConsole())
                    return (int)RunCliRedirect(args);
                else
                    return (int)RunGui(args);
            }
            else return (int)RunCli(args);
        }

        /// <summary>
        /// Runs the application in command-line mode.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <returns>The exit status code to end the process with.</returns>
        public static ExitCode RunCli(string[] args)
        {
            using var handler = new CliTaskHandler();
            return ProgramUtils.Run(args, handler, gui: false);
        }

        /// <summary>
        /// Runs the application in command-line mode with special-case handling for retroactively attached Windows consoles.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <returns>The exit status code to end the process with.</returns>
        public static ExitCode RunCliRedirect(string[] args)
        {
            using var handler = new CliTaskHandler();
            try
            {
                Console.WriteLine();
                return ProgramUtils.Run(args, handler, gui: false);
            }
            finally
            {
                if (handler.Verbosity != Verbosity.Batch)
                {
                    Console.WriteLine();
                    Console.Write(Environment.CurrentDirectory + @">");
                }
            }
        }

        /// <summary>
        /// Runs the application in GUI mode.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <returns>The exit status code to end the process with.</returns>
        public static ExitCode RunGui(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));

            using var handler = new GuiTaskHandler();
            return ProgramUtils.Run(args, handler, gui: true);
        }
    }
}
