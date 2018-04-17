// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

namespace ZeroInstall.Hooking
{
    internal static partial class WindowsUtils
    {
        /// <summary>
        /// <c>true</c> if the current operating system is Windows (9x- or NT-based); <c>false</c> otherwise.
        /// </summary>
        public static bool IsWindows => (Environment.OSVersion.Platform == PlatformID.Win32Windows) || (Environment.OSVersion.Platform == PlatformID.Win32NT);

        /// <summary>
        /// <c>true</c> if the current operating system is a modern Windows version (NT-based); <c>false</c> otherwise.
        /// </summary>
        public static bool IsWindowsNT => (Environment.OSVersion.Platform == PlatformID.Win32NT);

        /// <summary>
        /// <c>true</c> if the current operating system is Windows Vista or newer; <c>false</c> otherwise.
        /// </summary>
        public static bool IsWindowsVista => IsWindowsNT && (Environment.OSVersion.Version >= new Version(6, 0));

        /// <summary>
        /// <c>true</c> if the current operating system is Windows 7 or newer; <c>false</c> otherwise.
        /// </summary>
        public static bool IsWindows7 => IsWindowsNT && (Environment.OSVersion.Version >= new Version(6, 1));

        /// <summary>
        /// <c>true</c> if the current operating system is Windows 8 or newer; <c>false</c> otherwise.
        /// </summary>
        public static bool IsWindows8 => IsWindowsNT && (Environment.OSVersion.Version >= new Version(6, 2));

        /// <summary>
        /// <c>true</c> if the current operating system is 64-bit capable; <c>false</c> otherwise.
        /// </summary>
        public static bool Is64BitOperatingSystem => (Is64BitProcess || Is32BitProcessOn64BitOperatingSystem);

        /// <summary>
        /// <c>true</c> if the current process is 64-bit; <c>false</c> otherwise.
        /// </summary>
        public static bool Is64BitProcess => (IntPtr.Size == 8);

        /// <summary>
        /// <c>true</c> if the current process is 32-bit but the operating system is 64-bit capable; <c>false</c> otherwise.
        /// </summary>
        /// <remarks>Can only detect WOW on Windows XP and newer</remarks>
        public static bool Is32BitProcessOn64BitOperatingSystem
        {
            get
            {
                // Can only detect WOW on Windows XP or newer
                if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version < new Version(5, 1)) return false;

                SafeNativeMethods.IsWow64Process(Process.GetCurrentProcess().Handle, out bool retVal);
                return retVal;
            }
        }

        /// <summary>
        /// Indicates whether the current user is an administrator. Always returns <c>true</c> on non-Windows systems.
        /// </summary>
        public static bool IsAdministrator
        {
            get
            {
                if (!IsWindowsNT) return true;
                var identity = WindowsIdentity.GetCurrent();
                if (identity == null) return true;

                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Determines the path of the executable the current process was launched from.
        /// </summary>
        public static string CurrentProcessPath
        {
            get
            {
                var fileName = new StringBuilder(255);
                SafeNativeMethods.GetModuleFileName(IntPtr.Zero, fileName, fileName.Capacity);
                return fileName.ToString();
            }
        }
    }
}
