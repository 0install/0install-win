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
using System.IO;

namespace Common.Tasks
{
    /// <summary>
    /// Callback methods to inform the user about tasks being run.
    /// </summary>
    /// <remarks>The methods may be called from a background thread. Apply appropriate thread-synchronization to update UI elements.</remarks>
    public interface ITaskHandler
    {
        /// <summary>
        /// Do not show progress reports, questions or messages (except for non-intrusive background messages like tray icons) unless a critical error occurs.
        /// </summary>
        bool Batch { get; set; }

        /// <summary>
        /// Runs (and potentially tracks) an <see cref="ITask"/>. Returns once the task has been completed.
        /// </summary>
        /// <param name="task">The task to be run. (<see cref="ITask.RunSync"/> or equivalent is called on it.)</param>
        /// <param name="tag">An object used to associate the <paramref name="task"/> with a specific process; may be <see langword="null"/>.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskState.IOError"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ITask.State"/> is not <see cref="TaskState.Ready"/>.</exception>
        /// <remarks>
        /// This may be called multiple times concurrently but the concurrent calls must not depend on each other! The specific implementation of this method may chose whether to actually run the tasks concurrently or in sequence.
        /// There should not be more than one <see cref="ITask"/> running with a specific <paramref name="tag"/> at any given time.
        /// </remarks>
        void RunTask(ITask task, object tag);
    }
}
