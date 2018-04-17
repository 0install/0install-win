// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ZeroInstall.Hooking
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public partial class EntryPoint
    {
        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local"), SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
        #endregion

        private static class UnsafeNativeMethods
        {
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
