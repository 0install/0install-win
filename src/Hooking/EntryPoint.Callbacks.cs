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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using EasyHook;
using Microsoft.Win32;

namespace ZeroInstall.Hooking
{
    // ReSharper disable ClassNeverInstantiated.Global
    public partial class EntryPoint
    {
        #region RegQueryValueEx
        private uint RegQueryValueExWCallback(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint cbData)
        {
            string filteredData = GetFilteredValue(hKey, lpValueName);
            if (filteredData == null)
            { // Pass call through unmodified
                return UnsafeNativeMethods.RegQueryValueExW(hKey, lpValueName, lpReserved, out dwType, lpData, ref cbData);
            }
            else
            { // Write filtered result to buffer
                dwType = RegistryValueKind.String;
                return WriteStringToBuffer(lpData, ref cbData, true, filteredData);
            }
        }

        private uint RegQueryValueExACallback(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint cbData)
        {
            string filteredData = GetFilteredValue(hKey, lpValueName);
            if (filteredData == null)
            { // Pass call through unmodified
                return UnsafeNativeMethods.RegQueryValueExA(hKey, lpValueName, lpReserved, out dwType, lpData, ref cbData);
            }
            else
            { // Write filtered result to buffer
                dwType = RegistryValueKind.String;
                return WriteStringToBuffer(lpData, ref cbData, false, filteredData);
            }
        }
        #endregion

        #region RegSetValueEx
        private uint RegSetValueExWCallback(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData)
        {
            bool tempBuffer = false;
            if (dwType == RegistryValueKind.String && lpData != IntPtr.Zero) tempBuffer = FilterWriteBuffer(ref lpData, ref cbData, unicode: true);

            uint result = UnsafeNativeMethods.RegSetValueExW(hKey, lpValueName, lpReserved, dwType, lpData, cbData);
            if (tempBuffer) Marshal.FreeHGlobal(lpData);
            return result;
        }

        private uint RegSetValueExACallback(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData)
        {
            bool tempBuffer = false;
            if (dwType == RegistryValueKind.String && lpData != IntPtr.Zero) tempBuffer = FilterWriteBuffer(ref lpData, ref cbData, unicode: false);

            uint result = UnsafeNativeMethods.RegSetValueExA(hKey, lpValueName, lpReserved, dwType, lpData, cbData);
            if (tempBuffer) Marshal.FreeHGlobal(lpData);
            return result;
        }
        #endregion

        #region Registry helper methods
        /// <summary>
        /// Perfrms a registry read operation applying <see cref="_registryFilter"/>.
        /// </summary>
        /// <param name="hKey">A registry key handle.</param>
        /// <param name="valueName">The nam of the registry value in <paramref name="hKey"/> to read.</param>
        /// <returns>The value from the registry with any required substitutions applied or <see langword="null"/> if nothing was changed.</returns>
        private string GetFilteredValue(IntPtr hKey, string valueName)
        {
            RegistryValueKind valueType;
            uint dataLength = 0;

            // Determine necessary buffer size and value type
            uint result = UnsafeNativeMethods.RegQueryValueExW(hKey, valueName, IntPtr.Zero, out valueType, IntPtr.Zero, ref dataLength);
            if (result != 0 || dataLength == 0 || valueType != RegistryValueKind.String) return null;

            // Read string data to buffer
            IntPtr buffer = IntPtr.Zero;
            do
            {
                if (buffer != IntPtr.Zero) Marshal.FreeHGlobal(buffer);
                buffer = Marshal.AllocHGlobal((int)dataLength);

                result = UnsafeNativeMethods.RegQueryValueExW(hKey, valueName, IntPtr.Zero, out valueType, buffer, ref dataLength);
            } while (result == UnsafeNativeMethods.ErrorMoreData);

            if (result == 0)
            {
                // Filter data
                string registryData = Marshal.PtrToStringUni(buffer, (int)dataLength);
                Marshal.FreeHGlobal(buffer);
                return _registryFilter.ReadFilter(registryData);
            }
            else
            {
                Marshal.FreeHGlobal(buffer);
                return null;
            }
        }

        /// <summary>
        /// Writes data to a string buffer.
        /// </summary>
        /// <param name="buffer">The string buffer to write to.</param>
        /// <param name="dataLength">Initially the length of the buffer; set to the length of the encoded data after the method call.</param>
        /// <param name="unicode"><paramref langword="true"/> to to use Unicode-encoding in the buffer, <paramref langword="false"/> for ANSI.</param>
        /// <param name="data">The string data to be written.</param>
        /// <returns>The Windows API error code to ereturn, usually indicating whether <paramref name="dataLength"/> was sufficient.</returns>
        private static uint WriteStringToBuffer(IntPtr buffer, ref uint dataLength, bool unicode, string data)
        {
            // Encode the data for storage in the buffer
            uint bufferSize = dataLength;
            byte[] encodedData = (unicode ? Encoding.Unicode : Encoding.Default).GetBytes(data);
            dataLength = (uint)encodedData.Length;

            // Write the data to the buffer if it fits
            if (buffer == IntPtr.Zero) return 0;
            else if (dataLength > bufferSize) return UnsafeNativeMethods.ErrorMoreData;
            else
            {
                Marshal.Copy(encodedData, 0, buffer, encodedData.Length);
                return 0;
            }
        }

