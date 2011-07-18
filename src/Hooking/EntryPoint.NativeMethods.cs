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
using Microsoft.Win32;

namespace ZeroInstall.Hooking
{
    public partial class EntryPoint
    {
        private static class UnsafeNativeMethods
        {
            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern void SetLastError(uint dwErrCode);

            #region RegQueryValueEx
            [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern uint RegQueryValueExW(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint lpcbData);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public delegate uint DRegQueryValueExW(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint lpcbData);

            [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public static extern uint RegQueryValueExA(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint lpcbData);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public delegate uint DRegQueryValueExA(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint lpcbData);
            #endregion

            #region RegSetValueEx
            [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern uint RegSetValueExW(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public delegate uint DRegSetValueExW(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData);

            [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public static extern uint RegSetValueExA(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false)]
            public delegate uint DRegSetValueExA(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData);
            #endregion

            #region CreateProcess
            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern bool CreateProcessW(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, IntPtr lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public delegate bool DCreateProcessW(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, IntPtr lpProcessInformation);

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public static extern bool CreateProcessA(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, IntPtr lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public delegate bool DCreateProcessA(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, IntPtr lpProcessInformation);
            #endregion

            #region CreateWindowEx
            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern IntPtr CreateWindowExW(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public delegate IntPtr DCreateWindowExW(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public static extern IntPtr CreateWindowExA(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public delegate IntPtr DCreateWindowExA(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
            #endregion
        }
    }
}
