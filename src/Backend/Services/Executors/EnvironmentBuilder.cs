/*
 * Copyright 2010-2017 Bastian Eicher
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Executors
{
    /// <summary>
    /// Fluent-style builder for a process execution environment for a <see cref="Selections"/> document.
    /// </summary>
    public partial class EnvironmentBuilder : IEnvironmentBuilder
    {
        #region Dependencies
        /// <summary>
        /// Used to locate <see cref="Implementation"/>s.
        /// </summary>
        [NotNull]
        private readonly IStore _store;

        /// <summary>
        /// Creates a new build.
        /// </summary>
        /// <param name="store">Used to locate <see cref="Implementation"/>s.</param>
        public EnvironmentBuilder([NotNull] IStore store)
            => _store = store ?? throw new ArgumentNullException(nameof(store));
        #endregion

        /// <summary>
        /// Used to hold the process launch environment while it is being built.
        /// </summary>
        private readonly ProcessStartInfo _startInfo = new ProcessStartInfo {ErrorDialog = false, UseShellExecute = false};

        /// <summary>
        /// Used to hold the command-line of the main implementation while it is being built.
        /// </summary>
        private List<ArgBase> _mainCommandLine;

        /// <summary>
        /// The set of <see cref="Implementation"/>s be injected into the execution environment.
        /// </summary>
        private Selections _selections;

        /// <summary>
        /// Adds an environment variable to the execution environment.
        /// May not be called after <see cref="Inject"/> has been called.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <param name="value">The value to set the environment variable to.</param>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The executor was unable to process the <see cref="Selections"/>.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        /// <returns>The execution environment. Reference to self for fluent API use.</returns>
        /// <remarks>Must be called before using other methods on the object. May not be called more than once.</remarks>
        public void SetEnvironmentVariable([NotNull] string key, [NotNull] string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (_selections != null) throw new InvalidOperationException($"Environment variables cannot be added after {nameof(Inject)}() has been called.");
            #endregion

            _startInfo.EnvironmentVariables[key] = value;
        }

        /// <summary>
        /// Sets the <see cref="Selections"/> to be injected.
        /// Must be called before any methods of the <see cref="IEnvironmentBuilder"/> interface are used. May not be called more than once.
        /// </summary>
        /// <param name="selections">The set of <see cref="Implementation"/>s be injected into the execution environment.</param>
        /// <param name="overrideMain">An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="Element.Main"/>. May not contain command-line arguments! Whitespaces do not need to be escaped.</param>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The executor was unable to process the <see cref="Selections"/>.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        /// <returns>The execution environment. Reference to self for fluent API use.</returns>
        public IEnvironmentBuilder Inject([NotNull] Selections selections, [CanBeNull] string overrideMain = null)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException(nameof(selections));
            if (_selections != null) throw new InvalidOperationException($"{nameof(Inject)}() may not be called more than once.");
            #endregion

            if (string.IsNullOrEmpty(selections.Command)) throw new ExecutorException("The Selections document does not specify a start command.");
            if (selections.Implementations.Count == 0) throw new ExecutorException("The Selections document does not list any implementations.");
            _selections = selections;

            try
            {
                ApplyBindings();

                _mainCommandLine = GetCommandLine(GetMainImplementation(overrideMain), _selections.Command);
            }
                #region Error handling
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new ExecutorException(ex.Message);
            }
            #endregion

            return this;
        }

        /// <inheritdoc/>
        public IEnvironmentBuilder AddWrapper(string wrapper)
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException($"{nameof(Inject)}() must be called first.");
            #endregion

            if (!string.IsNullOrEmpty(wrapper))
                _mainCommandLine.InsertRange(0, Array.ConvertAll(WindowsUtils.SplitArgs(wrapper), x => new Arg {Value = x}));

            return this;
        }

        /// <inheritdoc/>
        public IEnvironmentBuilder AddArguments(params string[] arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (_selections == null) throw new InvalidOperationException($"{nameof(Inject)}() must be called first.");
            #endregion

            _mainCommandLine.AddRange(Array.ConvertAll(arguments, x => new Arg {Value = x}));

            return this;
        }

        /// <inheritdoc/>
        public ProcessStartInfo ToStartInfo()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException($"{nameof(Inject)}() must be called first.");
            #endregion

            try
            {
                ProcessRunEnvBindings();

                var split = SplitCommandLine(ExpandCommandLine(_mainCommandLine));
                _startInfo.FileName = split.Path;
                _startInfo.Arguments = split.Arguments;
            }
                #region Error handling
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new ExecutorException(ex.Message);
            }
            #endregion

            return _startInfo;
        }

        /// <inheritdoc/>
        public Process Start() => ToStartInfo().Start();

        /// <summary>
        /// Returns the main (first) implementation of the selection.
        /// Replaces the <see cref="Command"/> of the main implementation with the binary specified in <paramref name="overrideMain"/> if set.
        /// </summary>
        /// <param name="overrideMain">An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="Element.Main"/>. May not contain command-line arguments! Whitespaces do not need to be escaped.</param>
        [NotNull]
        private ImplementationSelection GetMainImplementation([CanBeNull] string overrideMain)
        {
            if (string.IsNullOrEmpty(overrideMain)) return _selections.MainImplementation;

            // Clone the first implementation so the command can replaced without affecting Selections
            var mainImplementation = _selections.MainImplementation.CloneImplementation();
            var command = mainImplementation[_selections.Command];
            Debug.Assert(command != null);

            string mainPath = FileUtils.UnifySlashes(overrideMain);
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
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> points to missing <see cref="Dependency"/>s.</exception>
        /// <exception cref="ImplementationNotFoundException">An <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">A <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        [NotNull, ItemNotNull]
        private List<ArgBase> GetCommandLine([NotNull] ImplementationSelection implementation, [NotNull] string commandName)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            if (commandName == null) throw new ArgumentNullException(nameof(commandName));
            #endregion

            if (commandName.Length == 0) throw new ExecutorException(string.Format(Resources.CommandNotSpecified, implementation.InterfaceUri));
            var command = implementation[commandName];
            Debug.Assert(command != null);

            // Apply bindings implementations use to find themselves and their dependencies
            ApplyBindings(command, implementation);
            if (command.WorkingDir != null) ApplyWorkingDir(command.WorkingDir, implementation);
            ApplyDependencyBindings(command);

            List<ArgBase> commandLine;
            var runner = command.Runner;
            if (runner == null) commandLine = new List<ArgBase>();
            else
            {
                commandLine = GetCommandLine(_selections[runner.InterfaceUri], runner.Command ?? Command.NameRun);
                commandLine.AddRange(runner.Arguments);
            }

            if (!string.IsNullOrEmpty(command.Path))
            {
                string path = FileUtils.UnifySlashes(command.Path);

                // Fully qualified paths are used by package/native implementations, usually relative to the implementation
                commandLine.Add(Path.IsPathRooted(path) ? path : Path.Combine(_store.GetPath(implementation), path));
            }
            commandLine.AddRange(command.Arguments);

            return commandLine;
        }

        /// <summary>
        /// Expands any Unix-style environment variables.
        /// </summary>
        /// <param name="commandLine">The command-line to expand.</param>
        [NotNull, ItemNotNull]
        private IList<string> ExpandCommandLine([NotNull, ItemNotNull] IEnumerable<ArgBase> commandLine)
        {
            var result = new List<string>();

            foreach (var part in commandLine)
            {
                switch (part)
                {
                    case Arg arg:
                        result.Add(FileUtils.ExpandUnixVariables(arg.Value, _startInfo.EnvironmentVariables));
                        break;

                    case ForEachArgs forEach:
                        string valueToSplit = _startInfo.EnvironmentVariables[forEach.ItemFrom];
                        if (!string.IsNullOrEmpty(valueToSplit))
                        {
                            string[] items = valueToSplit.Split(
                                new[] {forEach.Separator ?? Path.PathSeparator.ToString(CultureInfo.InvariantCulture)}, StringSplitOptions.None);
                            foreach (string item in items)
                            {
                                _startInfo.EnvironmentVariables["item"] = item;
                                result.AddRange(forEach.Arguments.Select(arg => FileUtils.ExpandUnixVariables(arg.Value, _startInfo.EnvironmentVariables)));
                            }
                            _startInfo.EnvironmentVariables.Remove("item");
                        }
                        break;

                    default:
                        throw new NotSupportedException($"Unknown command-line part: {part}");
                }
            }

            return result;
        }

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
        /// Splits a command-line into a file name and an arguments part.
        /// </summary>
        /// <param name="commandLine">The command-line to split.</param>
        private static CommandLineSplit SplitCommandLine([NotNull, ItemNotNull] IList<string> commandLine)
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
