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
using System.IO;
using System.Windows.Forms;
using Common.Helpers;
using NDesk.Options;
using ZeroInstall.Injector.Cli.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Interface;

namespace ZeroInstall.Injector.Cli
{
    internal static class Program
    {
        #region Enumerations
        private enum Mode
        {
            /// <summary>Launch or download an <see cref="Model.Interface"/>.</summary>
            Normal,
            /// <summary>List known <see cref="Model.Interface"/>s.</summary>
            List,
            /// <summary>Import feed files from local source.</summary>
            Import,
            /// <summary>Add feed aliases.</summary>
            Manage,
            /// <summary>Show the built-in help text.</summary>
            Help,
            /// <summary>Display version information.</summary>
            Version
        }
        #endregion

        #region Properties
        /// <summary>
        /// The directory where the executable file is located.
        /// </summary>
        public static string AppDir
        {
            get { return Path.GetDirectoryName(Application.ExecutablePath); }
        }

        /// <summary>
        /// The name of the executable file.
        /// </summary>
        public static string AppName
        {
            get { return Path.GetFileNameWithoutExtension(Application.ExecutablePath); }
        }
        #endregion

        //--------------------//

        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            #region Option variables
            Mode mode = Mode.Normal;
            bool downloadOnly = false, dryRun = false, getSelections = false, selectOnly = false;
            string selectionsFile = null;
            var constraint = new Constraint();
            bool source = false, offline = false, refresh = false;
            IStore additionalStore = null;
            string main = null, wrapper = null;
            #endregion

            #region Command-line options
            var options = new OptionSet
            {
                // Mode selection
                {"i|import", Resources.OptionImport, unused => mode = Mode.Import},
                {"l|list", Resources.OptionList, unused => mode = Mode.List},
                {"f|feed", Resources.OptionFeed, unused => mode = Mode.Manage},
                {"h|help|?", Resources.OptionHelp, unused => mode = Mode.Help},
                {"V|version", Resources.OptionHelp, unused => mode = Mode.Version},

                // Special operations
                {"d|download-only", Resources.OptionDownloadOnly, unused => downloadOnly = true},
                {"D|dry-run", Resources.OptionDryRun, unused => dryRun = true},
                {"get-selections", Resources.OptionGetSelections, unused => getSelections = true},
                {"select-only", Resources.OptionSelectOnly, unused => selectOnly = true},
                {"set-selections=", Resources.OptionSetSelections, file => selectionsFile = file},

                // Policy options
                {"before=", Resources.OptionBefore, version => constraint.BeforeVersion = new ImplementationVersion(version)},
                {"not-before=", Resources.OptionNotBefore, version => constraint.NotBeforeVersion = new ImplementationVersion(version)},
                {"s|source", Resources.OptionSource, unused => source = true},

                // Interface provider options
                {"o|offline", Resources.OptionOffline, unused => offline = true},
                {"r|refresh", Resources.OptionRefresh, unused => refresh = true},
                {"with-store=", Resources.OptionWithStore, path => additionalStore = new DirectoryStore(path)},

                // Launcher options
                {"m|main=", Resources.OptionMain, newMain => main = newMain},
                {"w|wrapper=", Resources.OptionWrapper, newWrapper => wrapper = newWrapper}
            };
            #endregion

            // ToDo: Prevent parsing of interspersed arguments
            var additional = options.Parse(args);

            switch (mode)
            {
                case Mode.Normal:
                {
                    // ToDo: Alternative policy for DryRun
                    Policy policy = Policy.CreateDefault(additional[0]);
                    policy.InterfaceCache.Refresh = refresh;
                    if (offline) policy.InterfaceCache.NetworkLevel = NetworkLevel.Offline;
                    policy.AdditionalStore = additionalStore;
                    policy.Source = source;
                    policy.Constraint = constraint;

                    if (selectionsFile == null) policy.Solve();
                    else policy.SetSelections(Selections.Load(selectionsFile));

                    if (!selectOnly)
                    {
                        // ToDo: Add progress callbacks
                        policy.DownloadUncachedImplementations();
                    }

                    if (getSelections)
                    {
                        Console.Write(policy.GetSelections().WriteToString());
                    }
                    else if (!downloadOnly && !selectOnly)
                    {
                        var launcher = policy.GetLauncher();
                        launcher.Main = main;
                        launcher.Wrapper = wrapper;

                        string arguments = StringHelper.Concatenate(additional.GetRange(1, additional.Count - 1), " ");
                        launcher.Execute(arguments);
                    }
                    break;
                }

                case Mode.Help:
                    Console.WriteLine(@"Usage: 0launch [options] -- interface [args]
       0launch --list [search-term]
       0launch --import [signed-interface-files]
       0launch --feed [interface]");
                    options.WriteOptionDescriptions(Console.Out);
                    break;

                case Mode.Version:
                    Console.WriteLine(@"Zero Install for Windows Injector v{0}", Application.ProductVersion);
                    break;
            }
        }
        #endregion
    }
}
