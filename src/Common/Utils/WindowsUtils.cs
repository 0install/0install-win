/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NanoByte.Common.Properties;

namespace NanoByte.Common.Utils
{
    /// <summary>
    /// Provides helper methods and API calls specific to the Windows platform.
    /// </summary>
    public static partial class WindowsUtils
    {
        #region .NET Framework
        /// <summary>The full version number of the .NET Framework 2.0.</summary>
        public const string NetFx20 = "v2.0.50727";

        /// <summary>The full version number of the .NET Framework 3.5.</summary>
        public const string NetFx35 = "v3.5";

        /// <summary>The full version number of the .NET Framework 4.0.</summary>
        public const string NetFx40 = "v4.0.30319";

        /// <summary>
        /// Determines whether a specific version of the .NET framework is available.
        /// </summary>
        /// <param name="version">The full .NET version number including the leading "v". Use predefined constants when possible.</param>
        /// <returns><see langword="true"/> if the specified version is available, <see langword="false"/> otherwise.</returns>
        /// <remarks>Automatically uses 64-bit directories if <see cref="Is64BitProcess"/> is <see langword="true"/>.</remarks>
        public static bool HasNetFxVersion(string version)
        {
            return Directory.Exists(GetNetFxDirectory(version));
        }

        /// <summary>
        /// Returns the .NET Framework root directory for a specific version of the .NET framework. Does not verify the directory actually exists!
        /// </summary>
        /// <param name="version">The full .NET version number including the leading "v". Use predefined constants when possible.</param>
        /// <returns>The path to the .NET Framework root directory.</returns>
        /// <remarks>Automatically uses 64-bit directories if <see cref="Is64BitProcess"/> is <see langword="true"/>.</remarks>
        public static string GetNetFxDirectory(string version)
        {
            return new[] {Environment.GetEnvironmentVariable("windir"), "Microsoft.NET", (Is64BitProcess ? "Framework64" : "Framework"), version}.Aggregate(Path.Combine);
        }
        #endregion

        #region Command-line arguments
        // ReSharper disable ReturnTypeCanBeEnumerable.Global
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

        // ReSharper restore ReturnTypeCanBeEnumerable.Global
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

            // Create new or open existing mutex
            handle = UnsafeNativeMethods.CreateMutex(IntPtr.Zero, false, name);

            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case ErrorSuccess:
                    // New mutex created, handle passed out
                    return false;
                case ErrorAlreadyExists:
                    // Existing mutex opened, handle passed out
                    return true;
                case ErrorAccessDenied:
                    // Try to open existing mutex
                    handle = UnsafeNativeMethods.OpenMutex(Synchronize, false, name);

                    if (handle == IntPtr.Zero)
                    {
                        error = Marshal.GetLastWin32Error();
                        switch (error)
                        {
                            case ErrorFileNotFound:
                                // No existing mutex found
                                return false;
                            default:
                                throw new Win32Exception(error);
                        }
                    }

                    // Existing mutex opened, handle passed out
                    return true;
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

            // Try to open existing mutex
            var handle = UnsafeNativeMethods.OpenMutex(Synchronize, false, name);

            if (handle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                switch (error)
                {
                    case ErrorFileNotFound:
                        // No existing mutex found
                        return false;
                    default:
                        throw new Win32Exception(error);
                }
            }

            // Existing mutex opened, handle remains until process terminates
            return true;
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

            // Try to open existing mutex
            var handle = UnsafeNativeMethods.OpenMutex(Synchronize, false, name);

            if (handle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                switch (error)
                {
                    case ErrorFileNotFound:
                        // No existing mutex found
                        return false;
                    default:
                        throw new Win32Exception(error);
                }
            }

            // Existing mutex opened, close handle again
            UnsafeNativeMethods.CloseHandle(handle);
            return true;
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
            UnsafeNativeMethods.SetForegroundWindow(form.Handle);
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
        // ReSharper disable InconsistentNaming
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

        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

