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

namespace Common
{
    /// <summary>
    /// Abstract base class for background tasks that implement <see cref="ITask"/>.
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

        #region Properties
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract bool CanCancel { get; }

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

        //--------------------//

        #region Control
        /// <inheritdoc />
        public abstract void Start();

        /// <inheritdoc />
        public abstract void RunSync();

        /// <inheritdoc />
        public abstract void Join();

        /// <inheritdoc />
        public abstract void Cancel();
        #endregion
    }
}
