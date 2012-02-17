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
using System.Threading;

namespace Common.Tasks
{
    /// <summary>
    /// Waits for a <see cref="Mutex"/> to become available.
    /// </summary>
    public sealed class MutexTask : ThreadTask
    {
        #region Variables
        /// <summary>The <see cref="Mutex"/> to wait for.</summary>
        private readonly Mutex _mutex;
        #endregion

        #region Properties
        private readonly string _name;

        /// <inheritdoc/>
        public override string Name { get { return _name; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new mutex-waiting task.
        /// </summary>
        /// <param name="name">A name describing the task in human-readable form.</param>
        /// <param name="mutex">The <see cref="Mutex"/> to wait for.</param>
        public MutexTask(string name, Mutex mutex)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (mutex == null) throw new ArgumentNullException("mutex");
            #endregion

            _name = name;
            _mutex = mutex;
        }
        #endregion

        //--------------------//

        #region Thread code
        /// <inheritdoc/>
        protected override void RunTask()
        {
            // Wait for the mutex and allow cancellation every 100 ms
            try
            {
                while (!_mutex.WaitOne(100))
                    if (CancelRequest) throw new OperationCanceledException();
            }
            catch (AbandonedMutexException ex)
            {
                // Abandoned mutexes also get owned, but indicate something may have gone wrong elsewhere
                Log.Warn(ex.Message);
            }

            lock (StateLock) State = TaskState.Complete;
        }
        #endregion
    }
}
