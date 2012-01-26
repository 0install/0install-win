/*
 * Copyright 2010-2012 Bastian Eicher
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
using Microsoft.Win32;

namespace ZeroInstall.Hooking
{
    public partial class EntryPoint
    {
        #region Structures
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
        #endregion

        private static class UnsafeNativeMethods
        {
            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern void SetLastError(uint dwErrCode);

            #region RegQueryValueEx
            public const uint ErrorMoreData = 0xEA;

            [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern uint RegQueryValueExW(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint lpcbData);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public delegate uint DRegQueryValueExW(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint lpcbData);

            [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false)]
            public static extern uint RegQueryValueExA(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint lpcbData);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false)]
            public delegate uint DRegQueryValueExA(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind dwType, IntPtr lpData, ref uint lpcbData);
            #endregion

            #region RegSetValueEx
            [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern uint RegSetValueExW(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public delegate uint DRegSetValueExW(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData);

            [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false)]
            public static extern uint RegSetValueExA(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false)]
            public delegate uint DRegSetValueExA(IntPtr hKey, string lpValueName, int lpReserved, RegistryValueKind dwType, IntPtr lpData, uint cbData);
            #endregion

            #region CreateProcess
            public const uint CreateSuspended = 0x4;

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool CreateProcessW(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            public delegate bool DCreateProcessW(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation);

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true)]
            public static extern bool CreateProcessA(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true)]
            public delegate bool DCreateProcessA(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, IntPtr lpStartupInfo, out ProcessInformation lpProcessInformation);
            #endregion

            #region CreateWindowEx
            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateWindowExW(uint dwExStyle, IntPtr lpClassName, IntPtr lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            public delegate IntPtr DCreateWindowExW(uint dwExStyle, IntPtr lpClassName, IntPtr lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true)]
            public static extern IntPtr CreateWindowExA(uint dwExStyle, IntPtr lpClassName, IntPtr lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true)]
            public delegate IntPtr DCreateWindowExA(uint dwExStyle, IntPtr lpClassName, IntPtr lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
            #endregion
        }
    }
}
