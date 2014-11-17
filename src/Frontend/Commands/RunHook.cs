/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using EasyHook;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.Windows;
using ZeroInstall.Hooking;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Injector;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;
using ZeroInstall.Store.Model.Selection;
using EntryPoint = ZeroInstall.Hooking.EntryPoint;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Hooks into the creation of new processes to inject API hooks.
    /// </summary>
    internal sealed class RunHook : IDisposable
    {
        #region Variables
        private readonly ITaskHandler _handler;

        private readonly InterfaceFeed _target;

        /// <summary>The local path the selected main implementation is launched from.</summary>
        private readonly string _implementationDir;

        private readonly Implementation _mainImplementation;

        private readonly RegistryFilter _registryFilter;
        private readonly RelaunchControl _relaunchControl;

        private readonly LocalHook _hookW, _hookA;
        #endregion

        #region Constuctor
        /// <summary>
        /// Hooks into the creation of new processes on the current thread to inject API hooks.
        /// </summary>
        /// <param name="selections">The implementations chosen for launch.</param>
        /// <param name="executor">The executor used to launch the new process.</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <exception cref="ImplementationNotFoundException">The main implementation is not cached (possibly because it is installed natively).</exception>
        public RunHook(Selections selections, IExecutor executor, IFeedManager feedManager, ITaskHandler handler)
        {
            _target = new InterfaceFeed(selections.InterfaceID, feedManager.GetFeed(selections.MainImplementation.FromFeed));

            var mainImplementation = selections.MainImplementation;
            _implementationDir = executor.GetImplementationPath(mainImplementation);
            _mainImplementation = _target.Feed[mainImplementation.ID];

            _handler = handler;
            _registryFilter = GetRegistryFilter();
            _relaunchControl = GetRelaunchControl();

            _hookW = LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "CreateProcessW"), new UnsafeNativeMethods.DCreateProcessW(CreateProcessWCallback), null);
            _hookW.ThreadACL.SetInclusiveACL(new[] {Thread.CurrentThread.ManagedThreadId});
            _hookA = LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "CreateProcessA"), new UnsafeNativeMethods.DCreateProcessA(CreateProcessACallback), null);
            _hookA.ThreadACL.SetInclusiveACL(new[] {Thread.CurrentThread.ManagedThreadId});
        }
        #endregion

        #region Registry filter
        /// <summary>
        /// Gets a set of filter rules for registry access.
        /// </summary>
        private RegistryFilter GetRegistryFilter()
        {
            var filterRuleList = new LinkedList<RegistryFilterRule>();

            if (_mainImplementation != null)
            {
                // Create one substitution stub for each command
                foreach (var command in _mainImplementation.Commands)
                {
                    // Only handle simple commands (executable path, no arguments)
                    if (string.IsNullOrEmpty(command.Path) || command.Arguments.Count != 0) continue;

                    string processCommandLine = Path.Combine(_implementationDir, command.Path);

                    string registryCommandLine;
                    try
                    { // Try to use a machine-wide stub if possible
                        registryCommandLine = _target.GetRunStub(true, _handler, command.Name);
                    }
                    catch (UnauthorizedAccessException)
                    { // Fall back to per-user stub
                        registryCommandLine = _target.GetRunStub(false, _handler, command.Name);
                    }

                    // Apply filter with normal and with escaped string
                    filterRuleList.AddLast(new RegistryFilterRule(processCommandLine, registryCommandLine));
                    filterRuleList.AddLast(new RegistryFilterRule("\"" + processCommandLine + "\"", "\"" + registryCommandLine + "\""));
                }
            }

            // Redirect Windows SPAD commands to Zero Install
            foreach (var defaultProgram in _target.Feed.CapabilityLists.CompatibleCapabilities().OfType<Store.Model.Capabilities.DefaultProgram>())
            {
                if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.Reinstall))
                    filterRuleList.AddLast(GetInstallCommandFilter(defaultProgram.InstallCommands.Reinstall, defaultProgram.InstallCommands.ReinstallArgs, "--machine --batch --add=defaults " + _target.InterfaceID.EscapeArgument()));
                if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.ShowIcons))
                    filterRuleList.AddLast(GetInstallCommandFilter(defaultProgram.InstallCommands.ShowIcons, defaultProgram.InstallCommands.ShowIconsArgs, "--machine --batch --add=icons " + _target.InterfaceID.EscapeArgument()));
                if (!string.IsNullOrEmpty(defaultProgram.InstallCommands.HideIcons))
                    filterRuleList.AddLast(GetInstallCommandFilter(defaultProgram.InstallCommands.HideIcons, defaultProgram.InstallCommands.HideIconsArgs, "--machine --batch --remove=icons " + _target.InterfaceID.EscapeArgument()));
            }

            return new RegistryFilter(filterRuleList);
        }

        /// <summary>
        /// Builds a <see cref="RegistryFilterRule"/> for a <see cref="Store.Model.Capabilities.InstallCommands"/> entry.
        /// </summary>
        /// <param name="path">The path of the command relative to the <see name="_implementationDir"/>.</param>
        /// <param name="arguments">Additional arguments passed to the <paramref name="path"/>.</param>
        /// <param name="zeroInstallCommand">The Zero Install command to be executed instead of the <paramref name="path"/>.</param>
        private RegistryFilterRule GetInstallCommandFilter(string path, string arguments, string zeroInstallCommand)
        {
            string exePath = Path.Combine(Locations.InstallBase, "0install-win.exe");
            return new RegistryFilterRule(
                Path.Combine(_implementationDir, path).EscapeArgument() + " " + arguments,
                exePath.EscapeArgument() + " " + zeroInstallCommand);
        }
        #endregion

        #region Relaunch control
        /// <summary>
        /// Collects information about how <see cref="Command"/>s within an <see cref="Implementation"/> can be relaunched.
        /// </summary>
        private RelaunchControl GetRelaunchControl()
        {
            // This will be used as a command-line argument
            string escapedTarget = _target.InterfaceID.EscapeArgument();

            // Build a relaunch entry for each entry point
            var entries =
                from entryPoint in _target.Feed.EntryPoints
                where !string.IsNullOrEmpty(entryPoint.Command) && entryPoint.Names.Count != 0 && !string.IsNullOrEmpty(entryPoint.BinaryName)
                select new RelaunchEntry(
                    entryPoint.BinaryName,
                    entryPoint.Names.GetBestLanguage(CultureInfo.CurrentUICulture) ?? _target.Feed.Name,
                    "--command=" + entryPoint.Command.EscapeArgument() + " " + escapedTarget,
                    entryPoint.NeedsTerminal,
                    GetIconPath(entryPoint.Command));

            // Create a relaunch entry for the main application
            entries = entries.Concat(new[]
            {
                new RelaunchEntry(
                    null, // This will always be matched last, serves as a universal fallback
                    _target.Feed.Name,
                    escapedTarget,
                    _target.Feed.NeedsTerminal,
                    GetIconPath())
            });

            return new RelaunchControl(entries,
                Path.Combine(Locations.InstallBase, "0install-win.exe"),
                Path.Combine(Locations.InstallBase, "0install.exe"));
        }

        /// <summary>
        /// Returns the path to a permanent copy of the best matching icon for a specific <see cref="Command"/>.
        /// </summary>
        /// <param name="command">The name of the command the icon should represent; <see langword="null"/> for <see cref="Command.NameRun"/>.</param>
        /// <returns>The path to the icon file; <see langword="null"/> if no suitable icon was found.</returns>
        private string GetIconPath(string command = null)
        {
            var icon = _target.Feed.GetIcon(Icon.MimeTypeIco, command);
            return icon == null ? null : IconProvider.GetIconPath(icon, _handler, machineWide: false);
        }
        #endregion

        //--------------------//

        #region Native
        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessInformation
        {
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
            // ReSharper restore FieldCanBeMadeReadOnly.Local
        }

        private static class UnsafeNativeMethods
        {
            public const uint CreateSuspended = 0x4;

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool CreateProcessW(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            public delegate bool DCreateProcessW(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation);

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true)]
            public static extern bool CreateProcessA(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true)]
            public delegate bool DCreateProcessA(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation);
        }
        #endregion

        #region Callback
        private bool CreateProcessWCallback(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation)
        {
            // Start the process suspended
            var result = UnsafeNativeMethods.CreateProcessW(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags | UnsafeNativeMethods.CreateSuspended, lpEnvironment, lpCurrentDirectory, lpStartupInfo, out lpProcessInformation);

            // Inject the hooking DLL (and resume the process)
            RemoteHooking.Inject(lpProcessInformation.dwProcessId, EntryPoint.AssemblyStrongName, EntryPoint.AssemblyStrongName,
                // Custom arguments
                _implementationDir, _registryFilter, _relaunchControl);

            return result;
        }

        private bool CreateProcessACallback(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation)
        {
            // Start the process suspended
            var result = UnsafeNativeMethods.CreateProcessA(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags | UnsafeNativeMethods.CreateSuspended, lpEnvironment, lpCurrentDirectory, lpStartupInfo, out lpProcessInformation);

            // Inject the hooking DLL (and resume the process)
            RemoteHooking.Inject(lpProcessInformation.dwProcessId, EntryPoint.AssemblyStrongName, EntryPoint.AssemblyStrongName,
                // Custom arguments
                _implementationDir, _registryFilter, _relaunchControl);

            return result;
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <summary>
        /// Unhooks from the creation of new processes.
        /// </summary>
        public void Dispose()
        {
            _hookW.Dispose();
            _hookA.Dispose();
        }
        #endregion
    }
}
