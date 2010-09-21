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
using Common;
using Common.Helpers;
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
    /// A list of errorlevels that are returned to the original caller after the application terminates, to indicate success or the reason for failure.
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
            ParseResults results;
            OperationMode mode;

            try
            { mode = ParseArgs(args, new CliHandler(), out results); }
            #region Error handling
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            switch (mode)
            {
                case OperationMode.Normal:
                    if (string.IsNullOrEmpty(results.Feed))
                    {
                        Console.Error.WriteLine("Missing arguments. Try 0launch --help");
                        return (int)ErrorLevel.InvalidArguments;
                    }
                    
                    return Execute(results);

                case OperationMode.List:
                    List(results);
                    return (int)ErrorLevel.OK;

                case OperationMode.Import:
                case OperationMode.Manage:
                    // ToDo: Implement
                    Console.Error.WriteLine("Not implemented yet!");
                    return (int)ErrorLevel.NotSupported;

                case OperationMode.Version:
                    // ToDo: Read version number from assembly data
                    Console.WriteLine(@"Zero Install for Windows Injector v{0}", "0.50.0");
                    return (int)ErrorLevel.OK;

                default:
                    Console.Error.WriteLine("Unknown operation mode");
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
                {"V|version", Resources.OptionHelp, unused => mode = OperationMode.Version},

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
        /// <returns>The error level to report to the original caller. 0 for everything OK, 1 or larger for an error.</returns>
        private static int Execute(ParseResults results)
        {
            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy);

            if (results.SelectionsFile == null)
            {
                try { controller.Solve(); }
                #region Error hanlding
                catch (IOException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.IOError;
                }
                catch (SolverException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.SolverError;
                }
                #endregion
            }
            else controller.SetSelections(Selections.Load(results.SelectionsFile));

            if (!results.SelectOnly)
            {
                try { controller.DownloadUncachedImplementations(); }
                #region Error hanlding
                catch (UserCancelException)
                {
                    return (int)ErrorLevel.UserCanceled;
                }
                catch (WebException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.WebError;
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.IOError;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.IOError;
                }
                catch (DigestMismatchException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.DigestMismatch;
                }
                #endregion
            }

            if (results.GetSelections)
            {
                Console.Write(controller.GetSelections().WriteToString());
            }
            else if (!results.DownloadOnly && !results.SelectOnly)
            {
                var launcher = controller.GetLauncher();
                launcher.Main = results.Main;
                launcher.Wrapper = results.Wrapper;
                try { launcher.RunSync(StringHelper.Concatenate(results.AdditionalArgs, " ")); }
                #region Error hanlding
                catch (ImplementationNotFoundException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.ImplementationError;
                }
                catch (MissingMainException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.ImplementationError;
                }
                catch (Win32Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.IOError;
                }
                catch (BadImageFormatException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (int)ErrorLevel.IOError;
                }
                #endregion
            }

            return (int)ErrorLevel.OK;
        }
        #endregion

        #region List
        private static void List(ParseResults results)
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
