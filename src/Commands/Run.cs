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
using System.IO;
using System.Threading;
using Common;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using EasyHook;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Hooking;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Download"/>, except that it also runs the program after ensuring it is in the cache.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Run : Download
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "run";
        #endregion

        #region Variables
        /// <summary>An alternative executable to to run from the main <see cref="Model.Implementation"/> instead of <see cref="Element.Main"/>.</summary>
        private string _main;

        /// <summary>Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.</summary>
        private string _wrapper;

        /// <summary>Immediately returns once the chosen program has been launched instead of waiting for it to finish executing.</summary>
        private bool _noWait;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRun; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionRun; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Run(Policy policy) : base(policy)
        {
            //Options.Remove("xml");
            //Options.Remove("show");

            Options.Add("m|main=", Resources.OptionMain, newMain => _main = newMain);
            Options.Add("w|wrapper=", Resources.OptionWrapper, newWrapper => _wrapper = newWrapper);
            Options.Add("no-wait", Resources.OptionNoWait, unused => _noWait = true);

            // Work-around to disable interspersed arguments (needed for passing arguments through to sub-processes)
            Options.Add("<>", value =>
            {
                AdditionalArgs.Add(value);

                // Stop using options parser, treat everything from here on as unknown
                Options.Clear();
            });
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            #endregion

            Policy.Handler.ShowProgressUI(Cancel);
            
            Solve();
            
            // If any implementations need to be downloaded rerun solver in refresh mode (unless it was already in that mode to begin with)
            if (!EnumerableUtils.IsEmpty(UncachedImplementations) && !Policy.FeedManager.Refresh && Policy.Config.NetworkUse != NetworkLevel.Offline)
            {
                Policy.FeedManager.Refresh = true;
                Solve();
            }
            SelectionsUI();

            DownloadUncachedImplementations();

            // If any of the feeds are getting old spawn background update process
            if (StaleFeeds && Policy.Config.NetworkUse != NetworkLevel.Offline)
            {
                // ToDo: Automatically switch to GTK# on Linux
                ProcessUtils.LaunchHelperAssembly("0install-win", "update --batch " + Requirements.ToCommandLineArgs());
            }

            if (Canceled) throw new UserCancelException();
            return LaunchImplementation();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Launches the selected implementation.
        /// </summary>
        /// <returns>The exit code of the process or 0 if waiting is disabled.</returns>
        private int LaunchImplementation()
        {
            // Prevent the user from pressing any buttons once the child process is being launched
            Policy.Handler.DisableProgressUI();

            // Spawn the child process; try to add desktop integration API hooks on Windows
            var executor = new Executor(Selections, Policy.Fetcher.Store) {Main = _main, Wrapper = _wrapper};
            Process process;
            if (Policy.Config.AllowApiHooking && WindowsUtils.IsWindows)
            {
                try { process = StartHooked(executor, Requirements.InterfaceID); }
                #region Error handling
                catch (ApplicationException ex)
                {
                    Log.Error(ex.Message);

                    // Fallback to normal startup
                    process = StartNormal(executor);
                }
                #endregion
            }
            else process = StartNormal(executor);

            // Wait for a moment before closing the GUI so that focus is retained until it can be passed on to the child process
            Thread.Sleep(1000);
            Policy.Handler.CloseProgressUI();

            if (_noWait || process == null) return 0;
            process.WaitForExit();
            try { return process.ExitCode; }
            catch (InvalidOperationException) { return 0; }
        }

        /// <summary>
        /// Starts a process.
        /// </summary>
        /// <param name="executor">The process to start.</param>
        private Process StartNormal(Executor executor)
        {
            return Process.Start(executor.GetStartInfo(AdditionalArgs.ToArray()));
        }
        #endregion

        #region Hooking
        /// <summary>
        /// Starts a process with hooks for operating sytem APIs to improve desktop integration.
        /// </summary>
        /// <param name="executor">The process to start.</param>
        /// <param name="interfaceID">The interface ID the process represents.</param>
        /// <returns>The newly spawned process.</returns>
        private Process StartHooked(Executor executor, string interfaceID)
        {
            string implementationDir = executor.GetImplementationPath(Selections.Implementations.Last);

            // Get relevant desktop integration data
            bool stale;
            var feed = Policy.FeedManager.GetFeed(interfaceID, Policy, out stale);
            var registryFilter = GetRegistryFilter(new InterfaceFeed(interfaceID, feed), implementationDir);

            // Start proces with hooks
            int processID;
            var startInfo = executor.GetStartInfo(AdditionalArgs.ToArray());
            RemoteHooking.CreateAndInject(startInfo.FileName, startInfo.Arguments, 0, Hooking.EntryPoint.AssemblyStrongName, Hooking.EntryPoint.AssemblyStrongName, out processID, startInfo.EnvironmentVariables, interfaceID, implementationDir, registryFilter);

            try { return Process.GetProcessById(processID); }
            catch (ArgumentException) { return null; }
        }

        /// <summary>
        /// Gets a set of filter rules for registry access.
        /// </summary>
        /// <param name="target">The application to be launched.</param>
        /// <param name="implementationDir">The local path the selected main implementation is launched from.</param>
        private RegistryFilter GetRegistryFilter(InterfaceFeed target, string implementationDir)
        {
            // Locate the selected main implementation
            var mainImplementation = target.Feed.GetImplementation(Selections.Implementations.Last.ID);

            // Create one substitution stub for each command
            var filterRuleList = new LinkedList<RegistryFilterRule>();
            foreach (var command in mainImplementation.Commands)
            {
                // Only handle simple commands (executable path, no arguments)
                if (string.IsNullOrEmpty(command.Path) || !command.Arguments.IsEmpty) continue;

                string processCommandLine = Path.Combine(implementationDir, command.Path);

                string registryCommandLine;
                try
                { // Try to use a system-wide stub if possible
                    registryCommandLine = DesktopIntegration.Windows.StubProvider.GetRunStub(target, command.Name, true, Policy.Handler);
                }
                catch(UnauthorizedAccessException)
                { // Fall back to per-user stub
                    registryCommandLine = DesktopIntegration.Windows.StubProvider.GetRunStub(target, command.Name, false, Policy.Handler);
                }

                // Apply filter with normal and with escaped string
                filterRuleList.AddLast(new RegistryFilterRule(processCommandLine, registryCommandLine));
                filterRuleList.AddLast(new RegistryFilterRule('"' + processCommandLine + '"', '"' + registryCommandLine + '"'));
            }

            // Redirect Windows SPAD commands to Zero Install
            foreach (var capabilityList in target.Feed.CapabilityLists.FindAll(list => list.Architecture.IsCompatible(Architecture.CurrentSystem)))
            {
                foreach (var defaultProgram in EnumerableUtils.OfType<Model.Capabilities.DefaultProgram>(capabilityList.Entries))
                {
                    if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.Reinstall))
                        filterRuleList.AddLast(GetInstallCommandFilter(implementationDir, defaultProgram.InstallCommands.Reinstall, defaultProgram.InstallCommands.ReinstallArgs, "--global --batch --add=defaults " + StringUtils.EscapeWhitespace(target.InterfaceID)));
                    if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.ShowIcons))
                        filterRuleList.AddLast(GetInstallCommandFilter(implementationDir, defaultProgram.InstallCommands.ShowIcons, defaultProgram.InstallCommands.ShowIconsArgs, "--global --batch --add=icons " + StringUtils.EscapeWhitespace(target.InterfaceID)));
                    if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.HideIcons))
                        filterRuleList.AddLast(GetInstallCommandFilter(implementationDir, defaultProgram.InstallCommands.HideIcons, defaultProgram.InstallCommands.HideIconsArgs, "--global --batch --remove=icons " + StringUtils.EscapeWhitespace(target.InterfaceID)));
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
                StringUtils.EscapeWhitespace(Path.Combine(implementationDir, command)) + " " + arguments,
                StringUtils.EscapeWhitespace(exePath) + " " + zeroInstallCommand);
        }
        #endregion
    }
}
