/*
 * Copyright 2006-2011 Bastian Eicher, Simon E. Silva Lauinger
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
using Common.Properties;
using Common.Storage;

namespace Common.Utils
{
    /// <summary>
    /// Provides methods for launching child processes in OS-specific ways.
    /// </summary>
    public static class ProcessUtils
    {
        /// <summary>
        /// Attempts to launch a .NET helper assembly in the application's base directory.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        /// <returns>The newly created process.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the assembly could not be located.</exception>
        /// <exception cref="Win32Exception">Thrown if there was a problem launching the assembly.</exception>
        public static Process LaunchHelperAssembly(string assembly, string arguments)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(assembly)) throw new ArgumentNullException("assembly");
            #endregion

            string appPath = Path.Combine(Locations.InstallBase, assembly + ".exe");
            if (!File.Exists(appPath)) throw new FileNotFoundException(string.Format(Resources.UnableToLocateAssembly, assembly), appPath);

            // Only Windows can directly launch .NET executables, other platforms must run through Mono
            return Process.Start(WindowsUtils.IsWindows
                ? new ProcessStartInfo(appPath, arguments)
                : new ProcessStartInfo("mono", "\"" + appPath + "\" " + arguments));
        }

        /// <summary>
        /// Starts executing a delegate in a new thread suitable for <see cref="System.Windows.Forms"/>.
        /// </summary>
        /// <param name="execute">The delegate to execute.</param>
        public static void RunAsync(ThreadStart execute)
        {
            var thread = new Thread(execute);
            thread.SetApartmentState(ApartmentState.STA); // Make COM work
            thread.Start();
        }
    }
}
