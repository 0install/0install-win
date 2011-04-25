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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Controls;
using NDesk.Options;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A WinForms-based GUI for Zero Install, for installing and launching applications, managing caches, etc.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Automatically show help for missing args
            if (args.Length == 0) args = new[] { "--help" };

#if !DEBUG
            ErrorReportForm.RunAppMonitored(delegate
#endif
            {
                var handler = new GuiHandler();
                CommandBase command;
                try
                {
                    command = CommandFactory.CreateAndParse(args, handler);

                    // Inform the handler about the name of the action being performed
                    var selection = command as Selection;
                    if (selection != null) handler.ActionTitle = selection.ActionTitle;
                }
                #region Error handling
                catch (UserCancelException)
                {
                    // This is reached if --help, --version or similar was used
                    return;
                }
                catch (OptionException ex)
                {
                    Msg.Inform(null, ex.Message + "\n" + Resources.TryHelp, MsgSeverity.Warn);
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

                try { command.Execute(); }
                #region Error handling
                catch (UserCancelException)
                {
                    handler.CloseProgressUI();
                }
                catch (OptionException ex)
                {
                    handler.CloseProgressUI();
                    Msg.Inform(null, ex.Message + "\n" + Resources.TryHelp, MsgSeverity.Error);
                }
                catch (WebException ex)
                {
                    handler.CloseProgressUI();
                    ErrorBox.Show(ex.Message, errorLog.ToString());
                }
                catch (NotSupportedException ex)
                {
                    handler.CloseProgressUI();
                    ErrorBox.Show(ex.Message, errorLog.ToString());
                }
                catch (IOException ex)
                {
                    handler.CloseProgressUI();
                    ErrorBox.Show(ex.Message, errorLog.ToString());
                }
                catch (UnauthorizedAccessException ex)
                {
                    handler.CloseProgressUI();
                    ErrorBox.Show(ex.Message, errorLog.ToString());
                }
                catch (DigestMismatchException ex)
                {
                    handler.CloseProgressUI();
                    // ToDo: Display generated manifest
                    ErrorBox.Show(ex.Message, errorLog.ToString());
                }
                catch (InvalidInterfaceIDException ex)
                {
                    handler.CloseProgressUI();
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                }
                catch (SolverException ex)
                {
                    handler.CloseProgressUI();
                    ErrorBox.Show(ex.Message, errorLog.ToString());
                }
                catch (ImplementationNotFoundException ex)
                {
                    handler.CloseProgressUI();
                    ErrorBox.Show(ex.Message, errorLog.ToString());
                }
                catch (CommandException ex)
                {
                    handler.CloseProgressUI();
                    ErrorBox.Show(ex.Message, errorLog.ToString());
                }
                catch (Win32Exception ex)
                {
                    handler.CloseProgressUI();
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                }
                catch (BadImageFormatException ex)
                {
                    handler.CloseProgressUI();
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                }
                #endregion
            }
#if !DEBUG
            , new Uri("http://0install.de/error-report/"));
#endif
        }
    }
}
