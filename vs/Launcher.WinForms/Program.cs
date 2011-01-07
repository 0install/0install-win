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
using System.Text;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Fetchers;
using ZeroInstall.Launcher.Arguments;
using ZeroInstall.Launcher.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Launcher.WinForms
{
    /// <summary>
    /// Launches Zero Install implementations and displays a WinForms GUI.
    /// </summary>
    public static class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ErrorReportForm.RunAppMonitored(delegate
            {
                var handler = new MainForm();
                ParseResults results;
                OperationMode mode;

                try { mode = ParseArgs(args, handler, out results); }
                #region Error handling
                catch (ArgumentException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    return;
                }
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    return;
                }
                #endregion

                try { ExecuteArgs(mode, results, handler); }
                #region Error hanlding
                catch (UserCancelException)
                {}
                catch (ArgumentException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (WebException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (SolverException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (FetcherException ex)
                {
                    Msg.Inform(null, (ex.InnerException ?? ex).Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (DigestMismatchException ex)
                {
                    // ToDo: Display generated manifest
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (ImplementationNotFoundException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (CommandException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (Win32Exception ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                catch (BadImageFormatException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    handler.CloseAsync();
                }
                #endregion
            });
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
        private static OperationMode ParseArgs(IEnumerable<string> args, IHandler handler, out ParseResults results)
        {
            // Prepare a structure for storing settings found in the arguments
            var mode = OperationMode.Normal;
            var parseResults = new ParseResults {Policy = Policy.CreateDefault()};

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {"i|import", unused => mode = OperationMode.Import},
                {"l|list", unused => mode = OperationMode.List},
                {"f|feed", unused => mode = OperationMode.Manage},
                {"V|version", unused => mode = OperationMode.Version},

                // Policy options
                {"command=", command => parseResults.Policy.CommandName = command},
                {"before=", version => parseResults.Policy.Constraint.BeforeVersion = new ImplementationVersion(version)},
                {"not-before=", version => parseResults.Policy.Constraint.NotBeforeVersion = new ImplementationVersion(version)},
                {"s|source", unused => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Cpu.Source)},
                {"os=", os => parseResults.Policy.Architecture = new Architecture(Architecture.ParseOS(os), parseResults.Policy.Architecture.Cpu)},
                {"cpu=", cpu => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Architecture.ParseCpu(cpu))},
                {"o|offline", unused => parseResults.Policy.FeedManager.NetworkLevel = NetworkLevel.Offline},
                {"r|refresh", unused => parseResults.Policy.FeedManager.Refresh = true},
                {"with-store=", path => parseResults.Policy.AdditionalStore = new DirectoryStore(path)},

                // Special operations
                {"d|download-only", unused => parseResults.DownloadOnly = true},
                {"set-selections=", file => parseResults.SelectionsFile = file},
                {"batch", unused => handler.Batch = true},

                // Launcher options
                {"m|main=", newMain => parseResults.Main = newMain},
                {"w|wrapper=", newWrapper => parseResults.Wrapper = newWrapper},

                // Operation modifiers
                {"no-wait", unused => parseResults.NoWait = true}
            };
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

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="mode">The operation mode selected by the parsing process.</param>
        /// <param name="results">The parser results to be executed.</param>
        /// <param name="handler">A callback object that controls the UI.</param>
        /// <exception cref="UserCancelException">Thrown if a download or IO task was cancelled.</exception>
        /// <exception cref="ArgumentException">Thrown if the number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to an <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="FetcherException">Thrown if an <see cref="Model.Implementation"/> could not be downloaded.</exception>
        /// <exception cref="DigestMismatchException">Thrown if an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        private static void ExecuteArgs(OperationMode mode, ParseResults results, MainForm handler)
        {
            switch (mode)
            {
                case OperationMode.Normal:
                    // Ask for URI via GUI if none was specified on command-line
                    if (string.IsNullOrEmpty(results.Feed))
                    {
                        results.Feed = InputBox.Show("Please enter the URI of a Zero Install interface here:", "Zero Install");
                        if (string.IsNullOrEmpty(results.Feed)) return;
                    }
                        
                    handler.ShowAsync();
                    Normal(results, handler);
                    break;

                case OperationMode.List:
                    if (results.AdditionalArgs.Count != 0) throw new ArgumentException("Too many arguments");
                    List(results);
                    break;

                case OperationMode.Import:
                case OperationMode.Manage:
                    // ToDo: Implement
                    Msg.Inform(null, "Not implemented yet!", MsgSeverity.Error);
                    break;

                case OperationMode.Version:
                    Msg.Inform(null, string.Format(@"Zero Install Launcher WinForms v{0}", Application.ProductVersion), MsgSeverity.Info);
                    break;

                default:
                    Msg.Inform(null, "Unknown operation mode", MsgSeverity.Error);
                    break;
            }
        }
        #endregion

        //--------------------//

        #region Normal
        /// <summary>
        /// Launches the interface specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <param name="handler">A callback object that controls the UI.</param>        
        /// <exception cref="UserCancelException">Thrown if a download or IO task was cancelled.</exception>
        /// <exception cref="ArgumentException">Thrown if <see cref="ParseResults.Feed"/> is not a valid URI or an existing local file.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to an <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="FetcherException">Thrown if an <see cref="Model.Implementation"/> could not be downloaded.</exception>
        /// <exception cref="DigestMismatchException">Thrown if an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        private static void Normal(ParseResults results, MainForm handler)
        {
            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy, handler);

            if (results.SelectionsFile == null) controller.Solve();
            else controller.SetSelections(Selections.Load(results.SelectionsFile));

            controller.DownloadUncachedImplementations();

            handler.CloseAsync();

            if (!results.DownloadOnly)
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
        /// Prints a list of feeds in the cache to a message box.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        private static void List(ParseResults results)
        {
            var feeds = results.Policy.FeedManager.Cache.ListAll();
            var builder = new StringBuilder("Found interfaces:\n");
            foreach (Uri entry in feeds)
            {
                if (results.Feed == null || entry.ToString().Contains(results.Feed))
                    builder.AppendLine(entry.ToString());
            }
            Msg.Inform(null, builder.ToString(), MsgSeverity.Info);
        }
        #endregion
    }
}
