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
using System.Windows.Forms;

namespace Common
{
    #region Enumerations
    /// <seealso cref="IProgress.State"/>
    public enum ProgressState
    {
        /// <summary>The task is ready to begin.</summary>
        Ready,
        /// <summary>The thread has just been started.</summary>
        Started,
        /// <summary>Handling the header.</summary>
        Header,
        /// <summary>Handling the actual data.</summary>
        Data,
        /// <summary>The task has been completed sucessfully.</summary>
        Complete,
        /// <summary>An error occured during the task.</summary>
        WebError,
        /// <summary>An error occured while writing the file.</summary>
        IOError
    }
    #endregion

    #region Delegates
    /// <summary>
    /// Delegate for handling an event concerning a specific <see cref="IProgress"/> instance.
    /// </summary>
    public delegate void ProgressEventHandler(IProgress sender);
    #endregion

    /// <summary>
    /// A background task that can report its progess via events and that can be cancelled.
    /// </summary>
    public interface IProgress : INamed
    {
        #region Events
        /// <summary>
        /// Occurs whenever <see cref="State"/> changes.
        /// </summary>
        /// <remarks>
        ///   <para>This event is raised from a background thread. Wrap via <see cref="Control.Invoke(System.Delegate)"/> to update UI elements.</para>
        ///   <para>The event handling blocks the thread, therefore observers should handle the event quickly.</para>
        /// </remarks>
        event ProgressEventHandler StateChanged;

        /// <summary>
        /// Occurs whenever <see cref="Progress"/> changes.
        /// </summary>
        /// <remarks>
        ///   <para>This event is raised from a background thread. Wrap via <see cref="Control.Invoke(System.Delegate)"/> to update UI elements.</para>
        ///   <para>The event handling blocks the thread, therefore observers should handle the event quickly.</para>
        /// </remarks>
        event ProgressEventHandler ProgressChanged;
        #endregion

        #region Properties
        /// <summary>
        /// The current status of the task.
        /// </summary>
        [Description("The current status of the task.")]
        ProgressState State { get; }

        /// <summary>
        /// Contains an error description if <see cref="State"/> is set to <see cref="ProgressState.WebError"/> or <see cref="ProgressState.IOError"/>.
        /// </summary>
        [Description("Contains an error description if State is set to WebError or IOError.")]
        string ErrorMessage { get; }

        /// <summary>
        /// The number of bytes that have been processed so far.
        /// </summary>
        [Description("The number of bytes that have been processed so far.")]
        long BytesProcessed { get; }

        /// <summary>
        /// The total number of bytes that are to be processed; -1 for unknown.
        /// </summary>
        /// <remarks>If this value is set to -1 in the constructor, the size be automatically set after <see cref="ProgressState.Data"/> has been reached.</remarks>
        [Description("The total number of bytes that are to be processed; -1 for unknown.")]
        long BytesTotal { get; }

        /// <summary>
        /// The progress of the task as a value between 0 and 1; -1 when unknown.
        /// </summary>
        [Description("The progress of the task as a value between 0 and 1; -1 when unknown.")]
        double Progress { get; }
        #endregion

        //--------------------//

        #region Control
        /// <summary>
        /// Blocks until the task is completed or terminated.
        /// </summary>
        /// <remarks>Calling this on a not running task will return immediately.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if called while a synchronous task is running (launched via <see cref="RunSync"/>).</exception>
        void Join();

        /// <summary>
        /// Stops executing the task.
        /// </summary>
        /// <remarks>Calling this on a not running task will have no effect.</remarks>
        void Cancel();
        #endregion
    }
}
