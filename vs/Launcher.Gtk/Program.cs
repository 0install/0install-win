/*
 * Copyright 2010 Bastian Eicher
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
using System.Text;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Utils;
using Gtk;
using NDesk.Options;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Commands;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Launcher.Gtk
{
    /// <summary>
    /// Launches Zero Install implementations and displays a GTK# GUI.
    /// </summary>
    public static class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Automatically show help for missing args
            if (args.Length == 0) args = new[] {"--help"};

            var handler = new MainWindow();
            var command = new Run(handler);

            try
            {
                command.Parse(args);

                // Ask user to specifiy interface URI if it is missing
                if (string.IsNullOrEmpty(command.Requirements.InterfaceID))
                {
                    // ToDo
                    //command.Requirements.InterfaceID = InputBox.Show("Please enter the URI of a Zero Install interface here:", "Zero Install");
                    //if (string.IsNullOrEmpty(command.Requirements.InterfaceID)) return;
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
            {}
            catch (ArgumentException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (WebException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (IOException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (SolverException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (FetcherException ex)
            {
                Msg.Inform(null, (ex.InnerException ?? ex).Message, MsgSeverity.Error);
            }
            catch (DigestMismatchException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (ImplementationNotFoundException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (CommandException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (BadImageFormatException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion
    }
}
