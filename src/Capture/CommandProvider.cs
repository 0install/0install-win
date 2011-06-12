/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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
using System.Text;
using Common.Utils;
using ZeroInstall.Model;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Maps command-lines to the best matching <see cref="Command"/>.
    /// </summary>
    public class CommandProvider
    {
        #region Private structs
        /// <summary>
        /// An association of a command-line with a <see cref="Model.Command"/>.
        /// </summary>
        private struct CommandTuple
        {
            /// <summary>The full command-line rooted in the filesystem.</summary>
            public readonly string CommandLine;

            /// <summary>The command coressponding to the execution of the command-line.</summary>
            public readonly Command Command;

            /// <summary>
            /// Creates a new command tuple.
            /// </summary>
            /// <param name="commandLine">The full command-line rooted in the filesystem.</param>
            /// <param name="command">The command coressponding to the execution of the command-line.</param>
            public CommandTuple(string commandLine, Command command)
            {
                CommandLine = commandLine;
                Command = command;
            }
        }
        #endregion

        #region Variables
        private readonly string _installationDir;

        /// <summary>A list of command-lines and coressponding <see cref="Command"/>s.</summary>
        private readonly List<CommandTuple> _commmands = new List<CommandTuple>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command provider.
        /// </summary>
        /// <param name="installationDir">The fully qualified path to the installation directory.</param>
        /// <param name="commmands">A list of all known-commands available within the installation directory.</param>
        public CommandProvider(string installationDir, IEnumerable<Command> commmands)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(installationDir)) throw new ArgumentNullException("installationDir");
            if (commmands == null) throw new ArgumentNullException("commmands");
            #endregion

            _installationDir = installationDir;

            // Associate each command with its command-line
            foreach (var commmand in commmands)
                _commmands.Add(new CommandTuple(GetCommandLine(installationDir, commmand), commmand));

            // Sort backwards to make sure the most specific matches are selected first
            _commmands.Sort((tuple1, tuple2) => tuple2.CommandLine.CompareTo(tuple2.CommandLine));
        }

        private static string GetCommandLine(string installationDir, Command command)
        {
            var builder = new StringBuilder(StringUtils.EscapeWhitespace(Path.Combine(installationDir, command.Path)));
            command.Arguments.Apply(args => builder.Append(" " + StringUtils.EscapeWhitespace(args)));
            return builder.ToString();
        }
        #endregion

        //--------------------//

        /// <summary>
        /// Tries to find the best-match <see cref="Command"/> for a command-line.
        /// </summary>
        /// <param name="commandLine">The fully qualified command-line to try to match.</param>
        /// <param name="additionalArgs">Any additional arguments from <paramref name="commandLine"/> that are not covered by the returned <see cref="Command"/>.</param>
        /// <returns>The best matching <see cref="Command"/> or <see langword="null"/> if no match was found.</returns>
        public Command GetCommand(string commandLine, out string additionalArgs)
        {
            #region Sanity checks
            if (commandLine == null) throw new ArgumentNullException("commandLine");
            #endregion

            foreach (var tuple in _commmands)
            {
                // Find partial matches
                if (commandLine.StartsWith(tuple.CommandLine))
                {
                    additionalArgs = commandLine.Substring(tuple.CommandLine.Length).TrimStart();
                    return tuple.Command;
                }

                // Find exact matches
                if (commandLine == Path.Combine(_installationDir, tuple.Command.Path))
                {
                    additionalArgs = "";
                    return tuple.Command;
                }
            }

            // No match found
            additionalArgs = null;
            return null;
        }
    }
}