        /// <summary>
        /// Informs all GUI applications that changes where made to the environment variables (e.g. PATH) and that they should re-pull them.
        /// </summary>
        public static void NotifyEnvironmentChanged()
        {
            if (!IsWindows) return;

            const int WM_SETTINGCHANGE = 0x001A;
            const int SMTO_ABORTIFHUNG = 0x0002;
            IntPtr result;
            UnsafeNativeMethods.SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 5000, out result);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #region Window messages
        /// <summary>
        /// Adds a UAC shield icon to a button. Does nothing if not running Windows Vista or newer.
        /// </summary>
        /// <remarks>This is purely cosmetic. UAC elevation is a separate concern.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Native API only applies to buttons.")]
        public static void AddShieldIcon(Button button)
        {
            #region Sanity checks
            if (button == null) throw new ArgumentNullException("button");
            #endregion

            // ReSharper disable InconsistentNaming
            const int BCM_FIRST = 0x1600, BCM_SETSHIELD = 0x000C;
            // ReSharper restore InconsistentNaming

            if (!IsWindowsVista) return;
            button.FlatStyle = FlatStyle.System;
            UnsafeNativeMethods.SendMessage(button.Handle, BCM_FIRST + BCM_SETSHIELD, IntPtr.Zero, new IntPtr(1));
        }

        /// <summary>
        /// Registers a new message type that can be sent to windows.
        /// </summary>
        /// <param name="message">A unique string used to identify the message type session-wide.</param>
        /// <returns>A unique ID number used to identify the message type session-wide.</returns>
        public static int RegisterWindowMessage(string message)
        {
            return IsWindows ? UnsafeNativeMethods.RegisterWindowMessage(message) : 0;
        }

        /// <summary>
        /// Sends a message of a specific type to all windows in the current session.
        /// </summary>
        /// <param name="messageID">A unique ID number used to identify the message type session-wide.</param>
        public static void BroadcastMessage(int messageID)
        {
            if (IsWindows) UnsafeNativeMethods.PostMessage(HWND_BROADCAST, messageID, IntPtr.Zero, IntPtr.Zero);
        }
        #endregion

        #region Filesystem
        /// <summary>
        /// Creates a symbolic link for a file or directory.
        /// </summary>
        /// <param name="source">The path of the link to create.</param>
        /// <param name="target">The path of the existing file or directory to point to (relative to <paramref name="source"/>).</param>
        /// <remarks>Only available on Windows Vista or newer.</remarks>
        /// <exception cref="Win32Exception">Thrown if the symbolic link creation failed.</exception>
        public static void CreateSymlink(string source, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            if (!IsWindowsVista) throw new NotSupportedException(Resources.OnlyAvailableOnWindows);

            string targetAbsolute = Path.Combine(Path.GetDirectoryName(source) ?? Environment.CurrentDirectory, target);
            int retval = UnsafeNativeMethods.CreateSymbolicLink(source, target, Directory.Exists(targetAbsolute) ? 1 : 0);
            if (retval != 1) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Creates a hard link between two files.
        /// </summary>
        /// <param name="source">The path of the link to create.</param>
        /// <param name="target">The absolute path of the existing file to point to.</param>
        /// <remarks>Only available on Windows 2000 or newer.</remarks>
        /// <exception cref="Win32Exception">Thrown if the hard link creation failed.</exception>
        public static void CreateHardlink(string source, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            if (!IsWindowsNT) throw new NotSupportedException(Resources.OnlyAvailableOnWindows);
            if (!UnsafeNativeMethods.CreateHardLink(source, target, IntPtr.Zero)) throw new Win32Exception();
        }

        /// <summary>
        /// Determines whether to files are hardlinked.
        /// </summary>
        /// <param name="path1">The path of the first file.</param>
        /// <param name="path2">The path of the second file.</param>
        /// <remarks>Only available on Windows 2000 or newer.</remarks>
        public static bool AreHardlinked(string path1, string path2)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path1)) throw new ArgumentNullException("path1");
            if (string.IsNullOrEmpty(path2)) throw new ArgumentNullException("path2");
            #endregion

            if (!IsWindowsNT) throw new NotSupportedException(Resources.OnlyAvailableOnWindows);
            return GetFileIndex(path1) == GetFileIndex(path2);
        }

        private static ulong GetFileIndex(string path)
        {
            var handle = UnsafeNativeMethods.CreateFile(path, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Archive, IntPtr.Zero);
            if (handle == IntPtr.Zero) throw new Win32Exception();

            try
            {
                UnsafeNativeMethods.BY_HANDLE_FILE_INFORMATION fileInfo;
                if (!UnsafeNativeMethods.GetFileInformationByHandle(handle, out fileInfo)) throw new Win32Exception();
                return fileInfo.FileIndexLow + (fileInfo.FileIndexHigh << 32);
            }
            finally
            {
                UnsafeNativeMethods.CloseHandle(handle);
            }
        }
        #endregion
    }
}
