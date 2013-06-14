﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Linq;
using System.Reflection;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
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
        #endregion

        #region Variables
        /// <summary>
        /// A list of run-environment executables pending to be configured.
        /// </summary>
        private readonly List<RunEnvPending> _runEnvPendings = new List<RunEnvPending>();
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
        private ProcessStartInfo BuildStartInfoWithBindings()
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
            foreach (var dependency in dependencyContainer.Dependencies.Where(IsEssentialOrSelected))
                ApplyBindings(dependency, Selections[dependency.Interface], startInfo);
        }

        private bool IsEssentialOrSelected(Dependency dependency)
        {
            return dependency.Importance == Importance.Essential || Selections.ContainsImplementation(dependency.Interface);
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
        /// <exception cref="CommandException">Thrown if <see cref="EnvironmentBinding.Name"/> or other data is invalid.</exception>
        private void ApplyEnvironmentBinding(EnvironmentBinding binding, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            if (string.IsNullOrEmpty(binding.Name)) throw new CommandException(string.Format(Resources.MissingBindingName, @"<environment>"));

            var environmentVariables = startInfo.EnvironmentVariables;

            string newValue;
            if (binding.Value != null && binding.Insert != null)
            { // Conflict
                throw new CommandException(Resources.EnvironmentBindingValueInvalid);
            }
            else if (binding.Value != null)
            { // Static value
                newValue = binding.Value;
            }
            else
            { // Path inside the implementation
                newValue = Path.Combine(GetImplementationPath(implementation), FileUtils.UnifySlashes(binding.Insert ?? ""));
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

        #region Executable
        /// <summary>
        /// Applies an <see cref="ExecutableInVar"/> binding by creating a run-environment executable.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <param name="startInfo">The process launch environment to use to make the run-environment executable available.</param>
        /// <exception cref="CommandException">Thrown if <see cref="ExecutableInVar.Name"/> is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        private void ApplyExecutableInVar(ExecutableInVar binding, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            if (string.IsNullOrEmpty(binding.Name)) throw new CommandException(string.Format(Resources.MissingBindingName, @"<executable-in-var>"));
            if (Path.GetInvalidFileNameChars().Any(invalidChar => binding.Name.Contains(invalidChar.ToString(CultureInfo.InvariantCulture))))
                throw new CommandException(string.Format(Resources.IllegalCharInBindingName, @"<executable-in-var>"));

            string exePath = DeployRunEnvExecutable(binding.Name);

            // Point variable directly to executable
            startInfo.EnvironmentVariables.Add(binding.Name, exePath);

            // Tell the executable what command-line to run
            _runEnvPendings.Add(new RunEnvPending(binding.Name, GetCommandLine(implementation, binding.Command, startInfo)));
        }

        /// <summary>
        /// Applies an <see cref="ExecutableInPath"/> binding by creating a run-environment executable.
        /// </summary>
        /// <param name="binding">The binding to apply.</param>
        /// <param name="implementation">The implementation to be made available.</param>
        /// <param name="startInfo">The process launch environment to use to make the run-environment executable available.</param>
        /// <exception cref="CommandException">Thrown if <see cref="ExecutableInPath.Name"/> is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        private void ApplyExecutableInPath(ExecutableInPath binding, ImplementationSelection implementation, ProcessStartInfo startInfo)
        {
            if (string.IsNullOrEmpty(binding.Name)) throw new CommandException(string.Format(Resources.MissingBindingName, @"<executable-in-path>"));
            if (Path.GetInvalidFileNameChars().Any(invalidChar => binding.Name.Contains(invalidChar.ToString(CultureInfo.InvariantCulture))))
                throw new CommandException(string.Format(Resources.IllegalCharInBindingName, @"<executable-in-path>"));

            string exePath = DeployRunEnvExecutable(binding.Name);

            // Add executable directory to PATH variable
            startInfo.EnvironmentVariables["PATH"] = Path.GetDirectoryName(exePath) + Path.PathSeparator + startInfo.EnvironmentVariables["PATH"];

            // Tell the executable what command-line to run
            _runEnvPendings.Add(new RunEnvPending(binding.Name, GetCommandLine(implementation, binding.Command, startInfo)));
        }

        /// <summary>
        /// Deploys a copy (hard or soft link if possible) of the run-environment executable within a cache directory.
        /// </summary>
        /// <param name="name">The executable name to deploy under (without file extensions).</param>
        /// <returns>The fully qualified path of the deployed run-environment executable.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        /// <remarks>A run-environment executable executes a command-line specified in an environment variable based on its own name.</remarks>
        private static string DeployRunEnvExecutable(string name)
        {
            string templatePath = GetRunEnvTemplate();
            string deployedPath = Path.Combine(Locations.GetCacheDirPath("0install.net", false, "injector", "executables", name), name);
            if (WindowsUtils.IsWindows) deployedPath += ".exe";

            try
            {
                if (File.Exists(deployedPath)) File.Delete(deployedPath);
                FileUtils.CreateHardlink(deployedPath, templatePath);
            }
                #region Error handling
            catch (IOException)
            {
                // File is probably currently in use
                if (!File.Exists(deployedPath)) throw;
            }
            catch (UnauthorizedAccessException)
            {
                // File is probably currently in use
                if (!File.Exists(deployedPath)) throw;
            }
            catch (PlatformNotSupportedException)
            {
                // Unable to hardlink, fallback to simple copy
                File.Copy(templatePath, deployedPath);
            }
            #endregion

            if (MonoUtils.IsUnix) FileUtils.SetExecutable(deployedPath, true);

            return deployedPath;
        }

        /// <summary>
        /// Deploys an appropriate runenv binary template for the current operating system.
        /// </summary>
        /// <returns>The path to the deployed executable file.</returns>
        private static string GetRunEnvTemplate()
        {
            string templateName;
            if (WindowsUtils.IsWindows)
            {
                templateName = (Directory.Exists(WindowsUtils.GetNetFxDirectory(WindowsUtils.NetFx40)))
                    ? "runenv.clr4.template"
                    : "runenv.clr2.template";
            }
            else if (MonoUtils.IsUnix) templateName = "runenv.sh.template";
            else throw new NotSupportedException(string.Format(Resources.BindingNotSupportedOnCurrentOS, @"<executable-in-*>"));

            string path = Path.Combine(Locations.GetCacheDirPath("0install.net", false, "injector", "executables"), templateName);
            try
            {
                WriteOutEmbeddedResource(templateName, path);
            }
                #region Error handling
            catch (IOException)
            {
                // File is probably currently in use
            }
            #endregion

            return path;
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
                resourceStream.CopyTo(fileStream);
        }

        /// <summary>
        /// Split and apply command-lines for executable bindings.
        /// This is delayed until the end because environment variables that might be modified are expanded.
        /// </summary>
        private void ProcessRunEnvBindings(ProcessStartInfo startInfo)
        {
            foreach (var runEnv in _runEnvPendings)
            {
                var split = SplitCommandLine(ExpandCommandLine(runEnv.CommandLine, startInfo.EnvironmentVariables));
                startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_FILE_" + runEnv.ExeName] = split.Path;
                startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_ARGS_" + runEnv.ExeName] = split.Arguments;
            }
            _runEnvPendings.Clear();

            // Make sure archives always get extracted by .NET code even if a Python-version of Zero Install is executed
            if (WindowsUtils.IsWindows)
                startInfo.EnvironmentVariables["ZEROINSTALL_EXTERNAL_STORE"] = Path.Combine(Locations.InstallBase, "0store.exe");
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
        /// <remarks>This method can only be called successfully once per <see cref="BuildStartInfoWithBindings()"/>.</remarks>
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
