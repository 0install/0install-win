/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Net;
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NDesk.Options;
using ZeroInstall.Commands;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Alias.Cli
{
    /// <summary>
    /// A shortcut for '0install add-alias'.
    /// </summary>
    /// <seealso cref="AddAlias"/>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "0alias";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static int Main(string[] args)
        {
            ProgramUtils.Init();
            return (int)Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        public static ExitCode Run(string[] args)
        {
            var handler = new CliCommandHandler();
            try
            {
                var command = new AddAlias(handler);
                command.Parse(args);
                return command.Execute();
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return ExitCode.UserCanceled;
            }
            catch (NeedGuiException ex)
            {
                if (ProgramUtils.GuiAssemblyName == null)
                {
                    Log.Error(ex);
                    return ExitCode.InvalidArguments;
                }
                else
                {
                    Log.Info("Switching to GUI");
                    try
                    {
                        return (ExitCode)ProcessUtils.Assembly(ProgramUtils.GuiAssemblyName, args.Prepend(AddAlias.Name)).Run();
                    }
                        #region Error handling
                    catch (OperationCanceledException)
                    {
                        return ExitCode.UserCanceled;
                    }
                    catch (IOException ex2)
                    {
                        Log.Error(ex2);
                        return ExitCode.IOError;
                    }
                    #endregion
                }
            }
            catch (NotAdminException ex)
            {
                try
                {
                    return (ExitCode)ProcessUtils.Assembly(ProgramUtils.GuiAssemblyName ?? "0install", args.Prepend(AddAlias.Name)).AsAdmin().Run();
                }
                catch (OperationCanceledException)
                {
                    return ExitCode.UserCanceled;
                }
                catch (PlatformNotSupportedException)
                {
                    Log.Error(ex);
                    return ExitCode.AccessDenied;
                }
                catch (IOException)
                {
                    Log.Error(ex);
                    return ExitCode.AccessDenied;
                }
            }
            catch (OptionException ex)
            {
                Log.Error(ex);
                Log.Error(string.Format(Resources.TryHelp, ExeName));
                return ExitCode.InvalidArguments;
            }
            catch (FormatException ex)
            {
                Log.Error(ex);
                return ExitCode.InvalidArguments;
            }
            catch (WebException ex)
            {
                Log.Error(ex);
                return ExitCode.WebError;
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ex);
                return ExitCode.NotSupported;
            }
            catch (IOException ex)
            {
                Log.Error(ex);
                return ExitCode.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
                return ExitCode.AccessDenied;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex);
                return ExitCode.InvalidData;
            }
            catch (SignatureException ex)
            {
                Log.Error(ex);
                return ExitCode.InvalidSignature;
            }
            catch (DigestMismatchException ex)
            {
                Log.Error(ex);
                return ExitCode.DigestMismatch;
            }
            catch (SolverException ex)
            {
                Log.Error(ex);
                return ExitCode.SolverError;
            }
            catch (ConflictException ex)
            {
                Log.Error(ex);
                return ExitCode.Conflict;
            }
                #endregion

            finally
            {
                handler.CloseUI();
            }
        }
    }
}
