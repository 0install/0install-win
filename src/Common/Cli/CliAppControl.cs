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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
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
        /// The name of the application's binary (without a file extension).
        /// </summary>
        protected abstract string AppBinary { get; }
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Runs the external application, processes its output and waits until it has terminated.
        /// </summary>
        /// <param name="arguments">Command-line arguments to launch the application with.</param>
        /// <param name="inputCallback">Callback allow you to write to the application's stdin-stream right after startup; <see langword="null"/> for none.</param>
        /// <param name="errorHandler">A callback method to call whenever something is written to the stdout-stream and possibly to respond to it; <see langword="null"/> for none.</param>
        /// <returns>The application's complete output to the stdout-stream.</returns>
        /// <exception cref="IOException">Thrown if the external application could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if there was output to stderr and <paramref name="errorHandler"/> was <see langword="null"/>.</exception>
        protected string Execute(string arguments, Action<StreamWriter> inputCallback, CliErrorHandler errorHandler)
        {
            Process process;
            try
            {
                process = Process.Start(GetStartInfo(arguments));
            }
                #region Error handling
            catch (Win32Exception ex)
            {
                throw new IOException(string.Format(Resources.UnableToLaunchBundled, AppBinary), ex);
            }
            catch (BadImageFormatException ex)
            {
                throw new IOException(string.Format(Resources.UnableToLaunchBundled, AppBinary), ex);
            }
            #endregion

            // Asynchronously buffer all StandardOutput data
            var outputBuffer = new StringBuilder();
            var outputReadThread = new Thread(() =>
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    // No locking since the data will only be read at the end
                    outputBuffer.AppendLine(process.StandardOutput.ReadLine());
                }
            }) {IsBackground = true};
            outputReadThread.Start();

            // Asynchronously buffer all StandardError messages
            var errorList = new Queue<string>();
            var errorReadThread = new Thread(() =>
            {
                while (!process.StandardError.EndOfStream)
                {
                    // Locking for thread-safe producer-consumer-behaviour
                    var data = process.StandardError.ReadLine();
                    if (!string.IsNullOrEmpty(data))
                        lock (errorList) errorList.Enqueue(data);
                }
            }) {IsBackground = true};
            errorReadThread.Start();

            // Use callback to send data into external process
            if (inputCallback != null) inputCallback(process.StandardInput);

            // Start handling messages to StandardError
            if (errorHandler != null)
            {
                do
                {
                    // Locking for thread-safe producer-consumer-behaviour
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
            else process.WaitForExit();

            // Finish any pending async operations
            outputReadThread.Join();
            errorReadThread.Join();

            // Handle any left over StandardError messages
            if (errorHandler != null)
            {
                while (errorList.Count > 0)
                {
                    string result = errorHandler(errorList.Dequeue());
                    if (!string.IsNullOrEmpty(result)) process.StandardInput.WriteLine(result);
                }
            }

            if (errorList.Count > 0)
                throw new UnhandledErrorsException(StringUtils.Join("\n", errorList));
            return outputBuffer.ToString();
        }
        #endregion

        #region Start info
        /// <summary>
        /// Creates the <see cref="ProcessStartInfo"/> used by <see cref="Execute"/> to launch the external application.
        /// </summary>
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
                ErrorDialog = false
            };

            return startInfo;
        }
        #endregion
    }
}
