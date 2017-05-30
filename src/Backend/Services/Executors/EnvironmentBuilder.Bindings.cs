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
using NanoByte.Common.Collections;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Executors
{
    public partial class EnvironmentBuilder
    {
        /// <summary>
        /// Used to track <see cref="IBindingContainer"/> that have already been applied to avoid cycles.
        /// </summary>
        private readonly HashSet<IBindingContainer> _appliedBindingContainers = new HashSet<IBindingContainer>();

        /// <summary>
        /// Applies all specified <see cref="Binding"/>s.
        /// </summary>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">An <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">A <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        private void ApplyBindings()
        {
            foreach (var implementation in _selections.Implementations)
            {
                // Self-bindings used by implementations to find their own resources
                ApplyBindings(implementation, implementation);

                ApplyDependencyBindings(implementation);
            }

            // Allow bindings to be reapplied for runners, etc.
            _appliedBindingContainers.Clear();
        }

        /// <summary>
        /// Applies <see cref="Binding"/>s to make a set of <see cref="Dependency"/>s available.
        /// </summary>
        /// <param name="dependencyContainer">The list of <see cref="Dependency"/>s to follow.</param>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">An <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">A <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        private void ApplyDependencyBindings([NotNull] IDependencyContainer dependencyContainer)
        {
            foreach (var dependency in dependencyContainer.Dependencies
                .Where(x => x.Importance == Importance.Essential || _selections.ContainsImplementation(x.InterfaceUri)))
                ApplyBindings(dependency, _selections[dependency.InterfaceUri]);
        }

        /// <summary>
        /// Applies all <see cref="Binding"/>s listed in a specific <see cref="IBindingContainer"/>.
        /// </summary>
        /// <param name="bindingContainer">The list of <see cref="Binding"/>s to be performed.</param>
        /// <param name="implementation">The implementation to be made available via the <see cref="Binding"/>s.</param>
        /// <exception cref="ImplementationNotFoundException">The <paramref name="implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">A <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        private void ApplyBindings([NotNull] IBindingContainer bindingContainer, [NotNull] ImplementationSelection implementation)
        {
            // Do not apply bindings more than once
            if (!_appliedBindingContainers.Add(bindingContainer)) return;

            if (bindingContainer.Bindings.Count == 0) return;

            // Don't use bindings for PackageImplementations
            if (implementation.ID.StartsWith(ExternalImplementation.PackagePrefix)) return;

            new PerTypeDispatcher<Binding>(ignoreMissing: true)
            {
                (EnvironmentBinding environmentBinding) => ApplyEnvironmentBinding(environmentBinding, implementation),
                //(OverlayBinding overlayBinding) => ApplyOverlayBinding(overlayBinding, implementation),
                (ExecutableInVar executableInVar) => ApplyExecutableInVar(executableInVar, implementation),
                (ExecutableInPath executableInPath) => ApplyExecutableInPath(executableInPath, implementation)
            }.Dispatch(bindingContainer.Bindings);
        }

        /// <summary>
        /// Applies an <see cref="EnvironmentBinding"/> by modifying environment variables.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <exception cref="ExecutorException"><see cref="EnvironmentBinding.Name"/> or other data is invalid.</exception>
        private void ApplyEnvironmentBinding([NotNull] EnvironmentBinding binding, [NotNull] ImplementationSelection implementation)
        {
            Log.Debug("Applying " + binding + " for " + implementation);

            if (string.IsNullOrEmpty(binding.Name)) throw new ExecutorException(string.Format(Resources.MissingBindingName, @"<environment>"));

            var environmentVariables = _startInfo.EnvironmentVariables;

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
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> points to missing <see cref="Dependency"/>s.</exception>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException"><see cref="ExecutableInVar.Name"/> is invalid.</exception>
        /// <exception cref="IOException">A problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        private void ApplyExecutableInVar([NotNull] ExecutableInVar binding, [NotNull] ImplementationSelection implementation)
        {
            Log.Debug("Applying " + binding + " for " + implementation);

            if (string.IsNullOrEmpty(binding.Name)) throw new ExecutorException(string.Format(Resources.MissingBindingName, @"<executable-in-var>"));
            if (Path.GetInvalidFileNameChars().Any(invalidChar => binding.Name.Contains(invalidChar.ToString(CultureInfo.InvariantCulture))))
                throw new ExecutorException(string.Format(Resources.IllegalCharInBindingName, @"<executable-in-var>"));

            string exePath = DeployRunEnvExecutable(binding.Name);

            // Point variable directly to executable
            if (_startInfo.EnvironmentVariables.ContainsKey(binding.Name)) Log.Warn("Overwriting existing environment variable with <executable-in-var>: " + binding.Name);
            _startInfo.EnvironmentVariables[binding.Name] = exePath;

            // Tell the executable what command-line to run
            _runEnvPendings.Add(new RunEnvPending(binding.Name, GetCommandLine(implementation, binding.Command ?? Command.NameRun)));
        }

        /// <summary>
        /// Applies an <see cref="ExecutableInPath"/> binding by creating a run-environment executable.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> points to missing <see cref="Dependency"/>s.</exception>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException"><see cref="ExecutableInPath.Name"/> is invalid.</exception>
        /// <exception cref="IOException">A problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        private void ApplyExecutableInPath([NotNull] ExecutableInPath binding, [NotNull] ImplementationSelection implementation)
        {
            Log.Debug("Applying " + binding + " for " + implementation);

            if (string.IsNullOrEmpty(binding.Name)) throw new ExecutorException(string.Format(Resources.MissingBindingName, @"<executable-in-path>"));
            if (Path.GetInvalidFileNameChars().Any(invalidChar => binding.Name.Contains(invalidChar.ToString(CultureInfo.InvariantCulture))))
                throw new ExecutorException(string.Format(Resources.IllegalCharInBindingName, @"<executable-in-path>"));

            string exePath = DeployRunEnvExecutable(binding.Name);

            // Add executable directory to PATH variable
            _startInfo.EnvironmentVariables[WindowsUtils.IsWindows ? "Path" : "PATH"] =
                Path.GetDirectoryName(exePath) + Path.PathSeparator +
                _startInfo.EnvironmentVariables[WindowsUtils.IsWindows ? "Path" : "PATH"];

            // Tell the executable what command-line to run
            _runEnvPendings.Add(new RunEnvPending(binding.Name, GetCommandLine(implementation, binding.Command ?? Command.NameRun)));
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
        private void ProcessRunEnvBindings()
        {
            foreach (var runEnv in _runEnvPendings)
            {
                var commandLine = ExpandCommandLine(runEnv.CommandLine);
                if (WindowsUtils.IsWindows)
                {
                    var split = SplitCommandLine(commandLine);
                    _startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_FILE_" + runEnv.ExeName] = split.Path;
                    _startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_ARGS_" + runEnv.ExeName] = split.Arguments;
                }
                else _startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_" + runEnv.ExeName] = commandLine.JoinEscapeArguments();
            }
            _runEnvPendings.Clear();

            try
            {
                // Allow Python/OCaml-version of Zero Install and other tools to call back to the Fetcher
                var fetchCommand = ProcessUtils.Assembly("0install", "fetch");
                _startInfo.EnvironmentVariables["ZEROINSTALL_EXTERNAL_FETCHER"] = fetchCommand.FileName.EscapeArgument() + " " + fetchCommand.Arguments;
            }
            catch (FileNotFoundException)
            {
                // Zero Install may be embedded as a library rather than an executable
            }
        }

        /// <summary>
        /// Applies a <see cref="WorkingDir"/> change to the <see cref="ProcessStartInfo"/>.
        /// </summary>
        /// <param name="binding">The <see cref="WorkingDir"/> to apply.</param>
        /// <param name="implementation">The implementation to be made available via the <see cref="WorkingDir"/> change.</param>
        /// <exception cref="ImplementationNotFoundException">The <paramref name="implementation"/> is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <paramref name="binding"/> has an invalid path or another working directory has already been set.</exception>
        private void ApplyWorkingDir([NotNull] WorkingDir binding, [NotNull] ImplementationSelection implementation)
        {
            Log.Debug("Applying " + binding + " for " + implementation);

            string source = FileUtils.UnifySlashes(binding.Source) ?? "";
            if (Path.IsPathRooted(source) || source.Contains(".." + Path.DirectorySeparatorChar)) throw new ExecutorException(Resources.WorkingDirInvalidPath);

            // Only allow working directory to be changed once
            if (!string.IsNullOrEmpty(_startInfo.WorkingDirectory)) throw new ExecutorException(Resources.Working);

            _startInfo.WorkingDirectory = Path.Combine(_store.GetPath(implementation), source);
        }
    }
}
