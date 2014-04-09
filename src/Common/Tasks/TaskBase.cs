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
using NanoByte.Common.Utils;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Abstract base class for <see cref="ITask"/> implementations.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposing WaitHandle is not necessary since the SafeWaitHandle is never touched")]
    public abstract class TaskBase : MarshalByRefObject, ITask
    {
        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public object Tag { get; set; }

        /// <inheritdoc/>
        public virtual bool CanCancel { get { return true; } }

        protected TaskBase()
        {
            if (WindowsUtils.IsWindowsNT)
                _originalIdentity = WindowsIdentity.GetCurrent();
        }

        #region Run
        /// <summary>The identity of the user that originally created this task.</summary>
        private readonly WindowsIdentity _originalIdentity;

        /// <summary>Signaled when the user wishes to cancel the task execution.</summary>
        protected CancellationToken CancellationToken;

        /// <summary>>Used to report back the task's progress.</summary>
        private IProgress<TaskSnapshot> _progress;

        /// <inheritdoc/>
        public void Run(CancellationToken cancellationToken = default(CancellationToken), IProgress<TaskSnapshot> progress = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CancellationToken = cancellationToken;
            _progress = progress;

            Status = TaskStatus.Started;

            try
            {
                // Run task with privileges of original user if possible
                if (_originalIdentity != null)
                {
                    using (_originalIdentity.Impersonate())
                        Execute();
                }
                else Execute();
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                Status = TaskStatus.Canceled;
                throw;
            }
            catch (IOException)
            {
                Status = TaskStatus.IOError;
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                Status = TaskStatus.IOError;
                throw;
            }
            catch (WebException)
            {
                Status = TaskStatus.WebError;
                throw;
            }
            #endregion
        }
        #endregion

        #region Progress
        private TaskStatus _status;

        /// <summary>The current status of the task.</summary>
        protected internal TaskStatus Status { get { return _status; } protected set { value.To(ref _status, OnProgressChanged); } }

        /// <summary>
        /// <see langword="true"/> if <see cref="UnitsProcessed"/> and <see cref="UnitsTotal"/> are measured in bytes;
        /// <see langword="false"/> if they are measured in generic units.
        /// </summary>
        protected abstract bool UnitsByte { get; }

        private long _unitsProcessed;

        /// <summary>The number of units that have been processed so far.</summary>
        protected long UnitsProcessed { get { return _unitsProcessed; } set { value.To(ref _unitsProcessed, OnProgressChangedThrottled); } }

        private long _unitsTotal = -1;

        /// <summary>The total number of units that are to be processed; -1 for unknown.</summary>
        protected long UnitsTotal { get { return _unitsTotal; } set { value.To(ref _unitsTotal, OnProgressChanged); } }

        /// <summary>
        /// Informs the caller of the current progress, if a callback was registered.
        /// </summary>
        private void OnProgressChanged()
        {
            if (_progress == null) return;

            _progress.Report(new TaskSnapshot(_status, UnitsByte, _unitsProcessed, _unitsTotal));
        }

        private DateTime _lastProgress;
        private static readonly TimeSpan _progressRate = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Informs the caller of the current progress, if a callback was registered. Limits the rate of progress updates.
        /// </summary>
        private void OnProgressChangedThrottled()
        {
            if (_progress == null) return;

            var now = DateTime.Now;
            if ((now - _lastProgress) < _progressRate) return;

            _progress.Report(new TaskSnapshot(_status, UnitsByte, _unitsProcessed, _unitsTotal));
            _lastProgress = now;
        }
        #endregion

        /// <summary>
        /// The actual code to be executed.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if the operation was canceled.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskStatus.IOError"/>.</exception>
        /// <exception cref="WebException">Thrown if the task ended with <see cref="TaskStatus.WebError"/>.</exception>
        protected abstract void Execute();
    }
}
