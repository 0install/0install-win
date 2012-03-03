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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Common;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Alias.Cli.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Commands;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Alias.Cli
{
    /// <summary>
    /// A shortcut for '0install add-alias'.
    /// </summary>
    /// <seealso cref="AddAlias"/>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static int Main(string[] args)
        {
            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + StringUtils.Hash(Locations.InstallBase, MD5.Create());
            if (AppMutex.Probe(mutexName + "-update")) return 99;
            AppMutex.Create(mutexName);

#if !DEBUG
            // Allow setup to detect Zero Install instances
            AppMutex.Create("Zero Install");
#endif

            // Automatically show help for missing args
            if (args == null) args = new string[0];
            if (args.Length == 0) args = new[] {"--help"};

            IHandler handler = new CliHandler();
            CommandBase command;
            try
            {
                command = new AddAlias(Policy.CreateDefault(handler));
                command.Parse(args);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                // This is reached if --help, --version or similar was used
                return 0;
            }
            catch (OptionException ex)
            {
                Log.Error(ex.Message + "\n" + string.Format(Resources.TryHelp, "0launch"));
                return 1;
            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message));
                return 1;
            }
            catch (InvalidInterfaceIDException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            #endregion

            try
            {
                return command.Execute();
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return 1;
            }
            catch (OptionException ex)
            {
                Log.Error(ex.Message + "\n" + string.Format(Resources.TryHelp, "0alias"));
                return 1;
            }
            catch (WebException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message));
                return 1;
            }
            catch (SignatureException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (DigestMismatchException ex)
            {
                Log.Error(ex.Message);
                //if (command.Verbosity >= 1) Log.Info("Generated manifest:\n" + ex.ActualManifest);
                return 1;
            }
            catch (InvalidInterfaceIDException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (SolverException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (ImplementationNotFoundException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (CommandException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (Win32Exception ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (BadImageFormatException ex)
            {
                Log.Error(ex.Message);
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
}
