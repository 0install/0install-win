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
            Execute(ParseArgs(args));
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
            var results = new ParseResults {Policy = Policy.CreateDefault(new SilentHandler())};

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

            // Parse the arguments and call the hooked handlers
            options.Parse(args);

            // Return the now filled results structure
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

                #region List
                case ProgramMode.List:
                    if (results.AdditionalArgs.Count != 0) throw new ArgumentException();

                    var interfaces = results.Policy.InterfaceCache.ListAllInterfaces();
                    foreach (var entry in interfaces)
                    {
                        if (results.Feed == null || entry.Contains(results.Feed))
                            Console.WriteLine(entry);
                    }
                    break;
                #endregion

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
