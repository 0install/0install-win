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
using Common.Utils;

namespace Common.Tasks
{
    /// <summary>
    /// Abstract base class for <see cref="ITask"/> implementations.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposing WaitHandle is not necessary since the SafeWaitHandle is never touched")]
    public abstract class TaskBase : MarshalByRefObject, ITask
    {
        #region Properties
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public object Tag { get; set; }

        /// <inheritdoc />
        public virtual bool CanCancel { get { return true; } }

        private TaskState _state;

        /// <inheritdoc />
        public TaskState State { get { return _state; } protected set { value.To(ref _state, OnStateChanged); } }

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
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event Action<ITask> StateChanged;

        private void OnStateChanged()
        {
            // Copy to local variable to prevent threading issues
            Action<ITask> stateChanged = StateChanged;
            if (stateChanged != null) stateChanged(this);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event Action<ITask> ProgressChanged;

        private void OnProgressChanged()
        {
            // Copy to local variable to prevent threading issues
            Action<ITask> progressChanged = ProgressChanged;
            if (progressChanged != null) progressChanged(this);
        }
        #endregion

        #region Constructor
        protected TaskBase()
        {
            if (WindowsUtils.IsWindowsNT)
                _originalIdentity = WindowsIdentity.GetCurrent();
        }
        #endregion

        //--------------------//

        #region Control
        /// <summary>Synchronization handle to prevent race conditions with <see cref="ITask.State"/> switching.</summary>
        protected readonly object StateLock = new object();

        /// <summary>The identity of the user that originally created this task.</summary>
        private readonly WindowsIdentity _originalIdentity;

        /// <summary>Signaled when the user wishes to cancel the task execution.</summary>
        protected CancellationToken CancellationToken;

        /// <inheritdoc/>
        public virtual void RunSync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            CancellationToken = cancellationToken;

            lock (StateLock) State = TaskState.Started;

            try
            {
                // Run task with privileges of original user if possible
                if (_originalIdentity != null)
                {
                    using (_originalIdentity.Impersonate())
                        RunTask();
                }
                else RunTask();
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                lock (StateLock) State = TaskState.Canceled;
                throw;
            }
            catch (IOException)
            {
                lock (StateLock) State = TaskState.IOError;
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                lock (StateLock) State = TaskState.IOError;
                throw;
            }
            catch (WebException)
            {
                lock (StateLock) State = TaskState.WebError;
                throw;
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// The actual code to be executed.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if the operation was canceled.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskState.IOError"/>.</exception>
        /// <exception cref="WebException">Thrown if the task ended with <see cref="TaskState.WebError"/>.</exception>
        protected abstract void RunTask();
    }
}
