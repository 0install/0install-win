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
using Common.Helpers;
using NDesk.Options;
using ZeroInstall.Injector.Cli.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector.Cli
{
    #region Enumerations
    /// <summary>
    /// List of operational modes that can be selected via command-line arguments.
    /// </summary>
    public enum ProgramMode
    {
        /// <summary>Launch or download an <see cref="Model.Feed"/>.</summary>
        Normal,
        /// <summary>List known <see cref="Model.Feed"/>s.</summary>
        List,
        /// <summary>Import feed files from local source.</summary>
        Import,
        /// <summary>Add feed aliases.</summary>
        Manage,
        /// <summary>Display version information.</summary>
        Version
    }
    #endregion

    #region Structures
    /// <summary>
    /// Structure for storing results of parsing command-line arguments.
    /// </summary>
    public struct ParseResults
    {
        /// <summary>The selected operational mode.</summary>
        public ProgramMode Mode;

        /// <summary>User settings controlling the dependency solving process.</summary>
        public Policy Policy;

        /// <summary>Only download <see cref="Implementation"/>s but don't execute them.</summary>
        public bool DownloadOnly;

        /// <summary>Only output what was supposed to be downloaded but don't actually use the network.</summary>
        public bool DryRun;

        /// <summary>Print the selected <see cref="Implementation"/>s to the console instead of executing them.</summary>
        public bool GetSelections;

        /// <summary>Only download feeds and not <see cref="Implementation"/>s.</summary>
        public bool SelectOnly;

        /// <summary>Load <see cref="Selections"/> from this file instead of using an <see cref="ISolver"/>.</summary>
        public string SelectionsFile;

        /// <summary>An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="ImplementationBase.Main"/>.</summary>
        public string Main;

        /// <summary>Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.</summary>
        public string Wrapper;

        /// <summary>The interface to launch, feed to download/add, term to search for, etc.</summary>
        public string Feed;

        /// <summary>Arguments to pass to the launched application, additional feeds to add, additional terms to search for, etc.</summary>
        public IList<string> AdditionalArgs;
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
        static void Main(string[] args)
        {
#if DEBUG
            Execute(ParseArgs(args));
#else
            try { Execute(ParseArgs(args)); }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
#endif
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The arguments to be parsed.</param>
        /// <returns>The results of the parsing process.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        public static ParseResults ParseArgs(IEnumerable<string> args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            // Prepare a structure for storing settings found in the arguments
            var results = new ParseResults {Policy = Policy.CreateDefault()};

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {"i|import", Resources.OptionImport, unused => results.Mode = ProgramMode.Import},
                {"l|list", Resources.OptionList, unused => results.Mode = ProgramMode.List},
                {"f|feed", Resources.OptionFeed, unused => results.Mode = ProgramMode.Manage},
                {"V|version", Resources.OptionHelp, unused => results.Mode = ProgramMode.Version},

                // Policy options
                {"before=", Resources.OptionBefore, version => results.Policy.Constraint = new Constraint(results.Policy.Constraint.NotBeforeVersion, new ImplementationVersion(version))},
                {"not-before=", Resources.OptionNotBefore, version => results.Policy.Constraint = new Constraint(new ImplementationVersion(version), results.Policy.Constraint.BeforeVersion)},
                {"s|source", Resources.OptionSource, unused => results.Policy.Architecture = new Architecture(results.Policy.Architecture.OS, Cpu.Source)},
                {"os=", Resources.OptionOS, os => results.Policy.Architecture = new Architecture(Architecture.ParseOS(os), results.Policy.Architecture.Cpu)},
                {"cpu=", Resources.OptionCpu, cpu => results.Policy.Architecture = new Architecture(results.Policy.Architecture.OS, Architecture.ParseCpu(cpu))},
                {"o|offline", Resources.OptionOffline, unused =>  results.Policy.InterfaceCache.NetworkLevel = NetworkLevel.Offline},
                {"r|refresh", Resources.OptionRefresh, unused => results.Policy.InterfaceCache.Refresh = true},
                {"with-store=", Resources.OptionWithStore, path => results.Policy.AdditionalStore = new DirectoryStore(path)},

                // Special operations
                {"d|download-only", Resources.OptionDownloadOnly, unused => results.DownloadOnly = true},
                {"D|dry-run", Resources.OptionDryRun, unused => results.DryRun = true},
                {"get-selections", Resources.OptionGetSelections, unused => results.GetSelections = true},
                {"select-only", Resources.OptionSelectOnly, unused => results.SelectOnly = true},
                {"set-selections=", Resources.OptionSetSelections, file => results.SelectionsFile = file},

                // Launcher options
                {"m|main=", Resources.OptionMain, newMain => results.Main = newMain},
                {"w|wrapper=", Resources.OptionWrapper, newWrapper => results.Wrapper = newWrapper}
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                Console.WriteLine(@"Usage: 0launch [options] -- interface [args]
       0launch --list [search-term]
       0launch --import [signed-interface-files]
       0launch --feed [interface]");
                options.WriteOptionDescriptions(Console.Out);
            });
            #endregion

            #region Feed and arguments
            var targetArgs = new List<string>();
            results.AdditionalArgs = targetArgs;
            options.Add("<>", v =>
            {
                if (results.Feed == null)
                {
                    if (v.StartsWith("-")) throw new ArgumentException("Unknown options");

                    results.Feed = v;
                    options.Clear();
                }
                else targetArgs.Add(v);
            });
            #endregion

            options.Parse(args);

            return results;
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by parsing command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        public static void Execute(ParseResults results)
        {
            switch (results.Mode)
            {
                #region Normal
                case ProgramMode.Normal:
                    {
                        // ToDo: Alternative policy for DryRun

                        var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy);

                        if (results.SelectionsFile == null) controller.Solve();
                        else controller.SetSelections(Selections.Load(results.SelectionsFile));

                        if (!results.SelectOnly)
                        {
                            // ToDo: Add progress callbacks
                            controller.DownloadUncachedImplementations();
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
                            launcher.RunSync(StringHelper.Concatenate(results.AdditionalArgs, " "));
                        }
                        break;
                    }
                #endregion

                case ProgramMode.List:
                case ProgramMode.Import:
                case ProgramMode.Manage:
                    // ToDo: Implement
                    throw new NotImplementedException();

                case ProgramMode.Version:
                    // ToDo: Read version number from assembly data
                    Console.WriteLine(@"Zero Install for Windows Injector v{0}", "1.0");
                    break;
            }
        }
        #endregion
    }
}
