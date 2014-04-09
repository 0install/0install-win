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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using NanoByte.Common.Properties;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Cli
{
    /// <summary>
    /// Provides an interface to an external command-line application controlled via arguments and stdin and monitored via stdout and stderr.
    /// </summary>
    public abstract class CliAppControl
    {
        /// <summary>
        /// The name of the application's binary (without a file extension).
        /// </summary>
        protected abstract string AppBinary { get; }

        /// <summary>
        /// Runs the external application, processes its output and waits until it has terminated.
        /// </summary>
        /// <param name="arguments">Command-line arguments to launch the application with.</param>
        /// <param name="inputCallback">Callback allow you to write to the application's stdin-stream right after startup; <see langword="null"/> for none.</param>
        /// <returns>The application's complete output to the stdout-stream.</returns>
        /// <exception cref="IOException">Thrown if the external application could not be launched.</exception>
        protected virtual string Execute(string arguments, Action<StreamWriter> inputCallback = null)        {
            Process process;
            try
            {
                process = Process.Start(GetStartInfo(arguments, hidden: true));
                if (process == null) return null;
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

            // Asynchronously buffer all stdout data
            var stdoutBuffer = new StringBuilder();
            var stdoutThread = ProcessUtils.RunBackground(() =>
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    // No locking since the data will only be read at the end
                    stdoutBuffer.AppendLine(process.StandardOutput.ReadLine());
                }
            }, name: "CliAppControl.stdout");

            // Asynchronously buffer all stderr messages
            var stderrList = new Queue<string>();
            var stderrThread = ProcessUtils.RunBackground(() =>
            {
                while (!process.StandardError.EndOfStream)
                {
                    // Locking for thread-safe producer-consumer-behaviour
                    var data = process.StandardError.ReadLine();
                    if (!string.IsNullOrEmpty(data))
                        lock (stderrList) stderrList.Enqueue(data);
                }
            }, name: "CliAppControl.stderr");

            // Use callback to send data into external process
            if (inputCallback != null) inputCallback(process.StandardInput);

            // Start handling messages to stderr
            do
            {
                // Locking for thread-safe producer-consumer-behaviour
                lock (stderrList)
                {
                    while (stderrList.Count > 0)
                    {
                        string result = HandleStderr(stderrList.Dequeue());
                        if (!string.IsNullOrEmpty(result)) process.StandardInput.WriteLine(result);
                    }
                }
            } while (!process.WaitForExit(50));

            // Finish any pending async operations
            stdoutThread.Join();
            stderrThread.Join();
            process.Close();

            // Handle any left over stderr messages
            while (stderrList.Count > 0)
            {
                string result = HandleStderr(stderrList.Dequeue());
                if (!string.IsNullOrEmpty(result)) process.StandardInput.WriteLine(result);
            }

            return stdoutBuffer.ToString();
        }

        /// <summary>
        /// Creates the <see cref="ProcessStartInfo"/> used by <see cref="Execute"/> to launch the external application.
        /// </summary>
        /// <param name="arguments">The arguments to pass to the process at startup.</param>
        /// <param name="hidden">Set to <see langword="true"/> to show no window and redirect all input and output for the process.</param>
        protected virtual ProcessStartInfo GetStartInfo(string arguments, bool hidden = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = AppBinary,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = hidden,
                RedirectStandardInput = hidden,
                RedirectStandardOutput = hidden,
                RedirectStandardError = hidden,
                ErrorDialog = false,
            };

            // Suppress localization to enable programatic parsing of output
            if (hidden) startInfo.EnvironmentVariables["LANG"] = "C";

            return startInfo;
        }

        /// <summary>
        /// A hook method for handling stderr messages from the CLI application.
        /// </summary>
        /// <param name="line">The error line written to stderr.</param>
        /// <returns>The response to write to stdin; <see langword="null"/> for none.</returns>
        protected virtual string HandleStderr(string line)
        {
            Log.Warn(line);
            return null;
        }
    }
}
