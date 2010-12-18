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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Common.Storage;
using Common.Utils;
using Common.Properties;

namespace Common.Cli
{
    #region Delegates
    /// <summary>
    /// A callback method for handling error messages from a CLI application.
    /// </summary>
    /// <param name="line">The error line written to stderr.</param>
    /// <returns>The response to write to stdin; <see langword="null"/> for none.</returns>
    public delegate string CliErrorHandler(string line);
    #endregion

    /// <summary>
    /// Provides an interface to an external command-line application controlled via arguments and stdin and monitored via stdout and stderr.
    /// </summary>
    public abstract class CliAppControl
    {
        #region Properties
        /// <summary>
        /// The name of the application to be executed. This also determines the name of the directory that is searched for a bundled version on Windows.
        /// </summary>
        protected abstract string AppName { get; }

        /// <summary>
        /// The name of the application's binary (without a file ending).
        /// </summary>
        protected abstract string AppBinary { get; }

        /// <summary>
        /// The directory to search for bundled versions of applications.
        /// </summary>
        /// <remarks>
        /// If a sub-directory named like <see cref="AppName"/> is found in the installation directory this is used.
        /// Otherwise a parallel directory named "Bundled" is probed.
        /// </remarks>
        protected string BundledDirectory
        {
            get
            {
                // Use the base directory of the launching application since the current directory may be arbitrary
                string searchBase = Locations.PortableBase;

                if (Directory.Exists(Path.Combine(searchBase, AppName))) return searchBase;
                return Path.Combine(Path.Combine(Path.Combine(searchBase, ".."), ".."), "Bundled");
            }
        }

        /// <summary>
        /// The directory containing the bundled version of the application. This is generally a sub-directory of <see cref="BundledDirectory"/>.
        /// </summary>
        protected string AppDirectory
        {
            get { return Path.Combine(BundledDirectory, AppName); }
        }
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Runs the external application, processes its output and waits until it has terminated.
        /// </summary>
        /// <param name="arguments">Command-line arguments to launch the application with.</param>
        /// <param name="defaultInput">Data to write to the application's stdin-stream right after startup; <see langword="null"/> for none.</param>
        /// <param name="errorHandler">A callback method to call whenever something is written to the stdout-stream and possibly to respond to it; <see langword="null"/> for none.</param>
        /// <returns>The application's complete output to the stdout-stream.</returns>
        /// <exception cref="IOException">Thrown if the external application could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if there was output to stderr and <paramref name="errorHandler"/> was <see langword="null"/>.</exception>
        protected string Execute(string arguments, string defaultInput, CliErrorHandler errorHandler)
        {
            Process process;
            try { process = ProcessUtils.RunAsync(GetStartInfo(arguments)); }
            #region Error handling
            catch (Win32Exception ex)
            {
                throw new IOException(string.Format(Resources.UnableToLaunchBundled, AppName), ex);
            }
            catch (BadImageFormatException ex)
            {
                throw new IOException(string.Format(Resources.UnableToLaunchBundled, AppName), ex);
            }
            #endregion

            // Asynchronously buffer all StandardOutput data
            var outputBuffer = new StringBuilder();
            // No locking since the data will only be read at the end
            process.OutputDataReceived += (sender, e) => outputBuffer.AppendLine(e.Data);

            // Asynchronously buffer all StandardError messages
            var errorList = new Queue<string>();
            // Locking for thread-safe producer-consumer-behaviour
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                lock (errorList) errorList.Enqueue(e.Data);
            };

            // Start async read threads
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Write the default input first off
            if (!string.IsNullOrEmpty(defaultInput)) process.StandardInput.WriteLine(defaultInput);

            if (errorHandler == null) process.WaitForExit();
            else
            {
                do
                {
                    // Handle messages to StandardError synchronously
                    lock (errorList)
                    {
                        while (errorList.Count > 0)
                        {
                            string result = errorHandler(errorList.Dequeue());
                            if (!string.IsNullOrEmpty(result)) process.StandardInput.WriteLine(result);
                        }
                    }
                } while (!process.WaitForExit(50));
            }

            // HACK: Some extra time to finish any pending async operations
            Thread.Sleep(100);

            if (errorHandler != null)
            {
                lock (errorList)
                {
                    while (errorList.Count > 0)
                    {
                        string result = errorHandler(errorList.Dequeue());
                        if (!string.IsNullOrEmpty(result)) process.StandardInput.WriteLine(result);
                    }
                }
            }

            if (errorList.Count > 0)
                throw new UnhandledErrorsException(StringUtils.Concatenate(errorList, "\n"));

            return outputBuffer.ToString();
        }
        #endregion

        #region Start info
        /// <summary>
        /// Creates the <see cref="ProcessStartInfo"/> used by <see cref="Execute"/> to launch the external application.
        /// </summary>
        protected virtual ProcessStartInfo GetStartInfo(string arguments)
        {
            // Try to use bundled version of the application when running on Windows
            bool useBundled = WindowsUtils.IsWindows && File.Exists(Path.Combine(AppDirectory, AppBinary + ".exe"));

            var startInfo = new ProcessStartInfo
            {
                FileName = (useBundled ? Path.Combine(AppDirectory, AppBinary) : AppBinary),
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                ErrorDialog = false
            };

            // Add bundled application directory to search path for locating DLLs
            if (useBundled)
                startInfo.EnvironmentVariables["PATH"] = AppDirectory + Path.PathSeparator + startInfo.EnvironmentVariables["PATH"];

            return startInfo;
        }
        #endregion
    }
}
