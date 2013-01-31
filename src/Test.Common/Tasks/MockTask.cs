/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Threading;

namespace Common.Tasks
{
    /// <summary>
    /// Pretends to perform a task for testing purposes
    /// </summary>
    public class MockTask : ThreadTask
    {
        #region Variables
        /// <summary>Indicates that <see cref="MockStateComplete"/> has been called.</summary>
        private readonly AutoResetEvent _joinWait = new AutoResetEvent(false);
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "MockTask"; } }

        /// <inheritdoc/>
        public override bool CanCancel { get { return false; } }

        /// <inheritdoc />
        public override bool UnitsByte { get { return false; } }
        #endregion

        #region Control
        /// <summary>
        /// Sets <see cref="ITask.State"/> to <see cref="TaskState.Started"/> and <see cref="ITask.UnitsProcessed"/> to <code>128</code>.
        /// </summary>
        public override void Start()
        {
            State = TaskState.Header;
            UnitsTotal = 128;
        }

        /// <summary>
        /// Blocks until <see cref="MockStateComplete"/> is called.
        /// </summary>
        public override void Join()
        {
            _joinWait.WaitOne();
        }

        /// <inheritdoc/>
        public override void Cancel()
        {
            throw new NotSupportedException("Task can not be canceled.");
        }
        #endregion

        #region Thread code
        /// <inheritdoc/>
        protected override void RunTask()
        {
            lock (StateLock) State = TaskState.Complete;
        }
        #endregion

        //--------------------//

        #region Mock state
        /// <summary>
        /// Sets <see cref="ITask.State"/> to <see cref="TaskState.Data"/> and <see cref="ITask.UnitsProcessed"/> to <code>64</code>.
        /// </summary>
        public void MockStateData()
        {
            State = TaskState.Data;
            UnitsProcessed = 64;
        }

        /// <summary>
        /// Sets <see cref="ITask.State"/> to <see cref="TaskState.Complete"/> and unlocks <see cref="Join"/>.
        /// </summary>
        public void MockStateComplete()
        {
            State = TaskState.Complete;
            _joinWait.Set();
        }
        #endregion
    }
}
