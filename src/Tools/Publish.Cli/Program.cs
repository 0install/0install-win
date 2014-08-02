/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.IO;
using System.Net;
using NanoByte.Common;
using NanoByte.Common.Utils;
using NDesk.Options;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;
using SharedResources = ZeroInstall.Publish.Properties.Resources;

namespace ZeroInstall.Publish.Cli
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
        IOError = 10
    }
    #endregion

    /// <summary>
    /// Launches a command-line tool for editing Zero Install feed XMLs.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static int Main(string[] args)
        {
            NetUtils.ApplyProxy();

            // Automatically show help for missing args
            if (args == null) args = new string[0];
            if (args.Length == 0) args = new[] {"--help"};

            PublishRun run;
            try
            {
                run = new PublishRun(args);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                // This is reached if --help, --version or similar was used
                return 0;
            }
            catch (OptionException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            try
            {
                return (int)run.Execute();
            }
                #region Error hanlding
            catch (OperationCanceledException)
            {
                return (int)ErrorLevel.UserCanceled;
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (OptionException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (WebException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (IOException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (DigestMismatchException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (KeyNotFoundException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (WrongPassphraseException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
                #endregion

            finally
            {
                run.Dispose();
            }
        }
    }
}
