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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Common.Tasks
{
    /// <summary>
    /// A task that performs an operation once for each element of a collection.
    /// </summary>
    public sealed class ForEachTask<T> : MarshalByRefObject, ITask
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
        public event TaskEventHandler ProgressChanged { add { } remove { } }
        #endregion

        #region Variables
        /// <summary>Flag that indicates the current process should be canceled.</summary>
        private volatile bool _cancelRequest;

        /// <summary>Synchronization handle to prevent race conditions with thread startup/shutdown or <see cref="State"/> switching.</summary>
        private readonly object _stateLock = new object();

        /// <summary>The background thread used for executing the task. Sub-classes must initalize this member.</summary>
        private readonly Thread _thread;

        /// <summary>A list of objects to execute work for. Cancellation is possible between two elements.</summary>
        private readonly IEnumerable<T> _target;

        /// <summary>The code to be executed once per element in <see cref="_target"/>. May throw <see cref="WebException"/>, <see cref="IOException"/> or <see cref="UserCancelException"/>.</summary>
        private readonly Action<T> _work;
        #endregion

        #region Properties
        /// <inheritdoc />
        public string Name { get; private set; }

        /// <inheritdoc />
        public bool CanCancel { get { return true; } }

        private TaskState _state;

        /// <inheritdoc />
        public TaskState State { get { return _state; } private set { UpdateHelper.Do(ref _state, value, OnStateChanged); } }

        /// <inheritdoc />
        public string ErrorMessage { get; private set; }

        /// <inheritdoc />
        public long BytesProcessed { get { return -1; } }

        /// <inheritdoc />
        public long BytesTotal { get { return -1; } }

        /// <inheritdoc />
        public double Progress { get { return -1; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new for-each task.
        /// </summary>
        /// <param name="name">A name describing the task in human-readable form.</param>
        /// <param name="target">A list of objects to execute work for. Cancellation is possible between two elements.</param>
        /// <param name="work">The code to be executed once per element in <paramref name="target"/>. May throw <see cref="WebException"/>, <see cref="IOException"/> or <see cref="UserCancelException"/>.</param>
        public ForEachTask(string name, IEnumerable<T> target, Action<T> work)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (target == null) throw new ArgumentNullException("target");
            if (work == null) throw new ArgumentNullException("work");
            #endregion

            Name = name;
            _work = work;
            _target = target;

            // Prepare the background thread for later execution
            _thread = new Thread(RunTask);
        }
        #endregion

        //--------------------//

        #region Control
        /// <inheritdoc/>
        public void Start()
        {
            lock (_stateLock)
            {
                if (State != TaskState.Ready) return;

                State = TaskState.Started;
                _thread.Start();
            }
        }

        /// <inheritdoc/>
        public void RunSync()
        {
            try
            {
                foreach (var element in _target)
                {
                    if (_cancelRequest) return;
                    _work(element);
                }
            }
                #region Error handling
            catch (WebException ex)
            {
                State = TaskState.WebError;
                ErrorMessage = ex.Message;
                throw;
            }
            catch (IOException ex)
            {
                State = TaskState.IOError;
                ErrorMessage = ex.Message;
                throw;
            }
            catch (UserCancelException)
            {
                State = TaskState.Ready;
                throw;
            }
            #endregion
        }

        /// <inheritdoc />
        public void Join()
        {
            lock (_stateLock)
            {
                if (_thread == null || !_thread.IsAlive) return;
            }

            _thread.Join();
        }

        /// <inheritdoc />
        public void Cancel()
        {
            _cancelRequest = true;
            _thread.Join();

            // Reset the state so the task can be started again
            State = TaskState.Ready;
            _cancelRequest = false;
        }
        #endregion

        #region Thread code
        /// <summary>
        /// The actual code to be executed by a background thread.
        /// </summary>
        private void RunTask()
        {
            lock (_stateLock) State = TaskState.Data;

            try
            {
                foreach (var element in _target)
                {
                    if (_cancelRequest) return;
                    _work(element);
                }
            }
                #region Error handling
            catch (WebException ex)
            {
                State = TaskState.WebError;
                ErrorMessage = ex.Message;
                return;
            }
            catch (IOException ex)
            {
                State = TaskState.IOError;
                ErrorMessage = ex.Message;
                return;
            }
            catch (UserCancelException)
            {
                State = TaskState.Ready;
                return;
            }
            #endregion

            lock (_stateLock) State = TaskState.Complete;
        }
        #endregion
    }
}
