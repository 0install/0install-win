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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Utils;
using ZeroInstall.Launcher.Properties;
using ZeroInstall.Model;
using ZeroInstall.Launcher.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Launcher
{
    /// <summary>
    /// Executes a set of <see cref="ImplementationSelection"/>s as a program using dependency injection.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class Executor
    {
        #region Variables
        /// <summary>The specific <see cref="Model.Implementation"/>s chosen for the <see cref="Dependency"/>s.</summary>
        private readonly Selections _selections;

        /// <summary>Used to locate the selected <see cref="Model.Implementation"/>s.</summary>
        private readonly IStore _store;
        #endregion

        #region Properties
        /// <summary>
        /// An alternative executable to to run from the main <see cref="Model.Implementation"/> instead of <see cref="Element.Main"/>.
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        /// Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.
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

            _selections = selections.CloneSelections();
            _store = store;
        }
        #endregion

        //--------------------//

        #region Prepare process
        /// <summary>
        /// Prepares a <see cref="ProcessStartInfo"/> for executing the program as specified by the <see cref="Selections"/>.
        /// </summary>
        /// <param name="arguments">Arguments to be passed to the launched programs.</param>
        /// <returns>The <see cref="ProcessStartInfo"/> that can be used to start the new <see cref="Process"/>.</returns>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        public ProcessStartInfo GetStartInfo(string arguments)
        {
            #region Sanity checks
            if (_selections.Commands.IsEmpty) throw new CommandException(Resources.NoCommands);
            #endregion

            var commands = GetCommands();

            // Build command line and add custom wrapper and arguments
            string commandLine = (Wrapper + " " + BuildCommandLine(commands) + arguments).Trim();

            // Prepare the new process to launch the implementation
            ProcessStartInfo startInfo = GetStartInfoHelper(commandLine);

            ApplyBindings(startInfo);

            return startInfo;
        }
        #endregion

        #region Get commands
        /// <summary>
        /// Gets the effective list of commands. Based on <see cref="Selections.Commands"/> but applying possible <see cref="Main"/> overrides.
        /// </summary>
        private IEnumerable<Command> GetCommands()
        {
            var commands = _selections.Commands;

            // Replace first command with custom main if specified
            if (!string.IsNullOrEmpty(Main))
            {
                string mainPath = StringUtils.UnifySlashes(Main);
                if (mainPath[0] == Path.DirectorySeparatorChar)
                { // Relative to implementation root
                    mainPath = mainPath.TrimStart(Path.DirectorySeparatorChar);

                    // Replace list and keep only one command
                    // TODO: Sure about this?
                    commands = new C5.ArrayList<Command> {new Command {Path = mainPath}};
                }
                else
                { // Relative to original command
                    // Clone the list because it needs to be modified (don't want to change original selections)
                    commands = (C5.ArrayList<Command>)commands.Clone();

                    commands[0] = new Command {Path = Path.Combine(Path.GetDirectoryName(_selections.Commands[0].Path) ?? "", mainPath), Runner = _selections.Commands[0].Runner};
                }
            }

            return commands;
        }
        #endregion

        #region Build command line
        /// <summary>
        /// Builds a complete command-line to execute from a list of commands.
        /// </summary>
        private string BuildCommandLine(IEnumerable<Command> commands)
        {
            string commandLine = "";

            // The firts command alaways refers to the actual interface to be launched
            string currentRunnerInterface = _selections.InterfaceID;

            foreach (Command command in commands)
            {
                string commandString = GetPath(_selections.GetImplementation(currentRunnerInterface), command);
                if (command.Arguments.Count != 0) commandString += " " + StringUtils.Concatenate(command.Arguments, " ");

                // Prepend the string to the command-line
                commandLine = commandString + " " + commandLine;

                // Determine the interface the next command will refer to
                if (command.Runner != null) currentRunnerInterface = command.Runner.Interface;
            }

            return commandLine;
        }
        #endregion

        #region Get StartInfo helper
        /// <summary>
        /// Creates a <see cref="ProcessStartInfo"/> from a command-line.
        /// </summary>
        private static ProcessStartInfo GetStartInfoHelper(string commandLine)
        {
            // HACK: Split command-line into file name and arguments part for Windows
            string fileName;
            string arguments;
            if (commandLine.StartsWith("\""))
            {
                string temp = commandLine.Trim('"');
                fileName = StringUtils.GetLeftPartAtFirstOccurrence(temp, "\" ");
                arguments = StringUtils.GetRightPartAtFirstOccurrence(temp, "\" ");
            }
            else
            {
                fileName = StringUtils.GetLeftPartAtFirstOccurrence(commandLine, ' ');
                arguments = StringUtils.GetRightPartAtFirstOccurrence(commandLine, ' ');
            }

            return new ProcessStartInfo(fileName, arguments) {ErrorDialog = false, UseShellExecute = false};
        }
        #endregion

        #region Path helpers
        /// <summary>
        /// Locates an <see cref="ImplementationBase"/> on the disk (usually in an <see cref="IStore"/>).
        /// </summary>
        /// <param name="implementation">The <see cref="ImplementationBase"/> to be located.</param>
        /// <returns>A fully qualified path pointing to the implementation's location on the local disk.</returns>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        private string GetImplementationPath(ImplementationBase implementation)
        {
            return (string.IsNullOrEmpty(implementation.LocalPath) ? _store.GetPath(implementation.ManifestDigest) : implementation.LocalPath);
        }

        /// <summary>
        /// Gets the fully qualified path of a command inside an implementation.
        /// </summary>
        /// <exception cref="CommandException">Throw if <paramref name="command"/>'s path is empty, not relative or tries to point outside the implementation directory.</exception>
        private string GetPath(ImplementationSelection implementation, Command command)
        {
            string path = StringUtils.UnifySlashes(command.Path);

            if (string.IsNullOrEmpty(path) || Path.IsPathRooted(path) || path.Contains(".." + Path.DirectorySeparatorChar)) throw new CommandException(Resources.CommandInvalidPath);

            return "\"" + Path.Combine(GetImplementationPath(implementation), path) + "\"";
        }
        #endregion

        #region Bindings
        /// <summary>
        /// Applies <see cref="Binding"/>s allowing the launched program to locate <see cref="ImplementationSelection"/>s.
        /// </summary>
        /// <param name="startInfo">The programs environment to apply the  <see cref="Binding"/>s to.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if an <see cref="Model.Implementation"/> is not cached yet.</exception>
        private void ApplyBindings(ProcessStartInfo startInfo)
        {
            foreach (var implementation in _selections.Implementations)
            {
                // Apply bindings implementations use to find themselves
                ApplyBindings(startInfo, implementation, implementation);

                // Apply bindings implementations use to find their dependencies
                foreach (var dependency in implementation.Dependencies)
                    ApplyBindings(startInfo, dependency, _selections.GetImplementation(dependency.Interface));
            }

            foreach (var command in _selections.Commands)
            {
                // Apply bindings commands use to find their dependencies
                foreach (var dependency in command.Dependencies)
                    ApplyBindings(startInfo, dependency, _selections.GetImplementation(dependency.Interface));

                if (command.Runner != null)
                    ApplyBindings(startInfo, command.Runner, _selections.GetImplementation(command.Runner.Interface));
            }
        }

        /// <summary>
        /// Applies all <see cref="Binding"/>s listed in a specific <see cref="IBindingContainer"/>.
        /// </summary>
        /// <param name="startInfo">The programs environment to apply the  <see cref="Binding"/>s to.</param>
        /// <param name="bindingContainer">The list of <see cref="Binding"/>s to be performed.</param>
        /// <param name="implementation">The <see cref="ImplementationSelection"/> to be made locatable via the <see cref="Binding"/>s.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        private void ApplyBindings(ProcessStartInfo startInfo, IBindingContainer bindingContainer, ImplementationSelection implementation)
        {
            if (bindingContainer.Bindings.IsEmpty) return;

            // Don't use bindings for PackageImplementations
            if (!string.IsNullOrEmpty(implementation.Package)) return;

            string implementationDirectory = GetImplementationPath(implementation);

            foreach (var binding in bindingContainer.Bindings)
            {
                var environmentBinding = binding as EnvironmentBinding;
                if (environmentBinding != null) ApplyEnvironmentBinding(startInfo, implementationDirectory, environmentBinding);
                //else
                //{
                //    var overlayBinding = binding as OverlayBinding;
                //    if (overlayBinding != null) ApplyOverlayBinding(startInfo, implementationDirectory, overlayBinding);
                //}
            }
        }

        private static void ApplyEnvironmentBinding(ProcessStartInfo startInfo, string implementationDirectory, EnvironmentBinding binding)
        {
            var environmentVariables = startInfo.EnvironmentVariables;
            string environmentValue = Path.Combine(implementationDirectory, StringUtils.UnifySlashes(binding.Value));

            if (!environmentVariables.ContainsKey(binding.Name)) environmentVariables.Add(binding.Name, binding.Default);

            switch (binding.Mode)
            {
                case EnvironmentMode.Prepend:
                    environmentVariables[binding.Name] = environmentValue + Path.PathSeparator + environmentVariables[binding.Name];
                    break;
                case EnvironmentMode.Append:
                    environmentVariables[binding.Name] += Path.PathSeparator + environmentValue;
                    break;
                case EnvironmentMode.Replace:
                    environmentVariables[binding.Name] = environmentValue;
                    break;
            }
        }
        #endregion
    }
}
