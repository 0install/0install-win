/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

namespace ZeroInstall.Hooking
{
    static partial class WindowsUtils
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
