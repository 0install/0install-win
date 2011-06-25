/*
 * Copyright 2006-2011 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Windows.Forms;
using Common.Properties;
using Microsoft.Win32;

namespace Common.Utils
{
    #region Enumerations
    /// <summary>
    /// Represents the thumbnail progress bar state.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "These enum values are mutually exclusive and not meant to be ORed like flags")]
    public enum TaskbarProgressBarState
    {
        /// <summary>
        /// No progress is displayed.
        /// </summary>
        NoProgress = 0,

        /// <summary>
        /// The progress is indeterminate (marquee).
        /// </summary>
        Indeterminate = 0x1,

        /// <summary>
        /// Normal progress is displayed.
        /// </summary>
        Normal = 0x2,

        /// <summary>
        /// An error occurred (red).
        /// </summary>
        Error = 0x4,

        /// <summary>
        /// The operation is paused (yellow).
        /// </summary>
        Paused = 0x8
    }
    #endregion

    /// <summary>
    /// Easily access non-Framework Windows DLLs.
    /// </summary>
    public static class WindowsUtils
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            #region Platform
            [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWow64Process([In] IntPtr hProcess, [Out, MarshalAs(UnmanagedType.Bool)] out bool lpSystemInfo);
            #endregion

            #region Foreground window
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);
            #endregion

            #region Taskbar
            [DllImport("shell32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appID);
            #endregion
        }

        [SuppressUnmanagedCodeSecurity]
        private static class UnsafeNativeMethods
        {
            #region Mutex
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string lpName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr OpenMutex(UInt32 desiredAccess, bool inheritHandle, string name);

            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int CloseHandle(IntPtr hObject);
            #endregion

            #region Window messages
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint msg, UIntPtr wParam, string lParam, uint flags, uint timeout);
            #endregion

            #region Shell
            [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
            #endregion
        }

        #region Platform
        /// <summary>
        /// <see langword="true"/> if the current operating system is a modern desktop Windows version (9x- or NT-based); <see langword="false"/> otherwise.
        /// </summary>
        public static bool IsWindows
        { get { return Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.Win32NT; } }

        /// <summary>
        /// <see langword="true"/> if the current operating system is Windows Vista or newer; <see langword="false"/> otherwise.
        /// </summary>
        public static bool IsWindowsVista
        { get { return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 0); } }

        /// <summary>
        /// <see langword="true"/> if the current operating system is Windows 7 or newer; <see langword="false"/> otherwise.
        /// </summary>
        public static bool IsWindows7
        { get { return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 1); } }

        /// <summary>
        /// <see langword="true"/> if the current operating system is 64-bit capable; <see langword="false"/> otherwise.
        /// </summary>
        public static bool Is64BitOperatingSystem
        { get { return Is64BitProcess || Is32BitProcessOn64BitOperatingSystem; } }

        /// <summary>
        /// <see langword="true"/> if the current process is 64-bit; <see langword="false"/> otherwise.
        /// </summary>
        public static bool Is64BitProcess
        { get { return IntPtr.Size == 8; } }

        /// <summary>
        /// <see langword="true"/> if the current process is 32-bit but the operating system is 64-bit capable; <see langword="false"/> otherwise.
        /// </summary>
        /// <remarks>Can only detect WOW on Windows XP and newer</remarks>
        public static bool Is32BitProcessOn64BitOperatingSystem
        {
            get
            {
                // Can only detect WOW on Windows XP or newer
                if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version < new Version(5, 1)) return false;

                bool retVal;
                SafeNativeMethods.IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);
                return retVal;
            }
        }

        /// <summary>
        /// Indicates whether the current user is an administrator. Always returns <see langword="flase"/> on non-Windows systems.
        /// </summary>
        public static bool IsAdministrator
        {
            get { return IsWindows && new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator); }
        }

        #endregion

        #region Mutex
        private const int ErrorSuccess = 0, ErrorFileNotFound = 2, ErrorAccessDenied = 5, ErrorAlreadyExists = 183;
        private const UInt32 Synchronize = 0x00100000;

        /// <summary>
        /// Creates or opens a mutex.
        /// </summary>
        /// <param name="name">The name to be used as a mutex identifier.</param>
        /// <param name="handle">The handle created for the mutex. Can be used to close it before the process ends.</param>
        /// <returns><see langword="true"/> if an existing mutex was opened; <see langword="false"/> if a new one was created.</returns>
        /// <exception cref="Win32Exception">Thrown if the native subsystem reported a problem.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown when this method is called on a platform other than Windows.</exception>
        /// <remarks>The mutex will automatically be released once the process terminates.</remarks>
        public static bool CreateMutex(string name, out IntPtr handle)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            if (!IsWindows) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            handle = UnsafeNativeMethods.CreateMutex(IntPtr.Zero, false, name);

            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case ErrorSuccess: return false;
                case ErrorAlreadyExists: return true;
                case ErrorAccessDenied: return OpenMutex(name);
                default: throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Tries to open an existing mutex.
        /// </summary>
        /// <param name="name">The name to be used as a mutex identifier.</param>
        /// <returns><see langword="true"/> if an existing mutex was opened; <see langword="false"/> if none existed.</returns>
        /// <exception cref="Win32Exception">Thrown if the native subsystem reported a problem.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown when this method is called on a platform other than Windows.</exception>
        /// <remarks>Opening a mutex creates an additional handle to it, keeping it alive until the process terminates.</remarks>
        public static bool OpenMutex(string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            if (!IsWindows) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            UnsafeNativeMethods.OpenMutex(Synchronize, false, name);

            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case ErrorSuccess: return true;
                case ErrorFileNotFound: return false;
                default: throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Checks whether a specific mutex exists without openining a lasting handle.
        /// </summary>
        /// <param name="name">The name to be used as a mutex identifier.</param>
        /// <returns><see langword="true"/> if an existing mutex was found; <see langword="false"/> if none existed.</returns>
        /// <exception cref="Win32Exception">Thrown if the native subsystem reported a problem.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown when this method is called on a platform other than Windows.</exception>
        public static bool ProbeMutex(string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            if (!IsWindows) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            var handle = UnsafeNativeMethods.OpenMutex(Synchronize, false, name);

            bool result;
            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case ErrorSuccess: result = true; break;
                case ErrorFileNotFound: result = false; break;
                default: throw new Win32Exception(error);
            }

            UnsafeNativeMethods.CloseHandle(handle);
            return result;
        }

        /// <summary>
        /// Closes an existing mutex handle. The mutex is destroyed if this is the last handle.
        /// </summary>
        /// <param name="handle">The mutex handle to be closed.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown when this method is called on a platform other than Windows.</exception>
        public static void CloseMutex(IntPtr handle)
        {
            if (!IsWindows) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            UnsafeNativeMethods.CloseHandle(handle);
        }
        #endregion

        #region Foreground window
        /// <summary>
        /// Forces a window to the foreground or flashes the taskbar if another process has the focus.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This method operates only on windows and not on individual controls.")]
        public static void SetForegroundWindow(Form form)
        {
            #region Sanity checks
            if (form == null) throw new ArgumentNullException("form");
            #endregion

            SafeNativeMethods.SetForegroundWindow(form.Handle);
        }
        #endregion

        #region Taskbar
        /// <summary>
        /// Sets the current process' explicit application user model ID.
        /// </summary>
        /// <param name="appID">The application ID to set.</param>
        /// <remarks>The application ID is used to group related windows in the taskbar.</remarks>
        public static void SetCurrentProcessAppID(string appID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appID)) throw new ArgumentNullException("appID");
            #endregion

            if (IsWindows7)
                SafeNativeMethods.SetCurrentProcessExplicitAppUserModelID(appID);
        }

        private static readonly object _syncLock = new object();
        private static ITaskbarList4 _taskbarList;
        /// <summary>
        /// Singleton COM object for taskbar operations.
        /// </summary>
        private static ITaskbarList4 TaskbarList
        {
            get
            {
                if (_taskbarList == null)
                {
                    lock (_syncLock)
                    {
                        if (_taskbarList == null)
                        {
                            _taskbarList = (ITaskbarList4)new CTaskbarList();
                            _taskbarList.HrInit();
                        }
                    }
                }

                return _taskbarList;
            }
        }

        /// <summary>
        /// Sets the state of the taskbar progress indicator.
        /// </summary>
        /// <param name="handle">The handle of the window whose taskbar button contains the progress indicator.</param>
        /// <param name="state">The state of the progress indicator.</param>
        public static void SetProgressState(TaskbarProgressBarState state, IntPtr handle)
        {
            if (IsWindows7)
                TaskbarList.SetProgressState(handle, (TBPFLAG)state);
        }

        /// <summary>
        /// Sets the value of the taskbar progress indicator.
        /// </summary>
        /// <param name="handle">The handle of the window whose taskbar button contains the progress indicator.</param>
        /// <param name="currentValue">The current value of the progress indicator.</param>
        /// <param name="maximumValue">The value <paramref name="currentValue"/> will have when the operation is complete.</param>
        public static void SetProgressValue(int currentValue, int maximumValue, IntPtr handle)
        {
            if (IsWindows7)
                TaskbarList.SetProgressValue(handle, Convert.ToUInt32(currentValue), Convert.ToUInt32(maximumValue));
        }
        #endregion

        #region Window messages
        /// <summary>
        /// Informs all GUI applications that changes where made to the environment variables (e.g. PATH) and that they should re-pull them.
        /// </summary>
        public static void NotifyEnvironmentChanged()
        {
            if (!IsWindows) return;

            var HWND_BROADCAST = new IntPtr(0xFFFF);
            const uint WM_SETTINGCHANGE = 0x001A;
            const uint SMTO_ABORTIFHUNG = 0x0002;
            UnsafeNativeMethods.SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 10000);
        }
        #endregion

        #region Shell
        /// <summary>
        /// Informs the Windows shell that changes were made to the file association data in the registry.
        /// </summary>
        /// <remarks>This should be called immediatley after the changes in order to trigger a refresh of the Explorer UI.</remarks>
        public static void NotifyAssocChanged()
        {
            if (!IsWindows) return;

            const uint SHCNE_ASSOCCHANGED = 0x08000000;
            const uint SHCNF_IDLIST = 0;
            UnsafeNativeMethods.SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
        #endregion

        #region Registry
        /// <summary>
        /// Retreives the names of all values within a specific subkey of a registry root.
        /// </summary>
        /// <param name="root">The root key to look within.</param>
        /// <param name="key">The path of the subkey below <paramref name="root"/>.</param>
        /// <returns>A list of value names; an empty array if the key does not exist.</returns>
        public static string[] GetValueNames(RegistryKey root, string key)
        {
            #region Sanity checks
            if (root == null) throw new ArgumentNullException("root");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            #endregion

            using (var contextMenuExtendedKey = root.OpenSubKey(key))
                return contextMenuExtendedKey == null ? new string[0] : contextMenuExtendedKey.GetValueNames();
        }

        /// <summary>
        /// Retreives the names of all subkeys within a specific subkey of a registry root.
        /// </summary>
        /// <param name="root">The root key to look within.</param>
        /// <param name="key">The path of the subkey below <paramref name="root"/>.</param>
        /// <returns>A list of key names; an empty array if the key does not exist.</returns>
        public static string[] GetSubKeyNames(RegistryKey root, string key)
        {
            #region Sanity checks
            if (root == null) throw new ArgumentNullException("root");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            #endregion

            using (var contextMenuExtendedKey = root.OpenSubKey(key))
                return contextMenuExtendedKey == null ? new string[0] : contextMenuExtendedKey.GetSubKeyNames();
        }

        /// <summary>
        /// Opens a HKEY_LOCAL_MACHINE key in the registry for reading, first trying to find the 64-bit version of it, then falling back to the 32-bit version.
        /// </summary>
        /// <param name="keyPath">The path to the key below HKEY_LOCAL_MACHINE.</param>
        /// <param name="x64">Indicates whether a 64-bit key was opened.</param>
        /// <returns>The opened registry key or <see langword="null"/> if it could not found.</returns>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if access to the registry was not permitted.</exception>
        public static RegistryKey OpenHklmKey(string keyPath, out bool x64)
        {
            RegistryKey result;
            x64 = false;
            // ToDo: Use Is64BitOperatingSystem and native APIs
            if (Is64BitProcess)
            {
                result = Registry.LocalMachine.OpenSubKey(@"WOW6432Node\" + keyPath);
                if (result == null) result = Registry.LocalMachine.OpenSubKey(keyPath);
                else x64 = true;
            }
            else result = Registry.LocalMachine.OpenSubKey(keyPath);

            return result;
        }
        #endregion
    }
}
