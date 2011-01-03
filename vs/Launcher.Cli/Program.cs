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
using ZeroInstall.Fetchers;
using ZeroInstall.Launcher.Arguments;
using ZeroInstall.Launcher.Cli.Properties;
using ZeroInstall.Model;
using ZeroInstall.Launcher.Solver;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

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

        /// <summary>The user cancelled the operation.</summary>
        UserCancelled = 1,

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
    /// Launches Zero Install implementations and displays a command-line interface.
    /// </summary>
    public static class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            // Automatically show help for missing args
            if (args.Length == 0) args = new[] {"--help"};

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

            try { return ExecuteArgs(mode, results, handler); }
            #region Error hanlding
            catch (UserCancelException)
            {
                return (int)ErrorLevel.UserCancelled;
            }
            catch (ArgumentException ex)
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
            catch (MissingMainException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.ImplementationError;
            }
            catch (DigestMismatchException ex)
            {
                Log.Error(ex.Message);
                Log.Info("Generated manifest:\n" + ex.ActualManifest);
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
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <param name="results">The options detected by the parsing process.</param>
        /// <returns>The operation mode selected by the parsing process.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static OperationMode ParseArgs(IEnumerable<string> args, IHandler handler, out ParseResults results)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            // Prepare a structure for storing settings found in the arguments
            var mode = OperationMode.Normal;
            var parseResults = new ParseResults {Policy = Policy.CreateDefault()};

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {"i|import", Resources.OptionImport, unused => mode = OperationMode.Import},
                {"l|list", Resources.OptionList, unused => mode = OperationMode.List},
                {"f|feed", Resources.OptionFeed, unused => mode = OperationMode.Manage},
                {"V|version", Resources.OptionVersion, unused => mode = OperationMode.Version},

                // Policy options
                {"before=", version => parseResults.Policy.Constraint.BeforeVersion = new ImplementationVersion(version)},
                {"not-before=", version => parseResults.Policy.Constraint.NotBeforeVersion = new ImplementationVersion(version)},
                {"s|source", Resources.OptionSource, unused => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Cpu.Source)},
                {"os=", Resources.OptionOS, os => parseResults.Policy.Architecture = new Architecture(Architecture.ParseOS(os), parseResults.Policy.Architecture.Cpu)},
                {"cpu=", Resources.OptionCpu, cpu => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Architecture.ParseCpu(cpu))},
                {"o|offline", Resources.OptionOffline, unused => parseResults.Policy.FeedManager.NetworkLevel = NetworkLevel.Offline},
                {"r|refresh", Resources.OptionRefresh, unused => parseResults.Policy.FeedManager.Refresh = true},
                {"with-store=", Resources.OptionWithStore, path => parseResults.Policy.AdditionalStore = new DirectoryStore(path)},

                // Special operations
                {"d|download-only", Resources.OptionDownloadOnly, unused => parseResults.DownloadOnly = true},
                {"set-selections=", Resources.OptionSetSelections, file => parseResults.SelectionsFile = file},
                {"get-selections", Resources.OptionGetSelections, unused => parseResults.GetSelections = true},
                {"select-only", Resources.OptionSelectOnly, unused => parseResults.SelectOnly = true},
                {"batch", Resources.OptionBatch, unused => handler.Batch = true},

                // Launcher options
                {"m|main=", Resources.OptionMain, newMain => parseResults.Main = newMain},
                {"w|wrapper=", Resources.OptionWrapper, newWrapper => parseResults.Wrapper = newWrapper},

                // Operation modifiers
                {"no-wait", Resources.OptionNoWait, unused => parseResults.NoWait = true},
                {"D|dry-run", Resources.OptionDryRun, unused => parseResults.DryRun = true},
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                mode = OperationMode.Help;

                PrintUsage();
                Console.WriteLine(Resources.Options);
                options.WriteOptionDescriptions(Console.Out);
            });
            #endregion

            #region Feed and arguments
            var targetArgs = new List<string>();
            parseResults.AdditionalArgs = targetArgs;
            options.Add("<>", value =>
            {
                if (parseResults.Feed == null)
                {
                    if (value.StartsWith("-")) throw new ArgumentException(Resources.UnknownOption);

                    parseResults.Feed = value;
                    options.Clear();
                }
                else targetArgs.Add(value);
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            options.Parse(args);

            // Return the now filled results structure
            results = parseResults;
            return mode;
        }
        #endregion

        #region Help
        private static void PrintUsage()
        {
            const string usage = "{0}\t{1}\n\t{2}\n\t{3}\n\t{4}\n";
            Console.WriteLine(usage, Resources.Usage, Resources.UsageNormal, Resources.UsageList, Resources.UsageImport, Resources.UsageFeed);
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="mode">The operation mode selected by the parsing process.</param>
        /// <param name="results">The parser results to be executed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <returns>The error code to end the process with.</returns>
        /// <exception cref="UserCancelException">Thrown if a download or IO task was cancelled.</exception>
        /// <exception cref="ArgumentException">Thrown if the number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to an <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="FetcherException">Thrown if an <see cref="Model.Implementation"/> could not be downloaded.</exception>
        /// <exception cref="DigestMismatchException">Thrown if an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="MissingMainException">Thrown if there is no main executable specified for the main <see cref="ImplementationBase"/>.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        private static int ExecuteArgs(OperationMode mode, ParseResults results, IHandler handler)
        {
            switch (mode)
            {
                case OperationMode.Normal:
                    Normal(results, handler);
                    return (int)ErrorLevel.OK;

                case OperationMode.List:
                    List(results);
                    return (int)ErrorLevel.OK;

                case OperationMode.Import:
                case OperationMode.Manage:
                    // ToDo: Implement
                    Log.Error(Resources.NotImplemented);
                    return (int)ErrorLevel.NotSupported;

                case OperationMode.Version:
                    Console.WriteLine(@"Zero Install Launcher CLI v{0}", Assembly.GetEntryAssembly().GetName().Version);
                    return (int)ErrorLevel.OK;

                case OperationMode.Help:
                    // Help text was already printed
                    return (int)ErrorLevel.OK;

                default:
                    Log.Error(Resources.UnknownMode);
                    return (int)ErrorLevel.NotSupported;
            }
        }
        #endregion

        //--------------------//

        #region Normal
        /// <summary>
        /// Launches the interface specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <exception cref="UserCancelException">Thrown if a download or IO task was cancelled.</exception>
        /// <exception cref="ArgumentException">Thrown if <see cref="ParseResults.Feed"/> is not a valid URI or an existing local file.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to an <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="FetcherException">Thrown if an <see cref="Model.Implementation"/> could not be downloaded.</exception>
        /// <exception cref="DigestMismatchException">Thrown if an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="MissingMainException">Thrown if there is no main executable specified for the main <see cref="ImplementationBase"/>.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        private static void Normal(ParseResults results, IHandler handler)
        {
            if (string.IsNullOrEmpty(results.Feed)) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageNormal));

            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy, handler);

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
                var executor = controller.GetExecutor();
                executor.Main = results.Main;
                executor.Wrapper = results.Wrapper;

                var startInfo = executor.GetStartInfo(StringUtils.Concatenate(results.AdditionalArgs, " "));
                if (results.NoWait) ProcessUtils.RunDetached(startInfo);
                else ProcessUtils.RunReplace(startInfo);
            }
        }
        #endregion

        #region List
        /// <summary>
        /// Prints a list of feeds in the cache to the console.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="ArgumentException">Thrown if the number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        private static void List(ParseResults results)
        {
            if (results.AdditionalArgs.Count != 0) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageList));

            var feeds = results.Policy.FeedManager.Cache.ListAll();
            foreach (Uri entry in feeds)
            {
                if (results.Feed == null || entry.ToString().Contains(results.Feed))
                    Console.WriteLine(entry);
            }
        }
        #endregion
    }
}
