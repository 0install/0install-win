/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Injector
{
    partial class Executor
    {
        /// <summary>
        /// Represents a command-line split into a path and arguments part.
        /// </summary>
        private struct CommandLineSplit
        {
            public readonly string Path;

            public readonly string Arguments;

            public CommandLineSplit(string path, string arguments)
            {
                Path = path;
                Arguments = arguments;
            }
        }

        /// <summary>
        /// Returns the main (first) implementation of the selection.
        /// Replaces the <see cref="Command"/> of the main implementation with the binary specified in <see cref="Main"/> if set.
        /// </summary>
        private ImplementationSelection GetMainImplementation()
        {
            if (string.IsNullOrEmpty(Main)) return Selections.MainImplementation;

            // Clone the first implementation so the command can replaced without affecting Selections
            var mainImplementation = Selections.MainImplementation.CloneImplementation();
            var command = mainImplementation[Selections.Command];

            string mainPath = FileUtils.UnifySlashes(Main);
            command.Path = (mainPath[0] == Path.DirectorySeparatorChar)
                // Relative to implementation root
                ? mainPath.TrimStart(Path.DirectorySeparatorChar)
                // Relative to original command
                : Path.Combine(Path.GetDirectoryName(command.Path) ?? "", mainPath);
            command.Arguments.Clear();

            return mainImplementation;
        }

        /// <summary>
        /// Determines the command-line needed to execute an <see cref="ImplementationSelection"/>. Recursivley handles <see cref="Runner"/>s.
        /// </summary>
        /// <param name="implementation">The implementation to launch.</param>
        /// <param name="commandName">The name of the <see cref="Command"/> within the <paramref name="implementation"/> to launch.</param>
        /// <param name="startInfo">The process launch environment to apply additional <see cref="Binding"/> to.</param>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> points to missing <see cref="Dependency"/>s.</exception>
        /// <exception cref="ImplementationNotFoundException">An <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">A <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">A problem occurred while creating a hard link.</exception>
        private List<ArgBase> GetCommandLine(ImplementationSelection implementation, string commandName, ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            if (string.IsNullOrEmpty(commandName)) throw new ExecutorException(string.Format(Resources.CommandNotSpecified, implementation.InterfaceID));
            Command command = implementation[commandName];

            // Apply bindings implementations use to find themselves and their dependencies
            ApplyBindings(command, implementation, startInfo);
            if (command.WorkingDir != null) ApplyWorkingDir(command.WorkingDir, implementation, startInfo);
            ApplyDependencyBindings(command, startInfo);

            List<ArgBase> commandLine;
            var runner = command.Runner;
            if (runner == null) commandLine = new List<ArgBase>();
            else
            {
                commandLine = GetCommandLine(Selections[runner.InterfaceID], runner.Command ?? Command.NameRun, startInfo);
                commandLine.AddRange(runner.Arguments);
            }

            if (!string.IsNullOrEmpty(command.Path))
            {
                string path = FileUtils.UnifySlashes(command.Path);

                // Fully qualified paths are used by package/native implementations, usually relative to the implementation
                commandLine.Add(Path.IsPathRooted(path) ? path : Path.Combine(GetImplementationPath(implementation), path));
            }
            commandLine.AddRange(command.Arguments);

            return commandLine;
        }

        /// <summary>
        /// Prepends the user-specified <see cref="Wrapper"/>, if any, to the command-line.
        /// </summary>
        /// <param name="commandLine"></param>
        private void PrependWrapper(List<ArgBase> commandLine)
        {
            if (string.IsNullOrEmpty(Wrapper)) return;

            var wrapper = WindowsUtils.SplitArgs(Wrapper);
            commandLine.InsertRange(0, Array.ConvertAll(wrapper, arg => new Arg {Value = arg}));
        }

        /// <summary>
        /// Appends the user specified <paramref name="arguments"/> to the command-line.
        /// </summary>
        private static void AppendUserArgs(string[] arguments, List<ArgBase> commandLine)
        {
            commandLine.AddRange(Array.ConvertAll(arguments, arg => new Arg {Value = arg}));
        }

        /// <summary>
        /// Split and apply main command-line
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="startInfo"></param>
        private static void ApplyCommandLine(IEnumerable<ArgBase> commandLine, ProcessStartInfo startInfo)
        {
            var split = SplitCommandLine(ExpandCommandLine(commandLine, startInfo.EnvironmentVariables));
            startInfo.FileName = split.Path;
            startInfo.Arguments = split.Arguments;
        }

        /// <summary>
        /// Expands any Unix-style environment variables.
        /// </summary>
        /// <param name="commandLine">The command-line to expand.</param>
        /// <param name="environmentVariables">A list of environment variables available for expansion.</param>
        private static IList<string> ExpandCommandLine(IEnumerable<ArgBase> commandLine, StringDictionary environmentVariables)
        {
            var result = new List<string>();
            new PerTypeDispatcher<ArgBase>(ignoreMissing: false)
            {
                (Arg arg) => result.Add(StringUtils.ExpandUnixVariables(arg.Value, environmentVariables)),
                (ForEachArgs forEach) =>
                {
                    string valueToSplit = environmentVariables[forEach.ItemFrom];
                    if (!string.IsNullOrEmpty(valueToSplit))
                    {
                        string[] items = valueToSplit.Split(
                            new[] {forEach.Separator ?? Path.PathSeparator.ToString(CultureInfo.InvariantCulture)}, StringSplitOptions.None);
                        foreach (string item in items)
                        {
                            environmentVariables["item"] = item;
                            result.AddRange(forEach.Arguments.Select(arg => StringUtils.ExpandUnixVariables(arg.Value, environmentVariables)));
                        }
                        environmentVariables.Remove("item");
                    }
                }
            }.Dispatch(commandLine);
            return result;
        }

        /// <summary>
        /// Splits a command-line into a file name and an arguments part.
        /// </summary>
        /// <param name="commandLine">The command-line to split.</param>
        private static CommandLineSplit SplitCommandLine(IList<string> commandLine)
        {
            if (commandLine.Count == 0) throw new ExecutorException(Resources.CommandLineEmpty);

            // Split into file name...
            string fileName = commandLine[0];

            // ... and everything else
            var arguments = new string[commandLine.Count - 1];
            for (int i = 0; i < arguments.Length; i++)
                arguments[i] = commandLine[i + 1];

            return new CommandLineSplit(fileName, arguments.JoinEscapeArguments());
        }
    }
}
