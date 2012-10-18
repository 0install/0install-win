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
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

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
        [STAThread]
        public static void Main(string[] args)
        {
            WindowsUtils.SetCurrentProcessAppID(AppUserModelID);

            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.Hash(MD5.Create());
            if (AppMutex.Probe(mutexName + "-update")) return;
            AppMutex.Create(mutexName);

            // Allow setup to detect Zero Install instances
#if !DEBUG
            AppMutex.Create("Zero Install");
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("http://0install.de/error-report/"));

            Log.Info("Zero Install Command Windows GUI started with: " + args.JoinEscapeArguments());

            // Automatically show help for missing args
            if (args == null) args = new string[0];
            if (args.Length == 0) args = new[] {"--help"};

            IHandler handler = new DelayedGuiHandler();
            FrontendCommand command;
            try
            {
                command = CommandFactory.CreateAndParse(args, handler);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                // This is reached if --help, --version or similar was used
                return;
            }
            catch (OptionException ex)
            {
                var messsage = new StringBuilder(ex.Message);
                if (ex.InnerException != null) messsage.Append("\n" + ex.InnerException.Message);
                messsage.Append("\n" + string.Format(Resources.TryHelp, ExeName));
                Msg.Inform(null, messsage.ToString(), MsgSeverity.Warn);
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Warn);
                return;
            }
            catch (InvalidInterfaceIDException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                return;
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
                command.Execute();
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (OptionException ex)
            {
                handler.DisableProgressUI();
                Msg.Inform(null, ex.Message + "\n" + string.Format(Resources.TryHelp, ExeName), MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                handler.DisableProgressUI();
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (BadImageFormatException ex)
            {
                handler.DisableProgressUI();
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (WebException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
            catch (NotSupportedException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
            catch (IOException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
            catch (UnauthorizedAccessException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
            catch (InvalidDataException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
            catch (SignatureException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
            catch (InvalidInterfaceIDException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
            catch (DigestMismatchException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                Log.Info("Generated manifest:\n" + ex.ActualManifest);
                ErrorBox.Show(Resources.DownloadDamaged, errorLog);
            }
            catch (SolverException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message.GetLeftPartAtFirstOccurrence(Environment.NewLine), errorLog);
            }
            catch (ImplementationNotFoundException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
            catch (CommandException ex)
            {
                handler.DisableProgressUI();
                Log.Error(ex);
                ErrorBox.Show(ex.Message, errorLog);
            }
                #endregion

            finally
            {
                // Always close GUI in the end
                handler.CloseProgressUI();
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
        public static void ConfigureTaskbar(Form form, string name, string subCommand, string arguments)
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
