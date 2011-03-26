/*
 * Copyright 2006-2010 Bastian Eicher
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

namespace Common.Tasks
{
    /// <summary>
    /// Passes calls on to another <see cref="ITaskHandler"/> but allows an additional callback hook.
    /// </summary>
    public sealed class HookTaskHandler : MarshalByRefObject, ITaskHandler
    {
        #region Variables
        private readonly ITaskHandler _innerHandler;
        private readonly Action<ITask> _taskCallback;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new hook task handler.
        /// </summary>
        /// <param name="innerHandler">The <see cref="ITaskHandler"/> to pass calls on to.</param>
        /// <param name="taskCallback">A callback method to be called before <see cref="ITaskHandler.RunTask"/> is called.</param>
        public HookTaskHandler(ITaskHandler innerHandler, Action<ITask> taskCallback)
        {
            _innerHandler = innerHandler;
            _taskCallback = taskCallback;
        }
        #endregion

        public void RunTask(ITask task, object tag)
        {
            _taskCallback(task);
            _innerHandler.RunTask(task, tag);
        }
    }
}
