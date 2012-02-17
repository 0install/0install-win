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
using System.IO;
using System.Net;
using System.Threading;

namespace Common.Tasks
{
    /// <summary>
    /// Abstract base class for background tasks that implement <see cref="ITask"/> using a <see cref="Thread"/>.
    /// </summary>
    public abstract class ThreadTask : MarshalByRefObject, ITask
    {
        #region Variables
        /// <summary>Flag that indicates the current process should be canceled.</summary>
        protected volatile bool CancelRequest;

        /// <summary>Synchronization handle to prevent race conditions with thread startup/shutdown or <see cref="ITask.State"/> switching.</summary>
        protected readonly object StateLock = new object();

        /// <summary>The background thread used for executing the task. Sub-classes must initalize this member.</summary>
        protected readonly Thread Thread;
        #endregion

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

        #region Properties
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public virtual bool CanCancel { get { return true; } }

        private TaskState _state;

        /// <inheritdoc />
        public TaskState State { get { return _state; } protected set { UpdateHelper.Do(ref _state, value, OnStateChanged); } }

        /// <inheritdoc />
        public string ErrorMessage { get; protected set; }

        private long _bytesReceived;

        /// <inheritdoc />
        public long BytesProcessed { get { return _bytesReceived; } protected set { UpdateHelper.Do(ref _bytesReceived, value, OnProgressChanged); } }

        private long _bytesTotal = -1;

        /// <inheritdoc />
        public long BytesTotal { get { return _bytesTotal; } protected set { UpdateHelper.Do(ref _bytesTotal, value, OnProgressChanged); } }

        /// <inheritdoc />
        public double Progress
        {
            get
            {
                switch (BytesTotal)
                {
                    case -1:
                        return -1;
                    case 0:
                        return 1;
                    default:
                        return BytesProcessed / (double)BytesTotal;
                }
            }
        }
        #endregion

        #region Constructor
        protected ThreadTask()
        {
            // Prepare the background thread for later execution
            Thread = new Thread(delegate()
            {
                try
                {
                    RunTask();
                }
                    #region Error handling
                catch (OperationCanceledException)
                {
                    lock (StateLock)
                    {
                        // Reset the state so the task can be started again
                        State = TaskState.Ready;
                        CancelRequest = false;
                    }
                }
                catch (IOException ex)
                {
                    lock (StateLock)
                    {
                        ErrorMessage = ex.Message;
                        State = TaskState.IOError;
                    }
                }
                catch (WebException ex)
                {
                    lock (StateLock)
                    {
                        ErrorMessage = ex.Message;
                        State = TaskState.WebError;
                    }
                }
                #endregion
            });
        }
        #endregion

        //--------------------//

        #region Control
        /// <inheritdoc/>
        public virtual void RunSync(CancellationToken cancellationToken)
        {
            if (cancellationToken != null) cancellationToken.CancellationRequested += Cancel;
            try
            {
                lock (StateLock) State = TaskState.Started;
                RunTask();
            }
            catch (OperationCanceledException)
            {
                // Reset the state so the task can be started again
                State = TaskState.Ready;
                CancelRequest = false;
                throw;
            }
            finally
            {
                if (cancellationToken != null) cancellationToken.CancellationRequested -= Cancel;
            }
        }

        /// <inheritdoc/>
        public virtual void Start()
        {
            lock (StateLock)
            {
                if (State != TaskState.Ready) return;

                State = TaskState.Started;
                Thread.Start();
            }
        }

        /// <inheritdoc />
        public virtual void Join()
        {
            lock (StateLock)
            {
                if (Thread == null || !Thread.IsAlive) return;
            }

            Thread.Join();
        }

        /// <inheritdoc />
        public virtual void Cancel()
        {
            lock (StateLock)
            {
                if (CancelRequest || State == TaskState.Ready || State >= TaskState.Complete) return;

                CancelRequest = true;
            }

            if (!Thread.IsAlive) return;
            Thread.Join();
        }
        #endregion

        #region Thread code
        /// <summary>
        /// The actual code to be executed by a background thread.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if <see cref="CancelRequest"/> was detected.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskState.IOError"/>.</exception>
        /// <exception cref="WebException">Thrown if the task ended with <see cref="TaskState.WebError"/>.</exception>
        protected abstract void RunTask();
        #endregion
    }
}
