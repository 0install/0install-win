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
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
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
            if (args.Length == 0) args = new[] {"--help"};

            ErrorReportForm.RunAppMonitored(delegate
            {
                var handler = new MainForm();
                CommandBase command;
                try
                {
                    command = CommandFactory.CreateAndParse(args, handler);

                    var selection = command as Selection;
                    if (selection != null)
                    {
                        // Ask user to specifiy interface ID if it is missing
                        if (string.IsNullOrEmpty(selection.Requirements.InterfaceID))
                        {
                            selection.Requirements.InterfaceID = InputBox.Show("Please enter the URI of a Zero Install interface here:", "Zero Install");
                            if (string.IsNullOrEmpty(selection.Requirements.InterfaceID)) return;
                        }

                        // Start warming up the GUI - selection commands are often long-running
                        handler.ShowAsync();
                    }
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
                catch (InvalidOperationException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
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

                try { command.Execute(); }
                #region Error hanlding
                catch (UserCancelException)
                { }
                catch (OptionException ex)
                {
                    Msg.Inform(null, ex.Message + "\n" + Resources.TryHelp, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (WebException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (InvalidInterfaceIDException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    return;
                }
                catch (SolverException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (FetcherException ex)
                {
                    Msg.Inform(null, (ex.InnerException ?? ex).Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (DigestMismatchException ex)
                {
                    // ToDo: Display generated manifest
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (ImplementationNotFoundException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (CommandException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (Win32Exception ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (BadImageFormatException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                #endregion
                finally
                {
                    // Close any windows that may still be open
                    handler.CloseAsync();
                }
            });
        }
    }
}
