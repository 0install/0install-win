/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Injector
{
    /// <summary>
    /// Executes a set of <see cref="Selections"/> as a program using dependency injection.
    /// </summary>
    public class Executor : IExecutor
    {
        #region Dependencies
        /// <summary>
        /// Used to locate the selected <see cref="Implementation"/>s.
        /// </summary>
        private readonly IStore _store;

        /// <summary>
        /// Creates a new executor.
        /// </summary>
        /// <param name="store">Used to locate the selected <see cref="Implementation"/>s.</param>
        public Executor([NotNull] IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException(nameof(store));
            #endregion

            _store = store;
        }
        #endregion

        /// <inheritdoc/>
        public Selections Selections { get; private set; }

        /// <inheritdoc/>
        public string Main { get; set; }

        /// <inheritdoc/>
        public string Wrapper { get; set; }

        /// <inheritdoc/>
        public Process Start(Selections selections, params string[] arguments)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException(nameof(selections));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            #endregion

            return GetStartInfo(selections, arguments).Start();
        }

        /// <inheritdoc/>
        public ProcessStartInfo GetStartInfo(Selections selections, params string[] arguments)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException(nameof(selections));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            #endregion

            if (string.IsNullOrEmpty(selections.Command)) throw new ExecutorException("The Selections document does not specify a start command.");
            if (selections.Implementations.Count == 0) throw new ExecutorException("The Selections document does not list any implementations.");
            Selections = selections;

            ProcessUtils.SanitizeEnvironmentVariables();

            try
            {
                var startInfo = BuildStartInfoWithBindings();
                var commandLine = GetCommandLine(GetMainImplementation(), Selections.Command, startInfo);
                PrependWrapper(commandLine);
                AppendUserArgs(arguments, commandLine);
                ProcessRunEnvBindings(startInfo);
                ApplyCommandLine(commandLine, startInfo);
                return startInfo;
            }
                #region Error handling
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new ExecutorException(ex.Message);
            }
            #endregion
        }

        /// <summary>
        /// Accumulates bindings in a process environment.
        /// </summary>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">An <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">A <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        [NotNull]
        private ProcessStartInfo BuildStartInfoWithBindings()
        {
            var startInfo = new ProcessStartInfo {ErrorDialog = false, UseShellExecute = false};
            foreach (var implementation in Selections.Implementations)
            {
                // Apply bindings implementations use to find themselves and their dependencies
                ApplyBindings(implementation, implementation, startInfo);
                ApplyDependencyBindings(implementation, startInfo);
            }
            _appliedBindingContainers.Clear();

            return startInfo;
        }

        private readonly HashSet<IBindingContainer> _appliedBindingContainers = new HashSet<IBindingContainer>();

        /// <summary>
        /// Applies all <see cref="Binding"/>s listed in a specific <see cref="IBindingContainer"/>.
        /// </summary>
        /// <param name="bindingContainer">The list of <see cref="Binding"/>s to be performed.</param>
        /// <param name="implementation">The implementation to be made available via the <see cref="Binding"/>s.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="Binding"/>s to.</param>
        /// <exception cref="ImplementationNotFoundException">The <paramref name="implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">A <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        private void ApplyBindings([NotNull] IBindingContainer bindingContainer, [NotNull] ImplementationSelection implementation, [NotNull] ProcessStartInfo startInfo)
        {
            // Do not apply bindings more than once
            if (!_appliedBindingContainers.Add(bindingContainer)) return;

            if (bindingContainer.Bindings.Count == 0) return;

            // Don't use bindings for PackageImplementations
            if (implementation.ID.StartsWith(ExternalImplementation.PackagePrefix)) return;

            new PerTypeDispatcher<Binding>(ignoreMissing: true)
            {
                (EnvironmentBinding environmentBinding) => ApplyEnvironmentBinding(environmentBinding, implementation, startInfo),
                //(OverlayBinding overlayBinding) => ApplyOverlayBinding(overlayBinding, implementation, startInfo),
                (ExecutableInVar executableInVar) => ApplyExecutableInVar(executableInVar, implementation, startInfo),
                (ExecutableInPath executableInPath) => ApplyExecutableInPath(executableInPath, implementation, startInfo)
            }.Dispatch(bindingContainer.Bindings);
        }

        /// <summary>
        /// Applies <see cref="Binding"/>s to make a set of <see cref="Dependency"/>s available.
        /// </summary>
        /// <param name="dependencyContainer">The list of <see cref="Dependency"/>s to follow.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="Binding"/>s to.</param>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">An <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">A <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        private void ApplyDependencyBindings([NotNull] IDependencyContainer dependencyContainer, [NotNull] ProcessStartInfo startInfo)
        {
            foreach (var dependency in dependencyContainer.Dependencies
                .Where(x => x.Importance == Importance.Essential || Selections.ContainsImplementation(x.InterfaceUri)))
                ApplyBindings(dependency, Selections[dependency.InterfaceUri], startInfo);
        }

        #region EnvironmentBinding
        /// <summary>
        /// Applies an <see cref="EnvironmentBinding"/> by modifying environment variables.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <param name="startInfo">The process launch environment to apply the binding to.</param>
        /// <exception cref="ExecutorException"><see cref="EnvironmentBinding.Name"/> or other data is invalid.</exception>
        private void ApplyEnvironmentBinding([NotNull] EnvironmentBinding binding, [NotNull] ImplementationSelection implementation, [NotNull] ProcessStartInfo startInfo)
        {
            Log.Debug("Applying " + binding + " for " + implementation);

            if (string.IsNullOrEmpty(binding.Name)) throw new ExecutorException(string.Format(Resources.MissingBindingName, @"<environment>"));

            var environmentVariables = startInfo.EnvironmentVariables;

            string newValue;
            if (binding.Value != null && binding.Insert != null)
            { // Conflict
                throw new ExecutorException(Resources.EnvironmentBindingValueInvalid);
            }
            else if (binding.Value != null)
            { // Static value
                newValue = binding.Value;
            }
            else
            { // Path inside the implementation
                newValue = Path.Combine(_store.GetPath(implementation), FileUtils.UnifySlashes(binding.Insert ?? ""));
            }

            // Set the default value if the variable is not already set on the system
            if (!environmentVariables.ContainsKey(binding.Name)) environmentVariables.Add(binding.Name, binding.Default);

            string previousValue = environmentVariables[binding.Name];
            string separator = (string.IsNullOrEmpty(binding.Separator) ? Path.PathSeparator.ToString(CultureInfo.InvariantCulture) : binding.Separator);

            switch (binding.Mode)
            {
                default:
                case EnvironmentMode.Prepend:
                    environmentVariables[binding.Name] = string.IsNullOrEmpty(previousValue)
                        // No exisiting value, just set new one
                        ? newValue
                        // Prepend new value to existing one seperated by path separator
                        : newValue + separator + environmentVariables[binding.Name];
                    break;

                case EnvironmentMode.Append:
                    environmentVariables[binding.Name] = string.IsNullOrEmpty(previousValue)
                        // No exisiting value, just set new one
                        ? newValue
                        // Append new value to existing one seperated by path separator
                        : environmentVariables[binding.Name] + separator + newValue;
                    break;

                case EnvironmentMode.Replace:
                    // Overwrite any existing value
                    environmentVariables[binding.Name] = newValue;
                    break;
            }
        }
        #endregion

        #region ExecutableBinding
        /// <summary>
        /// Represents a run-environment executable configuration pending to be applied via environment variables.
        /// </summary>
        private struct RunEnvPending
        {
            /// <summary>
            /// The executable's file name without any file ending.
            /// </summary>
            public readonly string ExeName;

            /// <summary>
            /// The command-line the executable should run.
            /// </summary>
            public readonly List<ArgBase> CommandLine;

            /// <param name="name">The executable's file name without any file ending.</param>
            /// <param name="commandLine">The command-line the executable should run.</param>
            public RunEnvPending(string name, List<ArgBase> commandLine)
            {
                ExeName = name;
                CommandLine = commandLine;
            }
        }

        /// <summary>
        /// A list of run-environment executables pending to be configured.
        /// </summary>
        private readonly List<RunEnvPending> _runEnvPendings = new List<RunEnvPending>();

        /// <summary>
        /// Applies an <see cref="ExecutableInVar"/> binding by creating a run-environment executable.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <param name="startInfo">The process launch environment to use to make the run-environment executable available.</param>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> points to missing <see cref="Dependency"/>s.</exception>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException"><see cref="ExecutableInVar.Name"/> is invalid.</exception>
        /// <exception cref="IOException">A problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        private void ApplyExecutableInVar([NotNull] ExecutableInVar binding, [NotNull] ImplementationSelection implementation, [NotNull] ProcessStartInfo startInfo)
        {
            Log.Debug("Applying " + binding + " for " + implementation);

            if (string.IsNullOrEmpty(binding.Name)) throw new ExecutorException(string.Format(Resources.MissingBindingName, @"<executable-in-var>"));
            if (Path.GetInvalidFileNameChars().Any(invalidChar => binding.Name.Contains(invalidChar.ToString(CultureInfo.InvariantCulture))))
                throw new ExecutorException(string.Format(Resources.IllegalCharInBindingName, @"<executable-in-var>"));

            string exePath = DeployRunEnvExecutable(binding.Name);

            // Point variable directly to executable
            if (startInfo.EnvironmentVariables.ContainsKey(binding.Name)) Log.Warn("Overwriting existing environment variable with <executable-in-var>: " + binding.Name);
            startInfo.EnvironmentVariables[binding.Name] = exePath;

            // Tell the executable what command-line to run
            _runEnvPendings.Add(new RunEnvPending(binding.Name, GetCommandLine(implementation, binding.Command ?? Command.NameRun, startInfo)));
        }

        /// <summary>
        /// Applies an <see cref="ExecutableInPath"/> binding by creating a run-environment executable.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <param name="startInfo">The process launch environment to use to make the run-environment executable available.</param>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> points to missing <see cref="Dependency"/>s.</exception>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException"><see cref="ExecutableInPath.Name"/> is invalid.</exception>
        /// <exception cref="IOException">A problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        private void ApplyExecutableInPath([NotNull] ExecutableInPath binding, [NotNull] ImplementationSelection implementation, [NotNull] ProcessStartInfo startInfo)
        {
            Log.Debug("Applying " + binding + " for " + implementation);

            if (string.IsNullOrEmpty(binding.Name)) throw new ExecutorException(string.Format(Resources.MissingBindingName, @"<executable-in-path>"));
            if (Path.GetInvalidFileNameChars().Any(invalidChar => binding.Name.Contains(invalidChar.ToString(CultureInfo.InvariantCulture))))
                throw new ExecutorException(string.Format(Resources.IllegalCharInBindingName, @"<executable-in-path>"));

            string exePath = DeployRunEnvExecutable(binding.Name);

            // Add executable directory to PATH variable
            startInfo.EnvironmentVariables[WindowsUtils.IsWindows ? "Path" : "PATH"] =
                Path.GetDirectoryName(exePath) + Path.PathSeparator +
                startInfo.EnvironmentVariables[WindowsUtils.IsWindows ? "Path" : "PATH"];

            // Tell the executable what command-line to run
            _runEnvPendings.Add(new RunEnvPending(binding.Name, GetCommandLine(implementation, binding.Command ?? Command.NameRun, startInfo)));
        }

        /// <summary>
        /// Deploys a copy (hard or soft link if possible) of the run-environment executable within a cache directory.
        /// </summary>
        /// <param name="name">The executable name to deploy under (without file extensions).</param>
        /// <returns>The fully qualified path of the deployed run-environment executable.</returns>
        /// <exception cref="IOException">A problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        /// <remarks>A run-environment executable executes a command-line specified in an environment variable based on its own name.</remarks>
        [NotNull]
        private static string DeployRunEnvExecutable([NotNull] string name)
        {
            string templatePath = GetRunEnvTemplate();
            string deployedPath = Path.Combine(Locations.GetCacheDirPath("0install.net", false, "injector", "executables", name), name);
            if (WindowsUtils.IsWindows) deployedPath += ".exe";

            Log.Info("Deploying run-environment executable to: " + deployedPath);
            try
            {
                if (File.Exists(deployedPath)) File.Delete(deployedPath);
                FileUtils.CreateHardlink(deployedPath, templatePath);
            }
            #region Error handling
            catch (IOException) when (File.Exists(deployedPath))
            {
                // File already exists and was probably locked because it was in use
            }
            catch (UnauthorizedAccessException) when (File.Exists(deployedPath))
            {
                // File already exists and was probably locked because it was in use
            }
            catch (PlatformNotSupportedException)
            {
                // Unable to hardlink, fallback to simple copy
                File.Copy(templatePath, deployedPath);
            }
            #endregion

            if (UnixUtils.IsUnix) FileUtils.SetExecutable(deployedPath, true);

            return deployedPath;
        }

        /// <summary>
        /// Deploys an appropriate runenv binary template for the current operating system.
        /// </summary>
        /// <returns>The path to the deployed executable file.</returns>
        [NotNull]
        private static string GetRunEnvTemplate()
        {
            var templateName = WindowsUtils.IsWindows
                ? (Environment.Version.Major == 4)
                    ? "runenv.clr4.template"
                    : "runenv.clr2.template"
                : "runenv.sh.template";

            string path = Path.Combine(Locations.GetCacheDirPath("0install.net", false, "injector", "executables"), templateName);
            Log.Info("Writing run-environment template to: " + path);
            try
            {
                typeof(Executor).CopyEmbeddedToFile(templateName, path);
            }
            #region Error handling
            catch (IOException ex)
            {
                Log.Info("Unable to write run-environment template.");
                Log.Info(ex);
            }
            #endregion

            return path;
        }

        /// <summary>
        /// Split and apply command-lines for executable bindings.
        /// This is delayed until the end because environment variables that might be modified are expanded.
        /// </summary>
        private void ProcessRunEnvBindings([NotNull] ProcessStartInfo startInfo)
        {
            foreach (var runEnv in _runEnvPendings)
            {
                var commandLine = ExpandCommandLine(runEnv.CommandLine, startInfo.EnvironmentVariables);
                if (WindowsUtils.IsWindows)
                {
                    var split = SplitCommandLine(commandLine);
                    startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_FILE_" + runEnv.ExeName] = split.Path;
                    startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_ARGS_" + runEnv.ExeName] = split.Arguments;
                }
                else startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_" + runEnv.ExeName] = commandLine.JoinEscapeArguments();
            }
            _runEnvPendings.Clear();

            if (WindowsUtils.IsWindows)
            {
                // Download implementations with .NET code even if a Python-version of Zero Install is executed
                startInfo.EnvironmentVariables["ZEROINSTALL_EXTERNAL_FETCHER"] = new[] {Path.Combine(Locations.InstallBase, "0install.exe"), "fetch"}.JoinEscapeArguments();

                // Extracted archvies with .NET code even if a Python-version of Zero Install is executed
                startInfo.EnvironmentVariables["ZEROINSTALL_EXTERNAL_STORE"] = Path.Combine(Locations.InstallBase, "0store.exe");
            }
        }
        #endregion

        #region WorkingDir
        /// <summary>
        /// Applies a <see cref="WorkingDir"/> change to the <see cref="ProcessStartInfo"/>.
        /// </summary>
        /// <param name="binding">The <see cref="WorkingDir"/> to apply.</param>
        /// <param name="implementation">The implementation to be made available via the <see cref="WorkingDir"/> change.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="WorkingDir"/> change to.</param>
        /// <exception cref="ImplementationNotFoundException">The <paramref name="implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <paramref name="binding"/> has an invalid path or another working directory has already been set.</exception>
        /// <remarks>This method can only be called successfully once per <see cref="BuildStartInfoWithBindings()"/>.</remarks>
        private void ApplyWorkingDir([NotNull] WorkingDir binding, [NotNull] ImplementationSelection implementation, [NotNull] ProcessStartInfo startInfo)
        {
            Log.Debug("Applying " + binding + " for " + implementation);

            string source = FileUtils.UnifySlashes(binding.Source) ?? "";
            if (Path.IsPathRooted(source) || source.Contains(".." + Path.DirectorySeparatorChar)) throw new ExecutorException(Resources.WorkingDirInvalidPath);

            // Only allow working directory to be changed once
            if (!string.IsNullOrEmpty(startInfo.WorkingDirectory)) throw new ExecutorException(Resources.Working);

            startInfo.WorkingDirectory = Path.Combine(_store.GetPath(implementation), source);
        }
        #endregion

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
        [NotNull]
        private ImplementationSelection GetMainImplementation()
        {
            if (string.IsNullOrEmpty(Main)) return Selections.MainImplementation;

            // Clone the first implementation so the command can replaced without affecting Selections
            var mainImplementation = Selections.MainImplementation.CloneImplementation();
            var command = mainImplementation[Selections.Command];
            Debug.Assert(command != null);

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
        [NotNull, ItemNotNull]
        private List<ArgBase> GetCommandLine([NotNull] ImplementationSelection implementation, [NotNull] string commandName, [NotNull] ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            if (startInfo == null) throw new ArgumentNullException(nameof(startInfo));
            if (commandName == null) throw new ArgumentNullException(nameof(commandName));
            #endregion

            if (commandName.Length == 0) throw new ExecutorException(string.Format(Resources.CommandNotSpecified, implementation.InterfaceUri));
            var command = implementation[commandName];
            Debug.Assert(command != null);

            // Apply bindings implementations use to find themselves and their dependencies
            ApplyBindings(command, implementation, startInfo);
            if (command.WorkingDir != null) ApplyWorkingDir(command.WorkingDir, implementation, startInfo);
            ApplyDependencyBindings(command, startInfo);

            List<ArgBase> commandLine;
            var runner = command.Runner;
            if (runner == null) commandLine = new List<ArgBase>();
            else
            {
                commandLine = GetCommandLine(Selections[runner.InterfaceUri], runner.Command ?? Command.NameRun, startInfo);
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
        /// Prepends the user-specified <see cref="Wrapper"/>, if any, to the command-line.
        /// </summary>
        /// <param name="commandLine"></param>
        private void PrependWrapper([NotNull, ItemNotNull] List<ArgBase> commandLine)
        {
            if (string.IsNullOrEmpty(Wrapper)) return;

            var wrapper = WindowsUtils.SplitArgs(Wrapper);
            commandLine.InsertRange(0, Array.ConvertAll(wrapper, arg => new Arg {Value = arg}));
        }

        /// <summary>
        /// Appends the user specified <paramref name="arguments"/> to the command-line.
        /// </summary>
        private static void AppendUserArgs([NotNull, ItemNotNull] string[] arguments, [NotNull, ItemNotNull] List<ArgBase> commandLine)
        {
            commandLine.AddRange(Array.ConvertAll(arguments, arg => new Arg {Value = arg}));
        }

        /// <summary>
        /// Split and apply main command-line
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="startInfo"></param>
        private static void ApplyCommandLine([NotNull, ItemNotNull] IEnumerable<ArgBase> commandLine, [NotNull] ProcessStartInfo startInfo)
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
        [NotNull, ItemNotNull]
        private static IList<string> ExpandCommandLine([NotNull, ItemNotNull] IEnumerable<ArgBase> commandLine, [NotNull] StringDictionary environmentVariables)
        {
            var result = new List<string>();
            new PerTypeDispatcher<ArgBase>(ignoreMissing: false)
            {
                (Arg arg) => result.Add(FileUtils.ExpandUnixVariables(arg.Value, environmentVariables)),
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
                            result.AddRange(forEach.Arguments.Select(arg => FileUtils.ExpandUnixVariables(arg.Value, environmentVariables)));
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
