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

using System;
using System.IO;
using System.Net;
using System.Threading;
using Common.Properties;

namespace Common
{
    /// <summary>
    /// Abstract base class for background tasks that implement <see cref="ITask"/> and can be canceled.
    /// </summary>
    public abstract class TaskBase : MarshalByRefObject, ITask
    {
        #region Events
        /// <inheritdoc />
        public event TaskEventHandler StateChanged;

        private void OnStateChanged()
        {
            // Copy to local variable to prevent threading issues
            TaskEventHandler stateChanged = StateChanged;
            if (stateChanged != null) stateChanged(this);
        }

        /// <inheritdoc />
        public event TaskEventHandler ProgressChanged;

        private void OnProgressChanged()
        {
            // Copy to local variable to prevent threading issues
            TaskEventHandler progressChanged = ProgressChanged;
            if (progressChanged != null) progressChanged(this);
        }
        #endregion

        #region Variables
        /// <summary>Synchronization handle to prevent race conditions with thread startup/shutdown or <see cref="State"/> switching.</summary>
        protected readonly object StateLock = new object();

        /// <summary>The background thread used for executing the task. Sub-classes must initalize this member.</summary>
        protected readonly Thread Thread;
        #endregion

        #region Properties
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public bool CanCancel { get { return true; } }

        private TaskState _state;
        /// <inheritdoc />
        public TaskState State
        {
            get { return _state; } protected set { UpdateHelper.Do(ref _state, value, OnStateChanged); }
        }

        /// <inheritdoc />
        public string ErrorMessage { get; protected set; }

        private long _bytesReceived;
        /// <inheritdoc />
        public long BytesProcessed
        {
            get { return _bytesReceived; } protected set { UpdateHelper.Do(ref _bytesReceived, value, OnProgressChanged); }
        }

        private long _bytesTotal;
        /// <inheritdoc />
        public long BytesTotal
        {
            get { return _bytesTotal; }
            protected set { UpdateHelper.Do(ref _bytesTotal, value, OnProgressChanged); }
        }

        /// <inheritdoc />
        public double Progress
        {
            get
            {
                switch (BytesTotal)
                {
                    case -1: return -1;
                    case 0: return 1;
                    default: return BytesProcessed / (double)BytesTotal;
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares a new background thread for executing a task.
        /// </summary>
        protected TaskBase()
        {
            // Prepare the background thread for later execution
            Thread = new Thread(RunTask);
        }
        #endregion

        //--------------------//

        #region Control
        /// <inheritdoc/>
        public void Start()
        {
            lock (StateLock)
            {
                if (State != TaskState.Ready) return;

                State = TaskState.Started;
                Thread.Start();
            }
        }

        /// <inheritdoc/>
        public void RunSync()
        {
            // Still use threads so cancel request from other threads will work
            lock (StateLock)
            {
                if (State != TaskState.Ready) throw new InvalidOperationException(Resources.StateMustBeReady);

                State = TaskState.Started;
                Thread.Start();
            }

            Thread.Join();

            lock (StateLock)
            {
                switch (State)
                {
                    case TaskState.Complete:
                        return;

                    case TaskState.WebError:
                        State = TaskState.Ready;
                        throw new WebException(ErrorMessage);

                    case TaskState.IOError:
                        State = TaskState.Ready;
                        throw new IOException(ErrorMessage);

                    default:
                        State = TaskState.Ready;
                        throw new UserCancelException();
                }
            }
        }

        /// <inheritdoc />
        public void Join()
        {
            lock (StateLock)
            {
                if (Thread == null || !Thread.IsAlive) return;
            }

            Thread.Join();
        }

        /// <inheritdoc />
        public abstract void Cancel();
        #endregion

        #region Thread code
        /// <summary>
        /// The actual code to be executed by a background thread.
        /// </summary>
        protected abstract void RunTask();
        #endregion
    }
}
