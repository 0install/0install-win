/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Common.Properties;

namespace Common.Utils
{
    /// <summary>
    /// Provides helper methods and API calls specific to the Windows platform.
    /// </summary>
    public static partial class WindowsUtils
    {
        #region Command-line arguments
        /// <summary>
        /// Tries to split a command-line into individual arguments.
        /// </summary>
        /// <param name="commandLine">The command-line to be split.</param>
        /// <returns>
        /// An array of individual arguments.
        /// Will return the entire command-line as one argument when not running on Windows or if splitting failed for some other reason.
        /// </returns>
        public static string[] SplitArgs(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine)) return new string[0];
            if (!IsWindows) return new[] {commandLine};

            int numberOfArgs;
            var ptrToSplitArgs = SafeNativeMethods.CommandLineToArgvW(commandLine, out numberOfArgs);
            if (ptrToSplitArgs == IntPtr.Zero) return new[] {commandLine};

            try
            {
                // Copy result to managed array
                var splitArgs = new string[numberOfArgs];
                for (int i = 0; i < numberOfArgs; i++)
                    splitArgs[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptrToSplitArgs, i * IntPtr.Size));
                return splitArgs;
            }
            finally
            {
                UnsafeNativeMethods.LocalFree(ptrToSplitArgs);
            }
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

            if (!IsWindowsNT) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            handle = UnsafeNativeMethods.CreateMutex(IntPtr.Zero, false, name);

            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case ErrorSuccess:
                    return false;
                case ErrorAlreadyExists:
                    return true;
                case ErrorAccessDenied:
                    return OpenMutex(name);
                default:
                    throw new Win32Exception(error);
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

            if (!IsWindowsNT) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            UnsafeNativeMethods.OpenMutex(Synchronize, false, name);

            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case ErrorSuccess:
                    return true;
                case ErrorFileNotFound:
                    return false;
                default:
                    throw new Win32Exception(error);
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

            if (!IsWindowsNT) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            var handle = UnsafeNativeMethods.OpenMutex(Synchronize, false, name);

            bool result;
            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case ErrorSuccess:
                    result = true;
                    break;
                case ErrorFileNotFound:
                    result = false;
                    break;
                default:
                    throw new Win32Exception(error);
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
            if (!IsWindowsNT) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

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

            if (!IsWindows) return;
            SafeNativeMethods.SetForegroundWindow(form.Handle);
        }
        #endregion

        #region App ID
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

            if (!IsWindows7) return;
            UnsafeNativeMethods.SetCurrentProcessExplicitAppUserModelID(appID);
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

        /// <summary>
        /// Informs all GUI applications that changes where made to the environment variables (e.g. PATH) and that they should re-pull them.
        /// </summary>
        public static void NotifyEnvironmentChanged()
        {
            if (!IsWindows) return;

            var HWND_BROADCAST = new IntPtr(0xFFFF);
            const uint WM_SETTINGCHANGE = 0x001A;
            const uint SMTO_ABORTIFHUNG = 0x0002;
            UIntPtr result;
            UnsafeNativeMethods.SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 5000, out result);
        }
        #endregion

        #region Filesystem
        /// <summary>
        /// Creates a hard link for a file.
        /// </summary>
        /// <param name="source">The new hard link to be created.</param>
        /// <param name="target">The existing file.</param>
        /// <remarks>Only available on Windows 2000 or newer.</remarks>
        /// <exception cref="Win32Exception">Thrown if the hard link creation failed.</exception>
        public static void CreateHardLink(string source, string target)
        {
            if (!IsWindowsNT) throw new NotSupportedException(Resources.OnlyAvailableOnWindows);
            if (!UnsafeNativeMethods.CreateHardLink(source, target, IntPtr.Zero)) throw new Win32Exception();
        }

        /// <summary>
        /// Creates a symbolic link for a file or directory.
        /// </summary>
        /// <param name="source">The new symbolic link to be created.</param>
        /// <param name="target">The existing file or directory to point to.</param>
        /// <remarks>Only available on Windows Vista or newer.</remarks>
        /// <exception cref="Win32Exception">Thrown if the symbolic link creation failed.</exception>
        public static void CreateSymbolicLink(string source, string target)
        {
            if (!IsWindowsVista) throw new NotSupportedException(Resources.OnlyAvailableOnWindows);
            if (!UnsafeNativeMethods.CreateSymbolicLink(source, target, Directory.Exists(target) ? 1 : 0)) throw new Win32Exception();
        }
        #endregion
    }
}