        /// <summary>
        /// Applies filter operations to string buffers for registry set/write operations.
        /// </summary>
        /// <param name="dataPointer">A pointer to a buffer containing the data retrieved from the registry.</param>
        /// <param name="dataLength">The length of the data in the buffer specified by <paramref name="dataPointer"/>.</param>
        /// <param name="unicode"><see langword="true"/> for Unicode-strings; <see langword="false"/> for ANSI-strings.</param>
        /// <returns>
        ///   <see langword="true"/> if the data was changed and a new temporary buffer is now referenced by <paramref name="dataPointer"/> (needs to be manually freed);
        ///   <see langword="false"/> if nothing was changed.
        /// </returns>
        private bool FilterWriteBuffer(ref IntPtr dataPointer, ref uint dataLength, bool unicode)
        {
            // Read string from buffer and apply filters
            string processData = unicode ? Marshal.PtrToStringUni(dataPointer, (int)dataLength) : Marshal.PtrToStringAnsi(dataPointer, (int)dataLength);
            string registryData = _registryFilter.WriteFilter(processData);
            if (registryData == null) return false; // Cancel if nothing was changed

            // Build new buffer for the string
            dataPointer = unicode ? Marshal.StringToHGlobalUni(registryData) : Marshal.StringToHGlobalAnsi(registryData);
            dataLength = (uint)(unicode ? Encoding.Unicode : Encoding.Default).GetByteCount(registryData);
            return true;
        }
        #endregion

        #region CreateProcess
        private bool CreateProcessWCallback(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation)
        {
            // Determine whether the child process belongs to the implementation and also needs hooking
            bool needsInjection;
            try
            {
                string target = Path.GetFullPath(string.IsNullOrEmpty(lpApplicationName) ? lpCommandLine.Trim('"') : lpApplicationName);
                needsInjection = target.StartsWith(_implementationDir) || target.StartsWith('"' + _implementationDir);
            }
            catch
            {
                needsInjection = false;
            }

            // Start the process suspended
            if (needsInjection) dwCreationFlags |= UnsafeNativeMethods.CreateSuspended;

            var result = UnsafeNativeMethods.CreateProcessW(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, out lpProcessInformation);

            // Inject the hooking DLL (and resume the process)
            if (needsInjection)
            {
                RemoteHooking.Inject(lpProcessInformation.dwProcessId, AssemblyStrongName, AssemblyStrongName,
                    // Custom arguments
                    _implementationDir, _registryFilter, _relaunchControl);
            }

            return result;
        }

        private bool CreateProcessACallback(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation)
        {
            // Determine whether the child process belongs to the implementation and also needs hooking
            bool needsInjection;
            try
            {
                string target = Path.GetFullPath(string.IsNullOrEmpty(lpApplicationName) ? lpCommandLine.Trim('"') : lpApplicationName);
                needsInjection = target.StartsWith(_implementationDir) || target.StartsWith('"' + _implementationDir);
            }
            catch
            {
                needsInjection = false;
            }

            // Start the process suspended
            if (needsInjection) dwCreationFlags |= UnsafeNativeMethods.CreateSuspended;

            var result = UnsafeNativeMethods.CreateProcessA(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, out lpProcessInformation);

            // Inject the hooking DLL (and resume the process)
            if (needsInjection)
            {
                RemoteHooking.Inject(lpProcessInformation.dwProcessId, AssemblyStrongName, AssemblyStrongName,
                    // Custom arguments
                    _implementationDir, _registryFilter, _relaunchControl);
            }

            return result;
        }
        #endregion

        #region CreateWindowEx
        private IntPtr CreateWindowExWCallback(uint dwExStyle, IntPtr lpClassName, IntPtr lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam)
        {
            var windowHandle = UnsafeNativeMethods.CreateWindowExW(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
            if (hWndParent == IntPtr.Zero) ConfigureTaskbar(windowHandle);

            return windowHandle;
        }

        private IntPtr CreateWindowExACallback(uint dwExStyle, IntPtr lpClassName, IntPtr lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam)
        {
            var windowHandle = UnsafeNativeMethods.CreateWindowExA(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
            if (hWndParent == IntPtr.Zero) ConfigureTaskbar(windowHandle);

            return windowHandle;
        }

        /// <summary>
        /// Applies relaunch command and jump list modifications to a Windows 7 taskbar entry.
        /// </summary>
        /// <param name="windowHandle">A handle to the window the taskbar entry belongs to.</param>
        private void ConfigureTaskbar(IntPtr windowHandle)
        {
            if (_relaunchInformation == null || !WindowsUtils.IsWindows7) return;

            // Add correct relaunch information
            string commandPath = (_relaunchInformation.NeedsTerminal ? _relaunchControl.CommandPathCli : _relaunchControl.CommandPathGui + " --no-wait"); // Select best suited launcher
            string icon = (string.IsNullOrEmpty(_relaunchInformation.IconPath) ? null : _relaunchInformation.IconPath + ",0"); // Always use the first icon in the file
            WindowsUtils.SetWindowAppID(windowHandle,
                _relaunchInformation.AppID, '"' + commandPath + "\" run " + _relaunchInformation.Target, icon, _relaunchInformation.Name);

            // Add jump list entry to select an alternative application version
            WindowsUtils.AddTaskLinks(_relaunchInformation.AppID, new[] {new WindowsUtils.ShellLink("Versions", _relaunchControl.CommandPathGui, "run --gui " + _relaunchInformation.Target)});
        }
        #endregion
    }

    // ReSharper restore ClassNeverInstantiated.Global
}
