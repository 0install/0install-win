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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using EasyHook;

namespace ZeroInstall.Hooking
{
    /// <summary>
    /// Used as an entry point when injecting the hooking DLL into another process.
    /// </summary>
    public partial class EntryPoint : IEntryPoint
    {
        #region Constants
        /// <summary>
        /// The strong name of the assembly used to locate it in the GAC.
        /// </summary>
        public const string AssemblyStrongName = "ZeroInstall.Hooking,PublicKeyToken=3090a828a7702cec";
        #endregion

        #region Variables
        private readonly string _implementationDir;

        private readonly RegistryFilter _registryFilter;

        private readonly RelaunchControl _relaunchControl;

        private readonly RelaunchEntry _relaunchInformation;

        private LocalHook _regQueryValueExWHook, _regQueryValueExAHook,
            _regSetValueExWHook, _regSetValueExAHook,
            _createProcessWHook, _createProcessAHook,
            _createWindowExWHook, _createWindowExAHook;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new entry point.
        /// </summary>
        /// <param name="inContext">Hooking context information.</param>
        /// <param name="implentationDir">The base directory of the implementation containing this executable.</param>
        /// <param name="registryFilter">A set of filter rules to registry access.</param>
        /// <param name="relaunchControl">Stores information about how commands within an implementation can be relaunched. Used for Windows 7 taskbar pinning.</param>
        public EntryPoint(RemoteHooking.IContext inContext, string implentationDir, RegistryFilter registryFilter, RelaunchControl relaunchControl)
        {
            _implementationDir = implentationDir;
            _registryFilter = registryFilter;
            _relaunchControl = relaunchControl;
            _relaunchInformation = relaunchControl.GetCurrentEntry();
        }
        #endregion

        //--------------------//

        #region Injection
        /// <summary>
        /// Sets up the API hooks and maintains them.
        /// </summary>
        /// <param name="inContext">Hooking context information.</param>
        /// <param name="implentationDir">The base directory of the implementation containing this executable.</param>
        /// <param name="registryFilter">A set of filter rules to registry access.</param>
        /// <param name="relaunchControl">Stores information about how commands within an implementation can be relaunched. Used for Windows 7 taskbar pinning.</param>
        public void Run(RemoteHooking.IContext inContext, string implentationDir, RegistryFilter registryFilter, RelaunchControl relaunchControl)
        {
            try { SetupHooks(); }
            #region Error handling
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                string errorLogFile = Path.Combine(Path.GetTempPath(), "ZeroInstall.Hooking.errorlog.txt");
                File.WriteAllText(errorLogFile, ex.ToString());
                EventLog.WriteEntry("Zero Install", ex.ToString(), EventLogEntryType.Error);
                return;
            }
            #endregion
            finally { RemoteHooking.WakeUpProcess(); }

            while (true) Thread.Sleep(500);
        }

        private void SetupHooks()
        {
            _regQueryValueExWHook = LocalHook.Create(LocalHook.GetProcAddress("advapi32.dll", "RegQueryValueExW"), new UnsafeNativeMethods.DRegQueryValueExW(RegQueryValueExWCallback), null);
            _regQueryValueExWHook.ThreadACL.SetExclusiveACL(new[] {0});
            _regQueryValueExAHook = LocalHook.Create(LocalHook.GetProcAddress("advapi32.dll", "RegQueryValueExA"), new UnsafeNativeMethods.DRegQueryValueExA(RegQueryValueExACallback), null);
            _regQueryValueExAHook.ThreadACL.SetExclusiveACL(new[] {0});

            _regSetValueExWHook = LocalHook.Create(LocalHook.GetProcAddress("advapi32.dll", "RegSetValueExW"), new UnsafeNativeMethods.DRegSetValueExW(RegSetValueExWCallback), null);
            _regSetValueExWHook.ThreadACL.SetExclusiveACL(new[] {0});
            _regSetValueExAHook = LocalHook.Create(LocalHook.GetProcAddress("advapi32.dll", "RegSetValueExA"), new UnsafeNativeMethods.DRegSetValueExA(RegSetValueExACallback), null);
            _regSetValueExAHook.ThreadACL.SetExclusiveACL(new[] {0});

            _createProcessWHook = LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "CreateProcessW"), new UnsafeNativeMethods.DCreateProcessW(CreateProcessWCallback), null);
            _createProcessWHook.ThreadACL.SetExclusiveACL(new[] {0});
            _createProcessAHook = LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "CreateProcessA"), new UnsafeNativeMethods.DCreateProcessA(CreateProcessACallback), null);
            _createProcessAHook.ThreadACL.SetExclusiveACL(new[] {0});

            _createWindowExWHook = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "CreateWindowExW"), new UnsafeNativeMethods.DCreateWindowExW(CreateWindowExWCallback), null);
            _createWindowExWHook.ThreadACL.SetExclusiveACL(new[] {0});
            _createWindowExAHook = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "CreateWindowExA"), new UnsafeNativeMethods.DCreateWindowExA(CreateWindowExACallback), null);
            _createWindowExAHook.ThreadACL.SetExclusiveACL(new[] {0});
        }
        #endregion
    }
}
