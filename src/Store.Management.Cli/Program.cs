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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Common;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Store.Management.Cli.Properties;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.Cli
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

        /// <summary>A requested implementation could not be found or could not be launched.</summary>
        ImplementationError = 15,

        /// <summary>A manifest digest for an implementation did not match the expected value.</summary>
        DigestMismatch = 20,
    }
    #endregion

    /// <summary>
    /// Launches a command-line tool for managing caches of Zero Install implementations.
    /// </summary>
    public static partial class Program
    {
        private static IStore _store;

        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static int Main(string[] args)
        {
            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.Hash(MD5.Create());
            if (AppMutex.Probe(mutexName + "-update")) return 99;
            AppMutex.Create(mutexName);

            // Allow setup to detect Zero Install instances
#if !DEBUG
            AppMutex.Create("Zero Install");
#endif

            // Automatically show help for missing args
            if (args == null) args = new string[0];
            if (args.Length == 0) args = new[] {"--help"};

            IList<string> restArgs;
            try
            {
                restArgs = ParseArgs(args);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return (int)ErrorLevel.OK;
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            if (restArgs.Count == 0) return (int)ErrorLevel.OK;

            try
            {
                _store = StoreProvider.CreateDefault();
                return (int)ExecuteArgs(restArgs, new CliTaskHandler());
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return (int)ErrorLevel.UserCanceled;
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
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
            catch (InvalidDataException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (ImplementationNotFoundException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.ImplementationError;
            }
            catch (ImplementationAlreadyInStoreException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.ImplementationError;
            }
            catch (DigestMismatchException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.DigestMismatch;
            }
            #endregion
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <returns>Any unparsed commands left over.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException">Thrown if <paramref name="args"/> contains unknown options.</exception>
        private static IList<string> ParseArgs(IEnumerable<string> args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {
                    "V|version", Resources.OptionVersion, unused =>
                    {
                        Console.WriteLine(AppInfo.Name + ' ' + AppInfo.Version + (Locations.IsPortable ? " - " + Resources.PortableMode : "") + Environment.NewLine + AppInfo.Copyright + Environment.NewLine + Resources.LicenseInfo);
                        throw new OperationCanceledException(); // Don't handle any of the other arguments
                    }
                },
                // Documentation
                {
                    "man", Resources.OptionMan, unused =>
                    {
                        PrintManual();
                        throw new OperationCanceledException(); // Don't handle any of the other arguments
                    }
                },
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                PrintUsage();
                Console.WriteLine(Resources.Options);
                options.WriteOptionDescriptions(Console.Out);

                // Don't handle any of the other arguments
                throw new OperationCanceledException();
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            return options.Parse(args);
        }
        #endregion

        #region Help
        private static void PrintUsage()
        {
            var usages = new[] {Resources.UsageAdd, Resources.UsageAudit, Resources.UsageCopy, Resources.UsageFind, Resources.UsageList, Resources.UsageManifest, Resources.UsageOptimize, Resources.UsageRemove, Resources.UsageVerify};
            Console.WriteLine(Resources.Usage + '\t' + string.Join(Environment.NewLine + '\t', usages) + '\n');
        }

        private static void PrintManual()
        {
            // ToDo: Add flow formatting for better readability on console
            Console.WriteLine(@"ADD" + Environment.NewLine + Environment.NewLine + Resources.DetailsAdd + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine(@"AUDIT" + Environment.NewLine + Environment.NewLine + Resources.DetailsAudit + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine(@"COPY" + Environment.NewLine + Environment.NewLine + Resources.DetailsCopy + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine(@"FIND" + Environment.NewLine + Environment.NewLine + Resources.DetailsFind + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine(@"LIST" + Environment.NewLine + Environment.NewLine + Resources.DetailsList + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine(@"MANAGE" + Environment.NewLine + Environment.NewLine + Resources.DetailsManage + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            string supportedFormats = string.Join(", ", Array.ConvertAll(ManifestFormat.All, format => format.ToString()));
            Console.WriteLine(@"MANIFEST" + Environment.NewLine + Environment.NewLine + string.Format(Resources.DetailsManifest, supportedFormats) + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine(@"REMOVE" + Environment.NewLine + Environment.NewLine + Resources.DetailsRemove + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine(@"VERIFY" + Environment.NewLine + Environment.NewLine + Resources.DetailsVerify);
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments that were not parsed as options.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <returns>The error level to return when the process ends.</returns>
        /// <exception cref="OperationCanceledException">Thrown if an IO task was canceled.</exception>
        /// <exception cref="ArgumentException">Thrown if the number of arguments passed in <paramref name="args"/> is incorrect.</exception>
        /// <exception cref="NotSupportedException">Thrown if the archive type is unknown or not supported.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if no implementation matching the <see cref="ManifestDigest"/> could be found in this store.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Model.Implementation"/> with the specified <see cref="ManifestDigest"/> in the store.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the archive/directory content doesn't match the <see cref="ManifestDigest"/>.</exception>
        private static ErrorLevel ExecuteArgs(IList<string> args, ITaskHandler handler)
        {
            switch (args[0])
            {
                case "add":
                    return Add(args, handler);

                case "audit":
                    return Audit(args, handler);

                case "copy":
                    Copy(args, handler);
                    return ErrorLevel.OK;

                case "find":
                    Find(args);
                    return ErrorLevel.OK;

                case "remove":
                    Remove(args);
                    return ErrorLevel.OK;

                case "list":
                    List(args);
                    return ErrorLevel.OK;

                case "manage":
                    // ToDo: Automatically switch to GTK# on Linux
                    ProcessUtils.LaunchHelperAssembly("0store-win", null);
                    return ErrorLevel.OK;

                case "manifest":
                    GenerateManifest(args, handler);
                    return ErrorLevel.OK;

                case "optimise":
                    Optimise(args, handler);
                    return ErrorLevel.OK;

                case "verify":
                    Verify(args, handler);
                    return ErrorLevel.OK;

                default:
                    Log.Error(Resources.UnknownMode);
                    return ErrorLevel.NotSupported;
            }
        }
        #endregion
    }
}
