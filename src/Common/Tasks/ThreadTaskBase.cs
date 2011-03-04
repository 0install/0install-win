/*
 * Copyright 2006-2011 Bastian Eicher
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

using System.Threading;

namespace Common.Tasks
{
    /// <summary>
    /// Abstract base class for background tasks that implement <see cref="ITask"/> using an explicit background thread.
    /// </summary>
    public abstract class ThreadTaskBase : TaskBase
    {
        #region Variables
        /// <summary>Synchronization handle to prevent race conditions with thread startup/shutdown or <see cref="ITask.State"/> switching.</summary>
        protected readonly object StateLock = new object();

        /// <summary>The background thread used for executing the task. Sub-classes must initalize this member.</summary>
        protected readonly Thread Thread;
        #endregion

        #region Properties
        /// <inheritdoc />
        public override bool CanCancel { get { return true; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares a new background thread for executing a task.
        /// </summary>
        protected ThreadTaskBase()
        {
            // Prepare the background thread for later execution
            Thread = new Thread(RunTask);
        }
        #endregion

        //--------------------//

        #region Control
        /// <inheritdoc/>
        public override void Start()
        {
            lock (StateLock)
            {
                if (State != TaskState.Ready) return;

                State = TaskState.Started;
                Thread.Start();
            }
        }

        /// <inheritdoc />
        public override void Join()
        {
            lock (StateLock)
            {
                if (Thread == null || !Thread.IsAlive) return;
            }

            Thread.Join();
        }
        #endregion

        #region Thread code
        /// <summary>
        /// The actual code to be executed by a background thread.
        /// </summary>
        protected abstract void RunTask();
        #endregion
    }
}
