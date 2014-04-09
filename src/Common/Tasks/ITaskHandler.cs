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
using System.IO;
using System.Net;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Callback methods to inform the user about <see cref="ITask"/>s being run.
    /// </summary>
    /// <remarks>The methods may be called from a background thread. Implementations apply appropriate thread-synchronization to update UI elements.</remarks>
    public interface ITaskHandler : IDisposable
    {
        /// <summary>
        /// Used to signal when the user wishes to cancel the entire current process (and any <see cref="ITask"/>s it includes).
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Runs and tracks an <see cref="ITask"/>. Returns once the task has been completed.
        /// </summary>
        /// <param name="task">The task to be run. (<see cref="ITask.Run"/> or equivalent is called on it.)</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskStatus.IOError"/>.</exception>
        /// <exception cref="WebException">Thrown if the task ended with <see cref="TaskStatus.WebError"/>.</exception>
        /// <remarks>
        /// This may be called multiple times concurrently but the concurrent calls must not depend on each other!
        /// The specific implementation of this method may chose whether to actually run the tasks concurrently or in sequence.
        /// </remarks>
        void RunTask(ITask task);

        /// <summary>
        /// The detail level of messages printed to the console or log file.
        /// 0 = normal, 1 = verbose, 2 = very verbose
        /// </summary>
        int Verbosity { get; set; }
    }
}
