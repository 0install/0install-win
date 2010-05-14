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
using ZeroInstall.Solver;

namespace ZeroInstall.Injector.Cli
{
    internal static class Program
    {
        #region Enumerations
        private enum Mode
        {
            /// <summary>Launch or download an <see cref="Model.Interface"/>.</summary>
            Interface,
            /// <summary>List known <see cref="Model.Interface"/>s.</summary>
            List,
            /// <summary>Import feed files from local source.</summary>
            Import,
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
            var policy = new DefaultPolicy();

            Mode mode = Mode.Interface;
            string selectionsFile = null, main = null, wrapper = null;
            bool dryRun = false, downloadOnly = false, registerFeed = false, getSelections = false, selectOnly = false;

            var options = new OptionSet
            {
                {"i", unused => mode = Mode.Import},
                {"import", Resources.OptionImport, unused => mode = Mode.Import},
                {"l", unused => mode = Mode.List},
                {"list", Resources.OptionList, unused => mode = Mode.List},
                {"d", unused => downloadOnly = true},
                {"download-only", Resources.OptionDownloadOnly, unused => downloadOnly = true},
                {"f", unused => registerFeed = true},
                {"feed", Resources.OptionFeed, unused => registerFeed = true},
                {"get-selections", Resources.OptionGetSelections, unused => getSelections = true},
                {"select-only", Resources.OptionSelectOnly, unused => selectOnly = true},
                {"set-selections", Resources.OptionSetSelections, file => selectionsFile = file},
                {"before", Resources.OptionBefore, version => policy.Solver.Before = new ImplementationVersion(version)},
                {"not-before", Resources.OptionNotBefore, version => policy.Solver.NotBefore = new ImplementationVersion(version)},
                {"s", unused => policy.Solver.Source = true},
                {"source", Resources.OptionSource, unused => policy.Solver.Source = true},
                {"m", newMain => main = newMain},
                {"main", Resources.OptionMain, newMain => main = newMain},
                {"w", newWrapper => wrapper = newWrapper},
                {"wrapper", Resources.OptionWrapper, newWrapper => wrapper = newWrapper},
                {"o", unused => policy.Solver.InterfaceProvider.Offline = true},
                {"offline", Resources.OptionOffline, unused => policy.Solver.InterfaceProvider.Offline = true},
                {"r", unused => policy.Solver.InterfaceProvider.Refresh = true},
                {"refresh", Resources.OptionRefresh, unused => policy.Solver.InterfaceProvider.Refresh = true},
                {"D", unused => dryRun = true},
                {"dry-run", Resources.OptionDryRun, unused => dryRun = true},
                {"h", unused => mode = Mode.Help},
                {"help", Resources.OptionHelp, unused => mode = Mode.Help},
                {"V", unused => mode = Mode.Version},
                {"version", Resources.OptionHelp, unused => mode = Mode.Version}
            };

            var additional = options.Parse(args);

            switch (mode)
            {
                case Mode.Interface:
                {
                    Launcher launcher = null;
                    if (!selectOnly)
                    {
                        if (string.IsNullOrEmpty(selectionsFile))
                        {
                            launcher = policy.GetLauncher(additional[0]);
                        }
                        else
                        {
                            launcher = new Launcher(Selections.Load(selectionsFile), policy.Store);
                        }
                    }

                    if (getSelections)
                    {
                        Console.Write(policy.GetSelections(additional[0]).WriteToString());
                    }
                    else if (!downloadOnly && launcher != null)
                    {
                        string arguments = StringHelper.Concatenate(additional.GetRange(1, additional.Count - 1), " ");
                        launcher.Main = main;
                        launcher.Wrapper = wrapper;
                        launcher.Run(arguments);
                    } 
                    break;
                }

                case Mode.Help:
                    options.WriteOptionDescriptions(Console.Out);
                    break;
            }
        }
        #endregion
    }
}
