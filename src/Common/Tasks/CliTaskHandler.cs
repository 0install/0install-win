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
using Common.Cli;

namespace Common.Tasks
{
    /// <summary>
    /// Uses the stderr stream to inform the user about the progress of tasks.
    /// </summary>
    public class CliTaskHandler : MarshalByRefObject, ITaskHandler
    {
        private readonly CancellationToken _cancellationToken = new CancellationToken();

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return _cancellationToken; } }

        /// <summary>Synchronization object used to prevent multiple concurrent <see cref="ITask"/>s.</summary>
        private readonly object _taskLock = new object();

        /// <inheritdoc />
        public void RunTask(ITask task, object tag)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            lock (_taskLock) // Prevent multiple concurrent tasks
            {
                Log.Info(task.Name + "...");
                using (new TrackingProgressBar(task))
                    task.RunSync(_cancellationToken);
            }
        }
    }
}
