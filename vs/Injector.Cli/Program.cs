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
using System.Reflection;
using Common;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Injector.Arguments;
using ZeroInstall.Injector.Cli.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector.Cli
{
    #region Enumerations
    /// <summary>
    /// An errorlevel is returned to the original caller after the application terminates, to indicate success or the reason for failure.
    /// </summary>
    public enum ErrorLevel
    {
        OK = 0,
        UserCanceled = 1,
        InvalidArguments = 2,
        NotSupported = 3,
        IOError = 10,
        WebError = 11,
        ImplementationError = 12,
        SolverError = 20,
        DigestMismatch = 21
    }
    #endregion

    /// <summary>
    /// Command-line application for launching applications via the Zero Install Injector.
    /// </summary>
    public static class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            var handler = new CliHandler();
            ParseResults results;
            OperationMode mode;

            try { mode = ParseArgs(args, handler, out results); }
            #region Error handling
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            switch (mode)
            {
                case OperationMode.Normal:
                    if (string.IsNullOrEmpty(results.Feed))
                    {
                        Log.Error(string.Format(Resources.MissingArguments, "0launch"));
                        return (int)ErrorLevel.InvalidArguments;
                    }
                    
                    try { Execute(results); }
                    #region Error hanlding
                    catch (UserCancelException)
                    {
                        return (int)ErrorLevel.UserCanceled;
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
                    catch (DigestMismatchException ex)
                    {
                        Log.Error(ex.Message);
                        return (int)ErrorLevel.DigestMismatch;
                    }
                    catch (SolverException ex)
                    {
                        Log.Error(ex.Message);
                        return (int)ErrorLevel.SolverError;
                    }
                    catch (ImplementationNotFoundException ex)
                    {
                        Log.Error(ex.Message);
                        return (int)ErrorLevel.ImplementationError;
                    }
                    catch (MissingMainException ex)
                    {
                        Log.Error(ex.Message);
                        return (int)ErrorLevel.ImplementationError;
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

                case OperationMode.List:
                    List(results);
                    return (int)ErrorLevel.OK;

                case OperationMode.Import:
                case OperationMode.Manage:
                    // ToDo: Implement
                    Log.Error("Not implemented yet!");
                    return (int)ErrorLevel.NotSupported;

                case OperationMode.Version:
                    // ToDo: Read version number from assembly data
                    Console.WriteLine(@"Zero Install Injector CLI v{0}", Assembly.GetEntryAssembly().GetName().Version);
                    return (int)ErrorLevel.OK;

                case OperationMode.Help:
                    return (int)ErrorLevel.OK;

                default:
                    Log.Error("Unknown operation mode");
                    return (int)ErrorLevel.NotSupported;
            }
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The arguments to be parsed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <param name="results">The options detected by the parsing process.</param>
        /// <returns>The operation mode selected by the parsing process.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        public static OperationMode ParseArgs(IEnumerable<string> args, IHandler handler, out ParseResults results)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            // Prepare a structure for storing settings found in the arguments
            var mode = OperationMode.Normal;
            var parseResults = new ParseResults {Policy = Policy.CreateDefault(handler)};

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {"i|import", Resources.OptionImport, unused => mode = OperationMode.Import},
                {"l|list", Resources.OptionList, unused => mode = OperationMode.List},
                {"f|feed", Resources.OptionFeed, unused => mode = OperationMode.Manage},
                {"V|version", Resources.OptionVersion, unused => mode = OperationMode.Version},

                // Policy options
                {"before=", Resources.OptionBefore, version => parseResults.Policy.Constraint = new Constraint(parseResults.Policy.Constraint.NotBeforeVersion, new ImplementationVersion(version))},
                {"not-before=", Resources.OptionNotBefore, version => parseResults.Policy.Constraint = new Constraint(new ImplementationVersion(version), parseResults.Policy.Constraint.BeforeVersion)},
                {"s|source", Resources.OptionSource, unused => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Cpu.Source)},
                {"os=", Resources.OptionOS, os => parseResults.Policy.Architecture = new Architecture(Architecture.ParseOS(os), parseResults.Policy.Architecture.Cpu)},
                {"cpu=", Resources.OptionCpu, cpu => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Architecture.ParseCpu(cpu))},
                {"o|offline", Resources.OptionOffline, unused =>  parseResults.Policy.InterfaceCache.NetworkLevel = NetworkLevel.Offline},
                {"r|refresh", Resources.OptionRefresh, unused => parseResults.Policy.InterfaceCache.Refresh = true},
                {"with-store=", Resources.OptionWithStore, path => parseResults.Policy.AdditionalStore = new DirectoryStore(path)},

                // Special operations
                {"d|download-only", Resources.OptionDownloadOnly, unused => parseResults.DownloadOnly = true},
                {"set-selections=", Resources.OptionSetSelections, file => parseResults.SelectionsFile = file},
                {"get-selections", Resources.OptionGetSelections, unused => parseResults.GetSelections = true},
                {"select-only", Resources.OptionSelectOnly, unused => parseResults.SelectOnly = true},
                {"batch", Resources.OptionBatch, unused => handler.Batch = true},
                {"D|dry-run", Resources.OptionDryRun, unused => parseResults.DryRun = true},

                // Launcher options
                {"m|main=", Resources.OptionMain, newMain => parseResults.Main = newMain},
                {"w|wrapper=", Resources.OptionWrapper, newWrapper => parseResults.Wrapper = newWrapper}
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                mode = OperationMode.Help;

                Console.WriteLine(@"Usage: 0launch [options] -- interface [args]
       0launch --list [search-term]
       0launch --import [signed-interface-files]
       0launch --feed [interface]");
                options.WriteOptionDescriptions(Console.Out);
            });
            #endregion

            #region Feed and arguments
            var targetArgs = new List<string>();
            parseResults.AdditionalArgs = targetArgs;
            options.Add("<>", v =>
            {
                if (parseResults.Feed == null)
                {
                    if (v.StartsWith("-")) throw new ArgumentException("Unknown options");

                    parseResults.Feed = v;
                    options.Clear();
                }
                else targetArgs.Add(v);
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            options.Parse(args);

            // Return the now filled results structure
            results = parseResults;
            return mode;
        }
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="UserCancelException">Thrown if a download, extraction or manifest task was cancelled.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="DigestMismatchException">Thrown an <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to <see cref="Store"/> is not permitted.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="MissingMainException">Thrown if there is no main executable specifed for the main <see cref="ImplementationBase"/>.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        public static void Execute(ParseResults results)
        {
            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy);

            if (results.SelectionsFile == null) controller.Solve();
            else controller.SetSelections(Selections.Load(results.SelectionsFile));

            if (!results.SelectOnly)
                controller.DownloadUncachedImplementations();

            if (results.GetSelections)
            {
                Console.Write(controller.GetSelections().WriteToString());
            }
            else if (!results.DownloadOnly && !results.SelectOnly)
            {
                var launcher = controller.GetLauncher();
                launcher.Main = results.Main;
                launcher.Wrapper = results.Wrapper;
                launcher.RunSync(StringUtils.Concatenate(results.AdditionalArgs, " "));
            }
        }
        #endregion

        #region List
        public static void List(ParseResults results)
        {
            if (results.AdditionalArgs.Count != 0) throw new ArgumentException();

            var interfaces = results.Policy.InterfaceCache.ListAllInterfaces();
            foreach (var entry in interfaces)
            {
                if (results.Feed == null || entry.Contains(results.Feed))
                    Console.WriteLine(entry);
            }
        }
        #endregion
    }
}
