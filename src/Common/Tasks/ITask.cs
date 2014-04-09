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
using System.ComponentModel;
using System.IO;
using System.Net;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// A task that can report its progess via events.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Runs the task and blocks until it is complete.
        /// </summary>
        /// <param name="cancellationToken">Used to signal when the user wishes to cancel the task execution.</param>
        /// <param name="progress">Used to report back the task's progress.</param>
        /// <exception cref="OperationCanceledException">Thrown if the task was canceled from another thread.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskStatus.IOError"/>.</exception>
        /// <exception cref="WebException">Thrown if the task ended with <see cref="TaskStatus.WebError"/>.</exception>
        /// <remarks>Even though the task runs synchronously it may be still executed on a separate thread so it can be canceled from other threads.</remarks>
        void Run(CancellationToken cancellationToken = default(CancellationToken), IProgress<TaskSnapshot> progress = null);

        /// <summary>
        /// A name describing the task in human-readable form.
        /// </summary>
        [Description("A name describing the task in human-readable form.")]
        string Name { get; }

        /// <summary>
        /// An object used to associate the task with a specific process; may be <see langword="null"/>.
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// Indicates whether this task can be canceled once it has been started.
        /// </summary>
        [Description("Indicates whether this task can be canceled once it has been started.")]
        bool CanCancel { get; }
    }
}
