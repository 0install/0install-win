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
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Controls;
using NDesk.Options;
using ZeroInstall.Launcher.Arguments;
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
                    Msg.Inform(null, ex.Message, MsgSeverity.Warning);
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warning);
                    return;
                }
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warning);
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warning);
                    return;
                }
                #endregion

                switch (mode)
                {
                    case OperationMode.Normal:
                        // Ask for URI via GUI if none was specified on command-line
                        if (string.IsNullOrEmpty(results.Feed))
                        {
                            results.Feed = InputBox.Show("Please enter the URI of a Zero Install interface here:", "Zero Install");
                            if (string.IsNullOrEmpty(results.Feed)) return;
                        }

                        handler.Execute(results);
                        break;

                    case OperationMode.List:
                    case OperationMode.Import:
                    case OperationMode.Manage:
                        Msg.Inform(null, "Not implemented yet!", MsgSeverity.Error);
                        break;

                    case OperationMode.Version:
                        // ToDo: Read version number from assembly data
                        Msg.Inform(null, "Zero Install for Windows Launcher v1.0", MsgSeverity.Information);
                        break;

                    default:
                        Msg.Inform(null, "Unknown operation mode", MsgSeverity.Error);
                        break;
                }
            });
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
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
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
                {"i|import", unused => mode = OperationMode.Import},
                {"l|list", unused => mode = OperationMode.List},
                {"f|feed", unused => mode = OperationMode.Manage},
                {"V|version", unused => mode = OperationMode.Version},

                // Policy options
                {"before=", version => parseResults.Policy.Constraint.BeforeVersion = new ImplementationVersion(version)},
                {"not-before=", version => parseResults.Policy.Constraint.NotBeforeVersion = new ImplementationVersion(version)},
                {"s|source", unused => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Cpu.Source)},
                {"os=", os => parseResults.Policy.Architecture = new Architecture(Architecture.ParseOS(os), parseResults.Policy.Architecture.Cpu)},
                {"cpu=", cpu => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Architecture.ParseCpu(cpu))},
                {"o|offline", unused =>  parseResults.Policy.FeedProvider.NetworkLevel = NetworkLevel.Offline},
                {"r|refresh", unused => parseResults.Policy.FeedProvider.Refresh = true},
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
    }
}
