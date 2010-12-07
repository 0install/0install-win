/*
 * Copyright 2006-2010 Bastian Eicher, Simon E. Silva Lauinger
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
using Common.Properties;
using Common.Storage;

namespace Common.Utils
{
    /// <summary>
    /// Provides methods for launching child processes in OS-specific ways.
    /// </summary>
    public static class ProcessUtils
    {
        #region Run
        /// <summary>
        /// Runs the specified program and waits for it to exit. Terminating this process (the parent) may also terminate the new process (the child).
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static void RunSync(ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            Process.Start(startInfo).WaitForExit();
        }

        /// <summary>
        /// Starts the specified program and immediately returns. Terminating this process (the parent) may also terminate the new process (the child).
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <returns>A handle to the newly launched process.</returns>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static Process RunAsync(ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            return Process.Start(startInfo);
        }

        /// <summary>
        /// Starts the specified program and immediately returns. Terminating this process (the parent) will not affect the new process (the child).
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static void RunDetached(ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            // On Unix-like systems using an external launch helper is required to detach the child process from the parent
            if (MonoUtils.IsUnix) startInfo.UseShellExecute = true;

            RunAsync(startInfo);
        }

        /// <summary>
        /// On Windows runs the specified program and waits for it to exit. On Unix-like systems replaces the currently executing process with a new one.
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        /// <exception cref="IOException">Thrown if the process could not be replaced.</exception>
        /// <remarks>This method may not return on success. Warning: Any concurrent threads may be terminated!</remarks>
        public static void RunReplace(ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            if (MonoUtils.IsUnix && !startInfo.UseShellExecute)
            {
                if (!string.IsNullOrEmpty(startInfo.WorkingDirectory)) Environment.CurrentDirectory = startInfo.WorkingDirectory;
                MonoUtils.ProcessReplace(startInfo.FileName, startInfo.Arguments, startInfo.EnvironmentVariables);
            }
            else RunSync(startInfo);
        }
        #endregion

        #region Helper applications
        /// <summary>
        /// Attempts to launch a .NET helper assembly in the application's base directory.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file ending).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        /// <exception cref="FileNotFoundException">Thrown if the assembly could not be located.</exception>
        /// <exception cref="Win32Exception">Thrown if there was a problem launching the assembly.</exception>
        public static void LaunchHelperAssembly(string assembly, string arguments)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(assembly)) throw new ArgumentNullException("assembly");
            #endregion

            string appPath = Path.Combine(Locations.PortableBase, assembly + ".exe");
            if (!File.Exists(appPath)) throw new FileNotFoundException(string.Format(Resources.UnableToLocateAssembly, assembly), appPath);

            // Only Windows can directly launch .NET executables, other platforms must run through Mono
            RunDetached(WindowsUtils.IsWindows
                ? new ProcessStartInfo(appPath, arguments)
                : new ProcessStartInfo("mono", "\"" + appPath + "\" " + arguments));
        }
        #endregion
    }
}
