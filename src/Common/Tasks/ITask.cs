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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;

namespace Common.Tasks
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
        /// <exception cref="OperationCanceledException">Thrown if the task was canceled from another thread.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskState.IOError"/>.</exception>
        /// <exception cref="WebException">Thrown if the task ended with <see cref="TaskState.WebError"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="State"/> is not <see cref="TaskState.Ready"/>.</exception>
        /// <remarks>Even though the task runs synchronously it may be still executed on a separate thread so it can be canceled from other threads.</remarks>
        void Run(CancellationToken cancellationToken = default(CancellationToken));

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

        /// <summary>
        /// The current status of the task.
        /// </summary>
        [Description("The current status of the task.")]
        TaskState State { get; }

        /// <summary>
        /// Occurs whenever <see cref="State"/> changes.
        /// </summary>
        /// <remarks>
        ///   <para>This event is raised from a background thread. Wrap via synchronization context to update UI elements.</para>
        ///   <para>Handling this blocks the task, therefore observers should handle the event quickly.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        event Action<ITask> StateChanged;

        /// <summary>
        /// The number of units that have been processed so far.
        /// </summary>
        [Description("The number of bytes that have been processed so far.")]
        long UnitsProcessed { get; }

        /// <summary>
        /// The total number of units that are to be processed; -1 for unknown.
        /// </summary>
        /// <remarks>If this value is set to -1 in the constructor, the size be automatically set after <see cref="TaskState.Data"/> has been reached.</remarks>
        [Description("The total number of bytes that are to be processed; -1 for unknown.")]
        long UnitsTotal { get; }

        /// <summary>
        /// <see langword="true"/> if <see cref="UnitsProcessed"/> and <see cref="UnitsTotal"/> are measured in bytes;
        /// <see langword="false"/> if they are measured in generic units.
        /// </summary>
        bool UnitsByte { get; }

        /// <summary>
        /// The progress of the task as a value between 0 and 1; -1 when unknown.
        /// </summary>
        [Description("The progress of the task as a value between 0 and 1; -1 when unknown.")]
        double Progress { get; }

        /// <summary>
        /// Occurs whenever <see cref="Progress"/> changes.
        /// </summary>
        /// <remarks>
        ///   <para>This event is raised from a background thread. Wrap via synchronization context to update UI elements.</para>
        ///   <para>Handling this blocks the task, therefore observers should handle the event quickly.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        event Action<ITask> ProgressChanged;
    }
}
