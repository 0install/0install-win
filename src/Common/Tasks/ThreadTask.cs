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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Threading;
using Common.Utils;

namespace Common.Tasks
{
    /// <summary>
    /// Abstract base class for background tasks that implement <see cref="ITask"/> using a <see cref="Thread"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposing WaitHandle is not necessary since the SafeWaitHandle is never touched")]
    public abstract class ThreadTask : MarshalByRefObject, ITask
    {
        #region Variables
        /// <summary>Synchronization handle to prevent race conditions with thread startup/shutdown or <see cref="ITask.State"/> switching.</summary>
        protected readonly object StateLock = new object();

        /// <summary>The background thread used for executing the task. Sub-classes must initalize this member.</summary>
        protected Thread Thread;

        /// <summary>The identity of the user that originally created this task.</summary>
        private readonly WindowsIdentity _originalIdentity;
        #endregion

        #region Properties
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public virtual bool CanCancel { get { return true; } }

        private TaskState _state;

        /// <inheritdoc />
        public TaskState State { get { return _state; } protected set { value.To(ref _state, OnStateChanged); } }

        /// <inheritdoc />
        public string ErrorMessage { get; protected set; }

        private long _unitsProcessed;

        /// <inheritdoc />
        public long UnitsProcessed { get { return _unitsProcessed; } protected set { value.To(ref _unitsProcessed, OnProgressChanged); } }

        private long _unitsTotal = -1;

        /// <inheritdoc />
        public long UnitsTotal { get { return _unitsTotal; } protected set { value.To(ref _unitsTotal, OnProgressChanged); } }

        /// <inheritdoc />
        public abstract bool UnitsByte { get; }

        /// <inheritdoc />
        public double Progress
        {
            get
            {
                switch (UnitsTotal)
                {
                    case -1:
                        return -1;
                    case 0:
                        return 1;
                    default:
                        return UnitsProcessed / (double)UnitsTotal;
                }
            }
        }
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

        #region Cancellation
        /// <summary>Flag that indicates the current process should be canceled.</summary>
        protected readonly AutoResetEvent CancelRequest = new AutoResetEvent(false);

        /// <summary>
        /// Throws an <see cref="OperationCanceledException"/> if <see cref="CancelRequest"/> has been signaled.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if <see cref="CancelRequest"/> has been casignaledlled.</exception>
        protected void ThrowIfCancellationRequested()
        {
            if (CancelRequest.WaitOne(0, exitContext: false)) throw new OperationCanceledException();
        }
        #endregion

        #region Constructor
        protected ThreadTask()
        {
            Thread = new Thread(ThreadExecute);

            if (WindowsUtils.IsWindowsNT)
                _originalIdentity = WindowsIdentity.GetCurrent();
        }
        #endregion

        //--------------------//

        #region Control
        /// <inheritdoc/>
        public virtual void RunSync(CancellationToken cancellationToken = null)
        {
            if (cancellationToken != null)
            {
                cancellationToken.CancellationRequested += Cancel;
                cancellationToken.ThrowIfCancellationRequested();
            }
            try
            {
                lock (StateLock) State = TaskState.Started;

                // Run task with prevliges of original user if possible
                if (_originalIdentity != null)
                {
                    using (_originalIdentity.Impersonate())
                        RunTask();
                }
                else RunTask();
            }
            catch (OperationCanceledException)
            {
                // Reset the state so the task can be started again
                State = TaskState.Ready;
                Thread = new Thread(ThreadExecute);
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
                if (State == TaskState.Ready || State >= TaskState.Complete) return;
            }

            CancelRequest.Set();
            if (!Thread.IsAlive) return;
            Thread.Join();
        }
        #endregion

        #region Thread
        /// <summary>
        /// The code executed by the worked <see cref="Thread"/>.
        /// </summary>
        private void ThreadExecute()
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
                    Thread = new Thread(ThreadExecute);
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
            catch (UnauthorizedAccessException ex)
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
        }

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
