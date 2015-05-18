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
using ZeroInstall.Publish;

namespace ZeroInstall.Capture.Cli
{
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
                return (int)ExitCode.UserCanceled;
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.InvalidArguments;
            }
            catch (OptionException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.InvalidArguments;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.InvalidArguments;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.InvalidData;
            }
            catch (IOException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.AccessDenied;
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.NotSupported;
            }
            #endregion
        }
    }
}
