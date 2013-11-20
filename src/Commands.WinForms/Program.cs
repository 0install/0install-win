/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A WinForms-based GUI for Zero Install, for installing and launching applications, managing caches, etc.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "0install-win";

        /// <summary>
        /// The application user model ID used by the Windows 7 taskbar. Encodes <see cref="Locations.InstallBase"/> and the name of this sub-app.
        /// </summary>
        public static readonly string AppUserModelID = "ZeroInstall." + Locations.InstallBase.Hash(MD5.Create()) + ".Commands";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        // NOTE: No [STAThread] here, because it could block .NET remoting callbacks
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

            return Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        [STAThread] // Required for WinForms
        public static int Run(string[] args)
        {
            Log.Info("Zero Install Command WinForms GUI started with: " + args.JoinEscapeArguments());

            using (var handler = new DelayedGuiHandler())
            {
                FrontendCommand command;
                try
                {
                    command = CommandFactory.CreateAndParse(args, handler);
                }
                    #region Error handling
                catch (OperationCanceledException)
                {
                    // This is reached if --help, --version or similar was used
                    return 0;
                }
                catch (OptionException ex)
                {
                    var messsage = new StringBuilder(ex.Message);
                    if (ex.InnerException != null) messsage.Append("\n" + ex.InnerException.Message);
                    messsage.Append("\n" + string.Format(Resources.TryHelp, ExeName));
                    Msg.Inform(null, messsage.ToString(), MsgSeverity.Warn);
                    return 1;
                }
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    return 1;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    return 1;
                }
                catch (InvalidDataException ex)
                {
                    Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Warn);
                    return 1;
                }
                catch (InvalidInterfaceIDException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    return 1;
                }
                #endregion

                #region Collect log entries for error messages
                var errorLog = new RtfBuilder();
                Log.NewEntry += delegate(LogSeverity severity, string message)
                {
                    switch (severity)
                    {
                        case LogSeverity.Info:
                            errorLog.AppendPar(message, RtfColor.Blue);
                            break;
                        case LogSeverity.Warn:
                            errorLog.AppendPar(message, RtfColor.Orange);
                            break;
                        case LogSeverity.Error:
                            errorLog.AppendPar(message, RtfColor.Red);
                            break;
                        default:
                            errorLog.AppendPar(message, RtfColor.Black);
                            break;
                    }
                };
                #endregion

                try
                {
                    return command.Execute();
                }
                    #region Error handling
                catch (OperationCanceledException)
                {
                    return 0;
                }
                catch (NotAdminException ex)
                {
                    handler.CloseProgressUI();

                    if (WindowsUtils.IsWindows) return ProcessUtils.RunAssemblyAsAdmin("0install-win", args.JoinEscapeArguments());
                    else
                    {
                        Log.Error(ex);
                        return 1;
                    }
                }
                catch (OptionException ex)
                {
                    handler.CloseProgressUI();
                    Msg.Inform(null, ex.Message + "\n" + string.Format(Resources.TryHelp, ExeName), MsgSeverity.Error);
                    return 1;
                }
                catch (Win32Exception ex)
                {
                    handler.CloseProgressUI();
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return 1;
                }
                catch (BadImageFormatException ex)
                {
                    handler.CloseProgressUI();
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return 1;
                }
                catch (WebException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                catch (NotSupportedException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                catch (IOException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                catch (UnauthorizedAccessException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                catch (InvalidDataException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                catch (SignatureException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                catch (InvalidInterfaceIDException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                catch (DigestMismatchException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    Log.Info("Generated manifest:\n" + ex.ActualManifest);
                    ErrorBox.Show(Resources.DownloadDamaged, errorLog);
                    return 1;
                }
                catch (SolverException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message.GetLeftPartAtFirstOccurrence(Environment.NewLine), errorLog);
                    return 1;
                }
                catch (ImplementationNotFoundException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                catch (CommandException ex)
                {
                    handler.CloseProgressUI();
                    Log.Error(ex);
                    ErrorBox.Show(ex.Message, errorLog);
                    return 1;
                }
                    #endregion

                finally
                {
                    // Always close GUI in the end
                    handler.CloseProgressUI();
                }
            }
        }

        /// <summary>
        /// Configures the Windows 7 taskbar for a specific window.
        /// </summary>
        /// <param name="form">The window to configure.</param>
        /// <param name="name">The name for the taskbar entry.</param>
        /// <param name="subCommand">The name to add to the <see cref="AppUserModelID"/> as a sub-command; may be <see langword="null"/>.</param>
        /// <param name="arguments">Additional arguments to pass to <see cref="ExeName"/> when restarting to get back to this window; may be <see langword="null"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Taskbar operations are always per-window.")]
        public static void ConfigureTaskbar(Form form, string name, string subCommand = null, string arguments = null)
        {
            #region Sanity checks
            if (form == null) throw new ArgumentNullException("form");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            string appUserModelID = AppUserModelID;
            if (!string.IsNullOrEmpty(subCommand)) appUserModelID += "." + subCommand;
            string exePath = Path.Combine(Locations.InstallBase, ExeName + ".exe");
            WindowsUtils.SetWindowAppID(form.Handle, appUserModelID, exePath.EscapeArgument() + " " + arguments, exePath, name);
        }
    }
}
