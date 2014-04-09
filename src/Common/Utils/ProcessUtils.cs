/*
 * Copyright 2006-2014 Bastian Eicher, Simon E. Silva Lauinger
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
using System.IO;
using System.Threading;
using NanoByte.Common.Properties;
using NanoByte.Common.Storage;

namespace NanoByte.Common.Utils
{
    /// <summary>
    /// Provides methods for launching child processes in OS-specific ways.
    /// </summary>
    public static class ProcessUtils
    {
        #region Assemblys
        /// <summary>
        /// Launches a .NET assembly located in the application's base directory.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly; may be <see langword="null"/>.</param>
        /// <returns>The newly created process.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the assembly could not be located.</exception>
        /// <exception cref="Win32Exception">Thrown if there was a problem launching the assembly.</exception>
        public static Process LaunchAssembly(string assembly, string arguments = null)
        {
            return Process.Start(CreateAssemblyStartInfo(assembly, arguments));
        }

        /// <summary>
        /// Launches a .NET assembly located in the application's base directory and waits for it to exit.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly; may be <see langword="null"/>.</param>
        /// <returns>The exit code of the target process.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the assembly could not be located.</exception>
        public static int RunAssembly(string assembly, string arguments = null)
        {
            try
            {
                var process = Process.Start(CreateAssemblyStartInfo(assembly, arguments));
                process.WaitForExit();
                return process.ExitCode;
            }
            catch (Win32Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// Launches a .NET assembly located in the application's base directory as an administrator (using UAC).
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly; may be <see langword="null"/>.</param>
        /// <returns>The newly created process.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the assembly could not be located.</exception>
        /// <exception cref="Win32Exception">Thrown if there was a problem launching the assembly.</exception>
        public static Process LaunchAssemblyAsAdmin(string assembly, string arguments = null)
        {
            return Process.Start(CreateAssemblyStartInfo(assembly, arguments, admin: true));
        }

        /// <summary>
        /// Launches a .NET assembly located in the application's base directory as an administrator (using UAC) and waits for it to exit.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        /// <returns>The exit code of the target process.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the assembly could not be located.</exception>
        public static int RunAssemblyAsAdmin(string assembly, string arguments = null)
        {
            try
            {
                var process = Process.Start(CreateAssemblyStartInfo(assembly, arguments, admin: true));
                process.WaitForExit();
                return process.ExitCode;
            }
            catch (Win32Exception)
            {
                return 1;
            }
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Creates a <see cref="ProcessStartInfo"/> for launching a .NET assembly located in the application's base directory.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        /// <param name="admin"><see langword="true"/> to use UAC to elevate to admin.</param>
        private static ProcessStartInfo CreateAssemblyStartInfo(string assembly, string arguments, bool admin = false)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(assembly)) throw new ArgumentNullException("assembly");
            #endregion

            string appPath = Path.Combine(Locations.InstallBase, assembly + ".exe");
            if (!File.Exists(appPath)) throw new FileNotFoundException(string.Format(Resources.UnableToLocateAssembly, assembly), appPath);

            // Only Windows can directly launch .NET executables, other platforms must run through Mono
            var startInfo = WindowsUtils.IsWindows
                ? new ProcessStartInfo(appPath, arguments)
                : new ProcessStartInfo("mono", appPath.EscapeArgument() + " " + arguments);

            if (admin && WindowsUtils.IsWindowsNT) startInfo.Verb = "runas";
            return startInfo;
        }
        #endregion

        #region Thread
        /// <summary>
        /// Starts executing a delegate in a new thread suitable for <see cref="System.Windows.Forms"/>.
        /// </summary>
        /// <param name="execute">The delegate to execute.</param>
        /// <param name="name">A short name for the new thread.</param>
        /// <returns>The newly launched thread.</returns>
        public static Thread RunAsync(ThreadStart execute, string name = null)
        {
            #region Sanity checks
            if (execute == null) throw new ArgumentNullException("execute");
            #endregion

            var thread = new Thread(execute) {Name = name};
            thread.SetApartmentState(ApartmentState.STA); // Make COM work
            thread.Start();
            return thread;
        }

        /// <summary>
        /// Starts executing a delegate in a new background thread (automatically terminated when application exits).
        /// </summary>
        /// <param name="execute">The delegate to execute.</param>
        /// <param name="name">A short name for the new thread.</param>
        /// <returns>The newly launched thread.</returns>
        public static Thread RunBackground(ThreadStart execute, string name = null)
        {
            #region Sanity checks
            if (execute == null) throw new ArgumentNullException("execute");
            #endregion

            var thread = new Thread(execute) {Name = name, IsBackground = true};
            thread.Start();
            return thread;
        }
        #endregion
    }
}
