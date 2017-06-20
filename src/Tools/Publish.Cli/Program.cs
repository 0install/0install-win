/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Linq;
using System.Net;
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish.Cli
{
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
            ProcessUtils.SanitizeEnvironmentVariables();
            NetUtils.ApplyProxy();
            NetUtils.ConfigureTls();

            // Automatically show help for missing args
            if (args == null) args = new string[0];
            if (args.Length == 0) args = new[] {"--help"};

            try
            {
                using (var handler = new CliTaskHandler())
                {
                    var command = (args.FirstOrDefault() == "capture")
                        ? (ICommand)new CaptureCommand(args.Skip(1), handler)
                        : new PublishCommand(args, handler);
                    return (int)command.Execute();
                }

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
            catch (WebException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.WebError;
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
            catch (DigestMismatchException ex)
            {
                Log.Info(ex.LongMessage);
                Log.Error(ex);
                return (int)ExitCode.DigestMismatch;
            }
            catch (KeyNotFoundException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.InvalidArguments;
            }
            catch (WrongPassphraseException ex)
            {
                Log.Error(ex);
                return (int)ExitCode.InvalidArguments;
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
