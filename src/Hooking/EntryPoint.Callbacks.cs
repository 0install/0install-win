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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ZeroInstall.Hooking
{
    public partial class EntryPoint
    {
        #region RegQueryValueEx
        private const uint ErrorMoreData = 0xEA;

        private uint RegQueryValueExWCallback(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint cbData)
        {
            uint bufferSize = cbData;
            var result = UnsafeNativeMethods.RegQueryValueExW(hKey, lpValueName, lpReserved, out dwType, lpData, ref cbData);

            if (result == 0 && dwType == RegistryValueKind.String)
            {
                if (lpData == IntPtr.Zero)
                {
                    // Load string into temporary buffer to determine the filtered length, even if the user doesn't want the value (yet)
                    var tempData = Marshal.AllocHGlobal((int)cbData);
                    UnsafeNativeMethods.RegQueryValueExW(hKey, lpValueName, lpReserved, out dwType, tempData, ref cbData);
                    RegQueryFilter(tempData, ref cbData, 0, true);
                    Marshal.FreeHGlobal(tempData);
                }
                else if (!RegQueryFilter(lpData, ref cbData, bufferSize, true)) return ErrorMoreData;
            }
            return result;
        }

        private uint RegQueryValueExACallback(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint cbData)
        {
            uint bufferSize = cbData;
            var result = UnsafeNativeMethods.RegQueryValueExA(hKey, lpValueName, lpReserved, out dwType, lpData, ref cbData);

            if (result == 0 && dwType == RegistryValueKind.String)
            {
                if (cbData > 5000)
                    MessageBox.Show(cbData.ToString());
                if (lpData == IntPtr.Zero)
                {
                    // Load string into temporary buffer to determine the filtered length, even if the user doesn't want the value (yet)
                    var tempData = Marshal.AllocHGlobal((int)cbData);
                    UnsafeNativeMethods.RegQueryValueExA(hKey, lpValueName, lpReserved, out dwType, tempData, ref cbData);
                    RegQueryFilter(tempData, ref cbData, 0, false);
                    Marshal.FreeHGlobal(tempData);
                }
                else if (!RegQueryFilter(lpData, ref cbData, bufferSize, false)) return ErrorMoreData;
            }
            return result;
        }

        /// <summary>
        /// Applies filter operations to registry query/read operations.
        /// </summary>
        /// <param name="dataPointer">A pointer to a buffer containing the data retreived from the registry.</param>
        /// <param name="dataLength">The length of the data in the buffer specified by <paramref name="dataPointer"/>.</param>
        /// <param name="bufferSize">The size of the buffer specified by <paramref name="dataLength"/>.</param>
        /// <param name="unicode"><see langword="true"/> for Unicode-strings; <see langword="false"/> for ANSI-strings.</param>
        /// <returns>
        ///   <see langword="true"/> if the <paramref name="bufferSize"/> was sufficient;
        ///   <see langword="false"/> if it wasn't and <paramref name="dataLength"/> now indicates the required size.
        /// </returns>
        private bool RegQueryFilter(IntPtr dataPointer, ref uint dataLength, uint bufferSize, bool unicode)
        {
            // Read string from buffer and apply filters
            string registryData = unicode ? Marshal.PtrToStringUni(dataPointer, (int)dataLength) : Marshal.PtrToStringAnsi(dataPointer, (int)dataLength);
            string processData = _registryFilter.ReadFilter(registryData);
            if (processData == null) return true; // Cancel if nothing was changed

            // Write string back to buffer (but do not exceed the buffer size)
            byte[] encodedData = (unicode ? Encoding.Unicode : Encoding.Default).GetBytes(processData);
            dataLength = (uint)encodedData.Length;
            if (dataLength > bufferSize) return false;
            Marshal.Copy(encodedData, 0, dataPointer, (int)dataLength);

            return true;
        }
        #endregion

        #region RegSetValueEx
        private uint RegSetValueExWCallback(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData)
        {
            bool tempBuffer = false;
            if (dwType == RegistryValueKind.String && lpData != IntPtr.Zero) tempBuffer = RegSetFilter(ref lpData, ref cbData, true);

            uint result = UnsafeNativeMethods.RegSetValueExW(hKey, lpValueName, lpReserved, dwType, lpData, cbData);
            if (tempBuffer) Marshal.FreeHGlobal(lpData);
            return result;
        }

        private uint RegSetValueExACallback(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData)
        {
            bool tempBuffer = false;
            if (dwType == RegistryValueKind.String && lpData != IntPtr.Zero) tempBuffer = RegSetFilter(ref lpData, ref cbData, false);

            uint result = UnsafeNativeMethods.RegSetValueExA(hKey, lpValueName, lpReserved, dwType, lpData, cbData);
            if (tempBuffer) Marshal.FreeHGlobal(lpData);
            return result;
        }

        /// <summary>
        /// Applies filter operations to registry set/write operations.
        /// </summary>
        /// <param name="dataPointer">A pointer to a buffer containing the data retreived from the registry.</param>
        /// <param name="dataLength">The length of the data in the buffer specified by <paramref name="dataPointer"/>.</param>
        /// <param name="unicode"><see langword="true"/> for Unicode-strings; <see langword="false"/> for ANSI-strings.</param>
        /// <returns>
        ///   <see langword="true"/> if the data was changed and a new temporary buffer is referenced by <paramref name="dataPointer"/> (needs to be manually freed);
        ///   <see langword="false"/> if nothing was changed.
        /// </returns>
        private bool RegSetFilter(ref IntPtr dataPointer, ref uint dataLength, bool unicode)
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
        private bool CreateProcessCallback(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, IntPtr lpProcessInformation)
        {
            // ToDo: Inherit hooks to child processes within the same implementation))
            return UnsafeNativeMethods.CreateProcessW(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, lpProcessInformation);
        }
        #endregion

        #region CreateWindowEx
        private IntPtr CreateWindowExWCallback(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam)
        {
            // ToDo: Set AppUserModelID
            return UnsafeNativeMethods.CreateWindowExW(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
        }

        private IntPtr CreateWindowExACallback(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam)
        {
            // ToDo: Set AppUserModelID
            return UnsafeNativeMethods.CreateWindowExA(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
        }
        #endregion
    }
}
