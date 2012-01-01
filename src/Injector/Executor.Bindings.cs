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
using System.Globalization;
using System.IO;
using System.Reflection;
using Common.Storage;
using Common.Streams;
using Common.Utils;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    public partial class Executor
    {
        #region Structs
        /// <summary>
        /// Represents a run-environment executable configuration pending to be applied via environment variables.
        /// </summary>
        private struct RunEnvPending
        {
            public readonly string Name;

            public readonly C5.IList<string> CommandLine;

            public RunEnvPending(string name, C5.IList<string> commandLine)
            {
                Name = name;
                CommandLine = commandLine;
            }
        }
        #endregion

        #region Variables
        /// <summary>
        /// A list of run-environment executables pending to be configured.
        /// </summary>
        private readonly C5.LinkedList<RunEnvPending> _runEnvPendings = new C5.LinkedList<RunEnvPending>();
        #endregion

        //--------------------//

        #region Start info
        /// <summary>
        /// Accumulates bindings in a process environment.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if an <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if a <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        private ProcessStartInfo BuildStartInfo()
        {
            var startInfo = new ProcessStartInfo {ErrorDialog = false, UseShellExecute = false};
            foreach (var implementation in Selections.Implementations)
            {
                // Apply bindings implementations use to find themselves and their dependencies
                ApplyBindings(implementation, implementation, startInfo);
                ApplyDependencyBindings(implementation, startInfo);
            }

            return startInfo;
        }
        #endregion

        #region Dependency
        /// <summary>
        /// Applies <see cref="Binding"/>s to make a set of <see cref="Dependency"/>s available.
        /// </summary>
        /// <param name="dependencyContainer">The list of <see cref="Dependency"/>s to follow.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="Binding"/>s to.</param>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if an <see cref="Implementation"/> is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if a <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        private void ApplyDependencyBindings(IDependencyContainer dependencyContainer, ProcessStartInfo startInfo)
        {
            foreach (var dependency in dependencyContainer.Dependencies)
                ApplyBindings(dependency, Selections[dependency.Interface], startInfo);
        }
        #endregion

        #region Container
        /// <summary>
        /// Applies all <see cref="Binding"/>s listed in a specific <see cref="IBindingContainer"/>.
        /// </summary>
        /// <param name="bindingContainer">The list of <see cref="Binding"/>s to be performed.</param>
        /// <param name="implementation">The implementation to be made available via the <see cref="Binding"/>s.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="Binding"/>s to.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if a <see cref="Command"/> contained invalid data.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        private void ApplyBindings(IBindingContainer bindingContainer, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            if (bindingContainer.Bindings.IsEmpty) return;

            // Don't use bindings for PackageImplementations
            if (!string.IsNullOrEmpty(implementation.Package)) return;

            foreach (var binding in bindingContainer.Bindings)
            {
                var environmentBinding = binding as EnvironmentBinding;
                if (environmentBinding != null) ApplyEnvironmentBinding(environmentBinding, implementation, startInfo);
                    //else
                    //{
                    //    var overlayBinding = binding as OverlayBinding;
                    //    if (overlayBinding != null) ApplyOverlayBinding(overlayBinding, implementation, startInfo);
                    //}
                else
                {
                    var executableInVar = binding as ExecutableInVar;
                    if (executableInVar != null) ApplyExecutableInVar(executableInVar, implementation, startInfo);
                    else
                    {
                        var executableInPath = binding as ExecutableInPath;
                        if (executableInPath != null) ApplyExecutableInPath(executableInPath, implementation, startInfo);
                    }
                }
            }
        }
        #endregion

        #region Environment
        /// <summary>
        /// Applies an <see cref="EnvironmentBinding"/> by modifying environment variables.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <param name="startInfo">The process launch environment to apply the binding to.</param>
        private void ApplyEnvironmentBinding(EnvironmentBinding binding, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            var environmentVariables = startInfo.EnvironmentVariables;

            string newValue = (binding.Value == "")
                // A path inside the implementation
                ? Path.Combine(GetImplementationPath(implementation), FileUtils.UnifySlashes(binding.Insert ?? ""))
                // A static value
                : binding.Value;

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

        #region Executable
        /// <summary>
        /// Applies an <see cref="ExecutableInVar"/> binding by creating a run-environment executable.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <param name="startInfo">The process launch environment to use to make the run-environment executable available.</param>
        /// <exception cref="CommandException">Thrown if <see cref="ExecutableInVar.Name"/> contains invalid characters.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        private void ApplyExecutableInVar(ExecutableInVar binding, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            // Point variable directly to executable
            startInfo.EnvironmentVariables.Add(binding.Name, DeployRunEnvExecutable(binding.Name));

            _runEnvPendings.Add(new RunEnvPending(binding.Name, GetCommandLine(implementation, binding.Command, startInfo)));
        }

        /// <summary>
        /// Applies an <see cref="ExecutableInPath"/> binding by creating a run-environment executable.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <param name="startInfo">The process launch environment to use to make the run-environment executable available.</param>
        /// <exception cref="CommandException">Thrown if <see cref="ExecutableInPath.Name"/> contains invalid characters.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        private void ApplyExecutableInPath(ExecutableInPath binding, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            // Add executable directory to PATH variable
            startInfo.EnvironmentVariables["PATH"] = Path.GetDirectoryName(DeployRunEnvExecutable(binding.Name)) + startInfo.EnvironmentVariables["PATH"];

            _runEnvPendings.Add(new RunEnvPending(binding.Name, GetCommandLine(implementation, binding.Command, startInfo)));
        }

        /// <summary>
        /// Deploys a copy (hard or soft link if possible) of the run-environment executable within a cache directory.
        /// </summary>
        /// <param name="name">The executable name to deploy under (without file extensions).</param>
        /// <returns>The fully qualified path of the deployed run-environment executable.</returns>
        /// <exception cref="CommandException">Thrown if the <paramref name="name"/> contains invalid characters.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        /// <remarks>A run-environment executable executes a command-line specified in an environment variable based on its own name.</remarks>
        private static string DeployRunEnvExecutable(string name)
        {
            if (Array.Exists(Path.GetInvalidFileNameChars(), invalidChar => name.Contains(invalidChar.ToString())))
                throw new CommandException(Resources.IllegalCharInExecutableBinding);

            // ToDo: Add different binaries for Windows GUI apps and for Linux
            string templatePath = Path.Combine(Locations.GetCacheDirPath("0install.net", "injector", "executables"), "runenv.cli.template");
            if (!File.Exists(templatePath))
                WriteOutEmbeddedResource("runenv.cli.template", templatePath);

            string deployedPath = FileUtils.PathCombine(Locations.GetCacheDirPath("0install.net", "injector", "executables", name), name);
            if (WindowsUtils.IsWindows) deployedPath += ".exe";

            if (WindowsUtils.IsWindowsNT) WindowsUtils.CreateHardLink(deployedPath, templatePath);
            else if (MonoUtils.IsUnix)
            {
                MonoUtils.CreateSymlink(deployedPath, templatePath);
                MonoUtils.SetExecutable(deployedPath, true);
            }
            else File.Copy(templatePath, deployedPath);

            return deployedPath;
        }

        /// <summary>
        /// Writes the contents of an embedded resource to a file.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource.</param>
        /// <param name="filePath">The file to write the data to.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        private static void WriteOutEmbeddedResource(string resourceName, string filePath)
        {
            var assembly = Assembly.GetAssembly(typeof(Executor));
            using (var resourceStream = assembly.GetManifestResourceStream(typeof(Executor), resourceName))
            using (var fileStream = File.OpenWrite(filePath))
                StreamUtils.Copy(resourceStream, fileStream);
        }
        #endregion

        #region Working dir
        /// <summary>
        /// Applies a <see cref="WorkingDir"/> change to the <see cref="ProcessStartInfo"/>.
        /// </summary>
        /// <param name="workingDir">The <see cref="WorkingDir"/> to apply.</param>
        /// <param name="implementation">The implementation to be made available via the <see cref="WorkingDir"/> change.</param>
        /// <param name="startInfo">The process launch environment to apply the <see cref="WorkingDir"/> change to.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if the <paramref name="workingDir"/> has an invalid path or another working directory has already been set.</exception>
        /// <remarks>This method can only be called successfully once per <see cref="BuildStartInfo()"/>.</remarks>
        private void ApplyWorkingDir(WorkingDir workingDir, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            string source = FileUtils.UnifySlashes(workingDir.Source) ?? "";
            if (Path.IsPathRooted(source) || source.Contains(".." + Path.DirectorySeparatorChar)) throw new CommandException(Resources.WorkingDirInvalidPath);

            // Only allow working directory to be changed once
            if (!string.IsNullOrEmpty(startInfo.WorkingDirectory)) throw new CommandException(Resources.WokringDirDuplicate);

            startInfo.WorkingDirectory = Path.Combine(GetImplementationPath(implementation), source);
        }
        #endregion
    }
}
