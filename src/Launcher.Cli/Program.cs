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
using Common;
using NDesk.Options;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Commands;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;
using CliHandler = ZeroInstall.Injector.CliHandler;

namespace ZeroInstall.Launcher.Cli
{
    #region Enumerations
    /// <summary>
    /// An errorlevel is returned to the original caller after the application terminates, to indicate success or the reason for failure.
    /// </summary>
    public enum ErrorLevel
    {
        ///<summary>Everything is OK.</summary>
        OK = 0,

        /// <summary>The user canceled the operation.</summary>
        UserCanceled = 1,

        /// <summary>The arguments passed on the command-line were not valid.</summary>
        InvalidArguments = 2,

        /// <summary>An unknown or not supported feature was requested.</summary>
        NotSupported = 3,

        /// <summary>An IO error occurred.</summary>
        IOError = 10,

        /// <summary>An network error occurred.</summary>
        WebError = 11,

        /// <summary>A requested implementation could not be found or could not be launched.</summary>
        ImplementationError = 15,

        /// <summary>A manifest digest for an implementation did not match the expected value.</summary>
        DigestMismatch = 20,

        /// <summary>A solver error occurred.</summary>
        SolverError = 21
    }
    #endregion

    /// <summary>
    /// A command-line interface for Zero Install, for launching applications.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            // Automatically show help for missing args
            if (args.Length == 0) args = new[] {"--help"};

            var command = new Run(new CliHandler());
            
            try { command.Parse(args); }
            #region Error handling
            catch (OptionException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            #endregion

            try { command.Execute(); }
            #region Error hanlding
            catch (UserCancelException)
            {
                return (int)ErrorLevel.UserCanceled;
            }
            catch (OptionException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (WebException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.WebError;
            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (ImplementationNotFoundException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.ImplementationError;
            }
            catch (CommandException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.ImplementationError;
            }
            catch (DigestMismatchException ex)
            {
                Log.Error(ex.Message);
                //if (Verbosity >= 1) Log.Info("Generated manifest:\n" + ex.ActualManifest);
                return (int)ErrorLevel.DigestMismatch;
            }
            catch (SolverException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.SolverError;
            }
            catch (FetcherException ex)
            {
                Log.Error((ex.InnerException ?? ex).Message);
                return (int)ErrorLevel.IOError;
            }
            catch (Win32Exception ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (BadImageFormatException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            #endregion

            return (int)ErrorLevel.OK;
        }
    }
}
