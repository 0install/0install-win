/*
 * Copyright 2010-2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
