/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Utils;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Executes a set of <see cref="ImplementationSelection"/>s as a program using dependency injection.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class Executor
    {
        #region Properties
        /// <summary>
        /// The specific <see cref="Model.Implementation"/>s chosen for the <see cref="Dependency"/>s.
        /// </summary>
        public Selections Selections { get; private set; }

        /// <summary>
        /// Used to locate the selected <see cref="Model.Implementation"/>s.
        /// </summary>
        private IStore Store { get; set; }

        /// <summary>
        /// An alternative executable to to run from the main <see cref="Model.Implementation"/> instead of <see cref="Element.Main"/>. May not contain command-line arguments! Whitespaces do not need to be escaped.
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        /// Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers. May contain command-line arguments. Whitespaces must be escaped!
        /// </summary>
        public string Wrapper { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new launcher from <see cref="Selections"/>.
        /// </summary>
        /// <param name="selections">The specific <see cref="ImplementationSelection"/>s chosen for the <see cref="Dependency"/>s.</param>
        /// <param name="store">Used to locate the selected <see cref="Model.Implementation"/>s.</param>
        public Executor(Selections selections, IStore store)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (store == null) throw new ArgumentNullException("store");
            if (selections.Implementations.IsEmpty) throw new ArgumentException(Resources.NoImplementationsPassed, "selections");
            #endregion

            Selections = selections.CloneSelections();
            Store = store;
        }
        #endregion

        //--------------------//

        #region Start process
        /// <summary>
        /// Prepares a <see cref="ProcessStartInfo"/> for executing the program as specified by the <see cref="Selections"/>.
        /// </summary>
        /// <param name="arguments">Arguments to be passed to the launched programs.</param>
        /// <returns>The <see cref="ProcessStartInfo"/> that can be used to start the new <see cref="Process"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        public ProcessStartInfo GetStartInfo(params string[] arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            // Accumulate bindings in process environment
            var startInfo = new ProcessStartInfo {ErrorDialog = false, UseShellExecute = false};
            foreach (var implementation in Selections.Implementations)
            {
                // Apply bindings implementations use to find themselves and their dependencies
                ApplyBindings(implementation, implementation, startInfo);
                ApplyDependencyBindings(implementation, startInfo);
            }

            var firstImplementation = Selections.Implementations.First;
            if (!string.IsNullOrEmpty(Main)) ApplyMain(ref firstImplementation);

            // Recursivley build command-line (applying additional bindings)
            var commandLine = GetCommandLine(firstImplementation, startInfo);
            if (!string.IsNullOrEmpty(Wrapper)) commandLine.InsertAll(0, WindowsUtils.SplitArgs(Wrapper)); // Add wrapper in front
            commandLine.AddAll(arguments); // Append user arguments
            ApplyCommandLine(startInfo, commandLine);

            return startInfo;
        }

        /// <summary>
        /// Starts the program as specified by the <see cref="Selections"/>.
        /// </summary>
        /// <param name="arguments">Arguments to be passed to the launched programs.</param>
        /// <returns>The newly created <see cref="Process"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        public Process Start(params string[] arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            return Process.Start(GetStartInfo(arguments));
        }
        #endregion

        #region Bindings
        /// <summary>
        /// Applies all <see cref="Binding"/>s listed in a specific <see cref="IBindingContainer"/>.
        /// </summary>
        /// <param name="bindingContainer">The list of <see cref="Binding"/>s to be performed.</param>
        /// <param name="implementation">The implementation to be made available via the <see cref="Binding"/>s.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="Binding"/>s to.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        private void ApplyBindings(IBindingContainer bindingContainer, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            if (bindingContainer.Bindings.IsEmpty) return;

            // Don't use bindings for PackageImplementations
            if (!string.IsNullOrEmpty(implementation.Package)) return;

            string implementationDirectory = GetImplementationPath(implementation);

            foreach (var binding in bindingContainer.Bindings)
            {
                var environmentBinding = binding as EnvironmentBinding;
                if (environmentBinding != null) ApplyEnvironmentBinding(environmentBinding, implementationDirectory, startInfo);
                //else
                //{
                //    var overlayBinding = binding as OverlayBinding;
                //    if (overlayBinding != null) ApplyOverlayBinding(startInfo, implementationDirectory, overlayBinding);
                //}
            }
        }

        /// <summary>
        /// Follows a list of <see cref="Dependency"/>s and applies their <see cref="Binding"/>s.
        /// </summary>
        /// <param name="dependencyContainer">The list of <see cref="Dependency"/>s to follow.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="Binding"/>s to.</param>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if an <see cref="Implementation"/> is not cached yet.</exception>
        private void ApplyDependencyBindings(IDependencyContainer dependencyContainer, ProcessStartInfo startInfo)
        {
            foreach (var dependency in dependencyContainer.Dependencies)
                ApplyBindings(dependency, Selections[dependency.Interface], startInfo);
        }

        /// <summary>
        /// Applies an <see cref="EnvironmentBinding"/> to the <see cref="ProcessStartInfo"/>.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementationDirectory">The implementation to be made available via the <see cref="EnvironmentBinding"/>.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="Binding"/> to.</param>
        private static void ApplyEnvironmentBinding(EnvironmentBinding binding, string implementationDirectory, ProcessStartInfo startInfo)
        {
            var environmentVariables = startInfo.EnvironmentVariables;

            string newValue = string.IsNullOrEmpty(binding.Value)
                // A path inside the implementation
                ? Path.Combine(implementationDirectory, FileUtils.UnifySlashes(binding.Insert ?? ""))
                // A static value
                : binding.Value;

            // Set the default value if the variable is not already set on the system
            if (!environmentVariables.ContainsKey(binding.Name)) environmentVariables.Add(binding.Name, binding.Default);

            string previousValue = environmentVariables[binding.Name];
            string separator = (string.IsNullOrEmpty(binding.Separator) ? Path.PathSeparator.ToString() : binding.Separator);

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
        /// Applies a <see cref="WorkingDir"/> change to the <see cref="ProcessStartInfo"/>.
        /// </summary>
        /// <param name="workingDir">The <see cref="WorkingDir"/> to apply.</param>
        /// <param name="implementation">The implementation to be made available via the <see cref="WorkingDir"/> change.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="WorkingDir"/> change to.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if the <paramref name="workingDir"/> has an invalid path or another working directory has already been set.</exception>
        /// <remarks>This method can only be called successfully once per <see cref="GetStartInfo"/>.</remarks>
        private void ApplyWorkingDir(WorkingDir workingDir, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            string source = FileUtils.UnifySlashes(workingDir.Source) ?? "";
            if (Path.IsPathRooted(source) || source.Contains(".." + Path.DirectorySeparatorChar)) throw new CommandException(Resources.WorkingDirInvalidPath);

            // Only allow working directory to be changed once
            if (!string.IsNullOrEmpty(startInfo.WorkingDirectory)) throw new CommandException(Resources.WokringDirDuplicate);

            startInfo.WorkingDirectory = Path.Combine(GetImplementationPath(implementation), source);
        }
        #endregion

        #region Command-line
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

        /// <summary>
        /// Determines the command-line needed to execute an <see cref="ImplementationSelection"/>. Recursivley handles <see cref="Runner"/>s.
        /// </summary>
        /// <param name="implementation">The implementation to launch.</param>
        /// <param name="startInfo">The process launch environment to apply additional <see cref="Binding"/> to.</param>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        private C5.LinkedList<string> GetCommandLine(ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            if (implementation.Commands.IsEmpty) throw new ArgumentException(Resources.NoImplementationsPassed, "implementation");
            #endregion

            var command = implementation.Commands.First;

            // Apply bindings implementations use to find themselves and their dependencies
            ApplyBindings(command, implementation, startInfo);
            if (command.WorkingDir != null) ApplyWorkingDir(command.WorkingDir, implementation, startInfo);
            ApplyDependencyBindings(command, startInfo);

            C5.LinkedList<string> commandLine;
            var runner = command.Runner;
            if (runner == null) commandLine = new C5.LinkedList<string>();
            else
            {
                commandLine = GetCommandLine(Selections[runner.Interface], startInfo);
                commandLine.AddAll(runner.Arguments);
            }

            // The command's path (relative to the interface)
            if (!string.IsNullOrEmpty(command.Path)) commandLine.Add(GetCommandPath(implementation));
            commandLine.AddAll(command.Arguments);

            return commandLine;
        }

        /// <summary>
        /// Applies a command-line to a process launch environment, expanding Unix-style environment variables.
        /// </summary>
        /// <param name="startInfo">The process launch environment to apply the command-line to.</param>
        /// <param name="commandLine">The command-line elements to apply.</param>
        private static void ApplyCommandLine(ProcessStartInfo startInfo, C5.IList<string> commandLine)
        {
            startInfo.FileName = commandLine.First;
            commandLine.RemoveFirst();

            var commandLineArray = commandLine.ToArray();
            for (int i = 0; i < commandLineArray.Length; i++)
                commandLineArray[i] = StringUtils.ExpandUnixVariables(commandLineArray[i], startInfo.EnvironmentVariables);

            startInfo.Arguments = StringUtils.ConcatenateEscapeArgument(commandLineArray);
        }
        #endregion

        #region Path helpers
        /// <summary>
        /// Locates an implementation on the disk (usually in an <see cref="IStore"/>).
        /// </summary>
        /// <param name="implementation">The <see cref="ImplementationBase"/> to be located.</param>
        /// <returns>A fully qualified path pointing to the implementation's location on the local disk.</returns>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        public string GetImplementationPath(ImplementationBase implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            #endregion

            return (string.IsNullOrEmpty(implementation.LocalPath) ? Store.GetPath(implementation.ManifestDigest) : implementation.LocalPath);
        }

        /// <summary>
        /// Gets the fully qualified path of a <see cref="Command"/> used to start an implementation.
        /// </summary>
        private string GetCommandPath(ImplementationSelection implementation)
        {
            string path = FileUtils.UnifySlashes(implementation.Commands.First.Path);

            // Fully qualified paths are used by package/native implementatinos
            if (Path.IsPathRooted(path)) return path;

            return Path.Combine(GetImplementationPath(implementation), path);
        }
        #endregion
    }
}
