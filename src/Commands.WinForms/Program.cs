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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Controls;
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

            ErrorReportForm.RunAppMonitored(delegate
            {
                var handler = new MainForm();

                // ToDo: Proper parsing
                var arguments = new LinkedList<String>(args);
                if (arguments.Contains("run")) arguments.Remove("run"); else return;

                var command = new Run(handler);

                try
                {
                    command.Parse(arguments);

                    // Ask user to specifiy interface URI if it is missing
                    if (string.IsNullOrEmpty(command.Requirements.InterfaceID))
                    {
                        command.Requirements.InterfaceID = InputBox.Show("Please enter the URI of a Zero Install interface here:", "Zero Install");
                        if (string.IsNullOrEmpty(command.Requirements.InterfaceID)) return;
                    }
                }
                #region Error handling
                catch (ArgumentException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
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
                #endregion

                handler.ShowAsync();
                try { command.Execute(); }
                #region Error hanlding
                catch (UserCancelException)
                { }
                catch (ArgumentException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
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
            });
        }
    }
}
