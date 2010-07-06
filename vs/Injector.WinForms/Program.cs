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
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector.WinForms
{
    /// <summary>
    /// GUI application for launching applications via the Zero Install Injector.
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
                // Policy options
                {"before=", version => results.Policy.Constraint = new Constraint(results.Policy.Constraint.NotBeforeVersion, new ImplementationVersion(version))},
                {"not-before=", version => results.Policy.Constraint = new Constraint(new ImplementationVersion(version), results.Policy.Constraint.BeforeVersion)},
                {"s|source", unused => results.Policy.Architecture = new Architecture(results.Policy.Architecture.OS, Cpu.Source)},
                {"os=", os => results.Policy.Architecture = new Architecture(Architecture.ParseOS(os), results.Policy.Architecture.Cpu)},
                {"cpu=", cpu => results.Policy.Architecture = new Architecture(results.Policy.Architecture.OS, Architecture.ParseCpu(cpu))},
                {"o|offline", unused =>  results.Policy.InterfaceCache.NetworkLevel = NetworkLevel.Offline},
                {"r|refresh", unused => results.Policy.InterfaceCache.Refresh = true},
                {"with-store=", path => results.Policy.AdditionalStore = new DirectoryStore(path)},

                // Special operations
                {"d|download-only", unused => results.DownloadOnly = true},
                {"set-selections=", file => results.SelectionsFile = file},

                // Launcher options
                {"m|main=", newMain => results.Main = newMain},
                {"w|wrapper=", newWrapper => results.Wrapper = newWrapper}
            };
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
            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy);

            if (results.SelectionsFile == null) controller.Solve();
            else controller.SetSelections(Selections.Load(results.SelectionsFile));

            // ToDo: Add progress callbacks
            var progress = new DownloadProgessForm();
            progress.Show();
            progress.Refresh();
            controller.DownloadUncachedImplementations();
            progress.Close();

            if (!results.DownloadOnly)
            {
                var launcher = controller.GetLauncher();
                launcher.Main = results.Main;
                launcher.Wrapper = results.Wrapper;
                launcher.RunSync(StringHelper.Concatenate(results.AdditionalArgs, " "));
            }
        }
        #endregion
    }
}
