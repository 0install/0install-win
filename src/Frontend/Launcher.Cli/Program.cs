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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NDesk.Options;
using ZeroInstall.Commands;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Launcher.Cli
{
    /// <summary>
    /// A shorcut for '0install run'.
    /// </summary>
    /// <seealso cref="Run"/>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "0launch";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static int Main(string[] args)
        {
            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.GetHashCode();
            if (AppMutex.Probe(mutexName + "-update")) return 99;
            AppMutex.Create(mutexName);

            // Allow setup to detect Zero Install instances
#if !DEBUG
            AppMutex.Create("Zero Install");
#endif

            ProgramUtils.Startup();
            return Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        public static int Run(string[] args)
        {
            var handler = new CliCommandHandler();
            try
            {
                var command = new Run(handler);
                command.Parse(args);
                return command.Execute();
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return 1;
            }
            catch (NotAdminException ex)
            {
                if (WindowsUtils.IsWindowsNT) return ProcessUtils.RunAssemblyAsAdmin("0install-win", new[] {"run"}.Concat(args).JoinEscapeArguments());
                else
                {
                    Log.Error(ex);
                    return 1;
                }
            }
            catch (NeedGuiException ex)
            {
                if (WindowsUtils.IsWindows) return ProcessUtils.RunAssembly("0install-win", new[] {"run"}.Concat(args).JoinEscapeArguments());
                else
                {
                    Log.Error(ex);
                    return 1;
                }
            }
            catch (OptionException ex)
            {
                var messsage = new StringBuilder(ex.Message);
                if (ex.InnerException != null) messsage.Append("\n" + ex.InnerException.Message);
                messsage.Append("\n" + string.Format(Resources.TryHelp, ExeName));
                Log.Error(messsage.ToString());
                return 1;
            }
            catch (Win32Exception ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (BadImageFormatException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (WebException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (IOException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (DigestMismatchException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (SignatureException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (FormatException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (SolverException ex)
            {
                Log.Error(ex);
                return 1;
            }
            catch (ExecutorException ex)
            {
                Log.Error(ex);
                return 1;
            }
                #endregion

            finally
            {
                handler.CloseUI();
            }
        }
    }
}
