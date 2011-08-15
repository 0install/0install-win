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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using EasyHook;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.Windows;
using ZeroInstall.Hooking;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    public sealed partial class Run
    {
        /// <summary>
        /// Starts a process with hooks for operating sytem APIs to improve desktop integration.
        /// </summary>
        /// <param name="executor">The process to start.</param>
        /// <param name="interfaceID">The interface ID the process represents.</param>
        /// <returns>The newly spawned process.</returns>
        private Process StartHooked(Executor executor, string interfaceID)
        {
            string implementationDir = executor.GetImplementationPath(Selections.Implementations.Last);

            // Get data from feed
            bool stale;
            var target = new InterfaceFeed(interfaceID, Policy.FeedManager.GetFeed(interfaceID, Policy, out stale));

            // Start proces with hooks
            int processID;
            var startInfo = executor.GetStartInfo(AdditionalArgs.ToArray());
            RemoteHooking.CreateAndInject(startInfo.FileName, StringUtils.EscapeArgument(startInfo.FileName) + " " + startInfo.Arguments, 0, Hooking.EntryPoint.AssemblyStrongName, Hooking.EntryPoint.AssemblyStrongName, out processID,
                // Custom arguments
                startInfo.EnvironmentVariables,
                implementationDir,
                GetRegistryFilter(target, implementationDir),
                GetRelaunchControl(target));

            try { return Process.GetProcessById(processID); }
            catch (ArgumentException) { return null; }
        }

        #region Registry filter
        /// <summary>
        /// Gets a set of filter rules for registry access.
        /// </summary>
        /// <param name="target">The application to be launched.</param>
        /// <param name="implementationDir">The local path the selected main implementation is launched from.</param>
        private RegistryFilter GetRegistryFilter(InterfaceFeed target, string implementationDir)
        {
            var filterRuleList = new LinkedList<RegistryFilterRule>();

            var mainImplementation = target.Feed.GetImplementation(Selections.Implementations.First.ID);
            if (mainImplementation != null)
            {
                // Create one substitution stub for each command
                foreach (var command in mainImplementation.Commands)
                {
                    // Only handle simple commands (executable path, no arguments)
                    if (string.IsNullOrEmpty(command.Path) || !command.Arguments.IsEmpty) continue;

                    string processCommandLine = Path.Combine(implementationDir, command.Path);

                    string registryCommandLine;
                    try
                    { // Try to use a system-wide stub if possible
                        registryCommandLine = StubProvider.GetRunStub(target, command.Name, true, Policy.Handler);
                    }
                    catch (UnauthorizedAccessException)
                    { // Fall back to per-user stub
                        registryCommandLine = StubProvider.GetRunStub(target, command.Name, false, Policy.Handler);
                    }

                    // Apply filter with normal and with escaped string
                    filterRuleList.AddLast(new RegistryFilterRule(processCommandLine, registryCommandLine));
                    filterRuleList.AddLast(new RegistryFilterRule('"' + processCommandLine + '"', '"' + registryCommandLine + '"'));
                }
            }

            // Redirect Windows SPAD commands to Zero Install
            foreach (var capabilityList in target.Feed.CapabilityLists.FindAll(list => list.Architecture.IsCompatible(Architecture.CurrentSystem)))
            {
                foreach (var defaultProgram in EnumerableUtils.OfType<Model.Capabilities.DefaultProgram>(capabilityList.Entries))
                {
                    if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.Reinstall))
                        filterRuleList.AddLast(GetInstallCommandFilter(implementationDir, defaultProgram.InstallCommands.Reinstall, defaultProgram.InstallCommands.ReinstallArgs, "--global --batch --add=defaults " + StringUtils.EscapeArgument(target.InterfaceID)));
                    if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.ShowIcons))
                        filterRuleList.AddLast(GetInstallCommandFilter(implementationDir, defaultProgram.InstallCommands.ShowIcons, defaultProgram.InstallCommands.ShowIconsArgs, "--global --batch --add=icons " + StringUtils.EscapeArgument(target.InterfaceID)));
                    if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.HideIcons))
                        filterRuleList.AddLast(GetInstallCommandFilter(implementationDir, defaultProgram.InstallCommands.HideIcons, defaultProgram.InstallCommands.HideIconsArgs, "--global --batch --remove=icons " + StringUtils.EscapeArgument(target.InterfaceID)));
                }
            }

            return new RegistryFilter(filterRuleList);
        }

        /// <summary>
        /// Builds a <see cref="RegistryFilterRule"/> for a <see cref="ZeroInstall.Model.Capabilities.InstallCommands"/> entry.
        /// </summary>
        /// <param name="implementationDir">The local path the selected main implementation is launched from.</param>
        /// <param name="command">The path of the command relative to the <paramref name="implementationDir"/>.</param>
        /// <param name="arguments">Additional arguments passed to the <paramref name="command"/>.</param>
        /// <param name="zeroInstallCommand">The Zero Install command to be executed instead of the <paramref name="command"/>.</param>
        private static RegistryFilterRule GetInstallCommandFilter(string implementationDir, string command, string arguments, string zeroInstallCommand)
        {
            string exePath = Path.Combine(Locations.InstallBase, "0install-win.exe");
            return new RegistryFilterRule(
                StringUtils.EscapeArgument(Path.Combine(implementationDir, command)) + " " + arguments,
                StringUtils.EscapeArgument(exePath) + " " + zeroInstallCommand);
        }
        #endregion

        #region Relaunch control
        /// <summary>
        /// Collects information about how <see cref="Command"/>s within an <see cref="Implementation"/> can be relaunched.
        /// </summary>
        /// <param name="target">The application to be launched.</param>
        private RelaunchControl GetRelaunchControl(InterfaceFeed target)
        {
            // This will be used as a command-line argument
            string escapedTarget = StringUtils.EscapeArgument(target.InterfaceID);

            // Build a relaunch entry for each entry point
            var entries = new List<RelaunchEntry>();
            foreach (var entryPoint in target.Feed.EntryPoints)
            {
                // Only handle entry valid points that specify names and binary names
                if (string.IsNullOrEmpty(entryPoint.Command) || entryPoint.Names.IsEmpty || string.IsNullOrEmpty(entryPoint.BinaryName)) continue;

                entries.Add(new RelaunchEntry(
                    entryPoint.BinaryName,
                    entryPoint.Names.GetBestLanguage(CultureInfo.CurrentCulture),
                     "--command=" + StringUtils.EscapeArgument(entryPoint.Command) + " " + escapedTarget,
                     entryPoint.NeedsTerminal,
                     GetIconPath(target.Feed, entryPoint.Command)));
            }

            // Create a relaunch entry for the main application
            entries.Add(new RelaunchEntry(
                null, // This will always be matched last, serves as a universal fallback
                target.Feed.Name,
                escapedTarget,
                target.Feed.NeedsTerminal,
                GetIconPath(target.Feed, null)));

            return new RelaunchControl(entries,
                Path.Combine(Locations.InstallBase, "0install-win.exe"),
                Path.Combine(Locations.InstallBase, "0install.exe"));
        }

        /// <summary>
        /// Returns the path to a permanent copy of the best matching icon for a specific <see cref="Command"/>.
        /// </summary>
        /// <param name="feed">The feed to search for icons.</param>
        /// <param name="command">The name of the command the icon should represent; <see langword="null"/> for <see cref="Command.NameRun"/>.</param>
        /// <returns>The path to the icon file; <see langword="null"/> if no suitable icon was found.</returns>
        private string GetIconPath(Feed feed, string command)
        {
            try
            {
                var icon = feed.GetIcon(Icon.MimeTypeIco, command);
                return IconProvider.GetIconPath(icon, false, Policy.Handler);
            }
            catch(KeyNotFoundException)
            {
                return null;
            }
        }
        #endregion
    }
}
