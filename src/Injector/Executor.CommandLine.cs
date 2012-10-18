/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.IO;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    public partial class Executor
    {
        #region Structs
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
        #endregion

        #region Main
        /// <summary>
        /// Replaces the <see cref="Command"/> of the first <see cref="Implementation"/> with the binary specified in <see cref="Main"/>.
        /// </summary>
        private void ApplyMain(ref ImplementationSelection firstImplementation)
        {
            // Clone the first implementation so the command can replaced without affecting Selections
            firstImplementation = firstImplementation.CloneImplementation();
            var firstCommand = firstImplementation.Commands.First;

            string mainPath = FileUtils.UnifySlashes(Main);
            firstCommand.Path = (mainPath[0] == Path.DirectorySeparatorChar)
                // Relative to implementation root
                ? mainPath.TrimStart(Path.DirectorySeparatorChar)
                // Relative to original command
                : Path.Combine(Path.GetDirectoryName(firstCommand.Path) ?? "", mainPath);
            firstCommand.Arguments.Clear();
        }
        #endregion

        #region Get command-line
        /// <summary>
        /// Determines the command-line needed to execute an <see cref="ImplementationSelection"/>. Recursivley handles <see cref="Runner"/>s.
        /// </summary>
        /// <param name="implementation">The implementation to launch.</param>
        /// <param name="commandName">The name of the <see cref="Command"/> within the <paramref name="implementation"/> to launch. Will default to <see cref="Command.NameRun"/> if <see langword="null"/>.</param>
        /// <param name="startInfo">The process launch environment to apply additional <see cref="Binding"/> to.</param>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if an <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if a <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        private List<string> GetCommandLine(ImplementationSelection implementation, string commandName, ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            Command command = implementation.GetCommand(commandName ?? Command.NameRun);

            // Apply bindings implementations use to find themselves and their dependencies
            ApplyBindings(command, implementation, startInfo);
            if (command.WorkingDir != null) ApplyWorkingDir(command.WorkingDir, implementation, startInfo);
            ApplyDependencyBindings(command, startInfo);

            List<string> commandLine;
            var runner = command.Runner;
            if (runner == null) commandLine = new List<string>();
            else
            {
                commandLine = GetCommandLine(Selections[runner.Interface], null, startInfo);
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
        #endregion

        #region Split command-line
        /// <summary>
        /// Splits a command-line into a file name and an arguments part. Expands any Unix-style environment variables.
        /// </summary>
        /// <param name="commandLine">The command-line to split.</param>
        /// <param name="environmentVariables">A list of environment variables available for expansion.</param>
        private static CommandLineSplit SplitCommandLine(List<string> commandLine, StringDictionary environmentVariables)
        {
            // Split into file name...
            string fileName = StringUtils.ExpandUnixVariables(commandLine[0], environmentVariables);

            // ... and everything else
            var arguments = new string[commandLine.Count - 1];
            for (int i = 0; i < arguments.Length; i++)
                arguments[i] = StringUtils.ExpandUnixVariables(commandLine[i + 1], environmentVariables);

            return new CommandLineSplit(fileName, arguments.JoinEscapeArguments());
        }
        #endregion
    }
}
