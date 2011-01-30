/*
 * Copyright 2010 Bastian Eicher
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
using System.IO;
using Common;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Callback methods to inform the user about IO tasks.
    /// </summary>
    /// <remarks>The callbacks may be called from a background thread. Apply thread-synchronization to update UI elements.</remarks>
    public interface IIOHandler
    {
        /// <summary>
        /// Don't print messages to <see cref="Console"/> unless errors occur and don't block with questions or messages.
        /// </summary>
        bool Batch { get; set; }

        /// <summary>
        /// Called when a new IO task (archive extraction, manifest generation, etc.) needs to be run. Returns once the task has been completed.
        /// </summary>
        /// <param name="task">The extraction task. Call <see cref="ITask.RunSync"/> or equivalent on it. Can be used for tracking the progress.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskState.IOError"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ITask.State"/> is not <see cref="TaskState.Ready"/>.</exception>
        void RunIOTask(ITask task);
    }
}
