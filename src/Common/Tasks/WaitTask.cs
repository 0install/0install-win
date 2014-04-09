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
using System.Threading;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Waits for a <see cref="WaitHandle"/> to become available.
    /// </summary>
    public sealed class WaitTask : TaskBase
    {
        #region Variables
        /// <summary>The <see cref="WaitHandle"/> to wait for.</summary>
        private readonly WaitHandle _waitHandle;
        #endregion

        #region Properties
        private readonly string _name;

        /// <inheritdoc/>
        public override string Name { get { return _name; } }

        /// <inheritdoc/>
        protected override bool UnitsByte { get { return false; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new handle-waiting task.
        /// </summary>
        /// <param name="name">A name describing the task in human-readable form.</param>
        /// <param name="waitHandle">>The <see cref="WaitHandle"/> to wait for.</param>
        public WaitTask(string name, WaitHandle waitHandle)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (waitHandle == null) throw new ArgumentNullException("waitHandle");
            #endregion

            _name = name;
            _waitHandle = waitHandle;
        }
        #endregion

        //--------------------//

        #region Thread code
        /// <inheritdoc/>
        protected override void Execute()
        {
            try
            {
                // Wait for the target handle or a cancel request to arrive
                if (WaitHandle.WaitAny(new[] {_waitHandle, CancellationToken.WaitHandle}) == 1)
                    throw new OperationCanceledException();
            }
            catch (AbandonedMutexException ex)
            {
                // Abandoned mutexes also get owned, but indicate something may have gone wrong elsewhere
                Log.Warn(ex.Message);
            }

            Status = TaskStatus.Complete;
        }
        #endregion
    }
}
