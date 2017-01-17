/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Net;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall
{
    /// <summary>
    /// Launches the bootstrapping GUI for Zero Install.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The full path of this binary.
        /// </summary>
        public static string ExePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;

        /// <summary>
        /// The current EXE name (without the file ending) of this binary.
        /// </summary>
        public static string ExeName = Path.GetFileNameWithoutExtension(ExePath);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("https://0install.de/error-report/"));
            NetUtils.ApplyProxy();

            if (WindowsUtils.IsWindows)
            {
                if (WindowsUtils.AttachConsole()) return (int)RunCliWindows(args);
                else return (int)RunGui(args);
            }
            else if (UnixUtils.HasGui) return (int)RunGui(args);
            else return (int)RunCli(args);
        }

        /// <summary>
        /// Runs the application in command-line mode.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <returns>The exit status code to end the process with.</returns>
        private static ExitCode RunCli(string[] args)
        {
            using (var handler = new CliTaskHandler())
                return Run(args, handler, gui: false);
        }

        /// <summary>
        /// Runs the application in command-line mode with special-case handling for retroactively attached Windows consoles.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <returns>The exit status code to end the process with.</returns>
        private static ExitCode RunCliWindows(string[] args)
        {
            using (var handler = new CliTaskHandler())
            {
                try
                {
                    Console.WriteLine();
                    return Run(args, handler, gui: false);
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
        }

        /// <summary>
        /// Runs the application in GUI mode.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <returns>The exit status code to end the process with.</returns>
        private static ExitCode RunGui(string[] args)
        {
            using (var handler = new GuiTaskHandler())
                return Run(args, handler, gui: true);
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <param name="gui"><c>true</c> if the application was launched in GUI mode; <c>false</c> if it was launched in command-line mode.</param>
        /// <returns>The exit status code to end the process with.</returns>
        private static ExitCode Run(string[] args, ITaskHandler handler, bool gui)
        {
            try
            {
                return new BootstrapProcess(handler, gui).Execute(args);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return ExitCode.UserCanceled;
            }
            catch (OptionException ex)
            {
                handler.Error(ex);
                return ExitCode.InvalidArguments;
            }
            catch (FormatException ex)
            {
                handler.Error(ex);
                return ExitCode.InvalidArguments;
            }
            catch (WebException ex)
            {
                handler.Error(ex);
                return ExitCode.WebError;
            }
            catch (NotSupportedException ex)
            {
                handler.Error(ex);
                return ExitCode.NotSupported;
            }
            catch (IOException ex)
            {
                handler.Error(ex);
                return ExitCode.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                handler.Error(ex);
                return ExitCode.AccessDenied;
            }
            catch (InvalidDataException ex)
            {
                handler.Error(ex);
                return ExitCode.InvalidData;
            }
            catch (SignatureException ex)
            {
                handler.Error(ex);
                return ExitCode.InvalidSignature;
            }
            catch (DigestMismatchException ex)
            {
                Log.Info(ex.LongMessage);
                handler.Error(ex);
                return ExitCode.DigestMismatch;
            }
            catch (SolverException ex)
            {
                handler.Error(ex);
                return ExitCode.SolverError;
            }
            catch (ExecutorException ex)
            {
                handler.Error(ex);
                return ExitCode.ExecutorError;
            }
            #endregion
        }
    }
}
