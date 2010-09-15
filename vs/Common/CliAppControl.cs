/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.Text;
using Common.Helpers;
using Common.Properties;

namespace Common
{
    /// <summary>
    /// Provides an interface to an external command-line tool controlled via arguments and stdin and monitored via stdout and stderr.
    /// </summary>
    public abstract class CliAppControl
    {
        #region Properties
        /// <summary>
        /// The name of the application to be executed. This also determines the name of the directory that is searched for a portable version.
        /// </summary>
        protected abstract string AppName { get; }

        /// <summary>
        /// The name of the application's binary.
        /// </summary>
        protected abstract string AppBinaryName { get; }

        /// <summary>
        /// The directory to search for portable versions of applications.
        /// </summary>
        /// <remarks>
        /// If a sub-directory named like <see cref="AppName"/> is found in the installation directory this is used.
        /// Otherwise a parallel directory named "Portable" is probed.
        /// </remarks>
        protected string PortableDirectory
        {
            get
            {
#if DEBUG
                // Use the current directory since the launching application might be a test runner in another directory
                string searchBase = Environment.CurrentDirectory;
#else
                // Use the base directory of the launching application since the current directory may be arbitrary
                string searchBase = AppDomain.CurrentDomain.BaseDirectory;
#endif

                if (Directory.Exists(Path.Combine(searchBase, AppName))) return searchBase;
                return Path.Combine(Path.Combine(Path.Combine(searchBase, ".."), ".."), "Portable");
            }
        }

        /// <summary>
        /// The directory containing the portable version of the application. This is generally a sub-directory of <see cref="PortableDirectory"/>.
        /// </summary>
        protected string AppDirectory
        {
            get { return Path.Combine(PortableDirectory, AppName); }
        }

        /// <summary>
        /// The path to the binary of the application to be executed.
        /// </summary>
        protected string AppBinary
        {
            get
            {
                // Use portable version of application on Windows NT and a native version on all other OSes
                return Environment.OSVersion.Platform == PlatformID.Win32NT ? Path.Combine(AppDirectory, AppBinaryName + ".exe") : AppBinaryName;
            }
        }
        #endregion

        //--------------------//

        protected string Execute(string arguments, DataReceivedEventHandler errorHandler, SimpleResult<string> waitLoop)
        {
            var process = new Process { StartInfo = GetStartInfo(arguments) };

            // Asynchronously buffer all StandardOutput data
            var outputBuffer = new StringBuilder();
            process.OutputDataReceived += (sender, e) => outputBuffer.AppendLine(e.Data);

            if (errorHandler != null) process.ErrorDataReceived += errorHandler;

            try { process.Start(); }
            #region Error handling
            catch (Win32Exception ex)
            {
                throw new IOException(string.Format(Resources.UnableToLaunchBundled, AppName), ex);
            }
            #endregion

            // Start async read threads
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (!process.WaitForExit(50))
            {
                if (waitLoop != null)
                {
                    string write = waitLoop();
                    if (write != null)
                    {
                        process.StandardInput.WriteLine(write);
                        process.StandardInput.Flush();
                    }
                }
            }

            return outputBuffer.ToString();
        }

        protected virtual ProcessStartInfo GetStartInfo(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = AppBinary,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            // Make sure additional files of the portable application can be located
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                startInfo.EnvironmentVariables["PATH"] = AppDirectory + Path.PathSeparator + startInfo.EnvironmentVariables["PATH"];

            return startInfo;
        }
    }
}
