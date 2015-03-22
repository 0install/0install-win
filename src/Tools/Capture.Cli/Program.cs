/*
 * Copyright 2011 Bastian Eicher
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
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Tasks;
using NDesk.Options;

namespace ZeroInstall.Capture.Cli
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

        /// <summary>A warning regarding potentially unintended usage caused the operation to be canceled.</summary>
        Warning = 9,

        /// <summary>An IO error occurred.</summary>
        IOError = 10
    }
    #endregion

    /// <summary>
    /// Launches a command-line tool for capturing application installations to feeds.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static int Main(string[] args)
        {
            NetUtils.ApplyProxy();

            try
            {
                var command = new CaptureCommand(args, new CliTaskHandler());
                return (int)command.Execute();
            }
                #region Error hanlding
            catch (OperationCanceledException)
            {
                return (int)ErrorLevel.UserCanceled;
            }
            catch (OptionException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (InvalidOperationException ex)
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
            #endregion
        }
    }
}
