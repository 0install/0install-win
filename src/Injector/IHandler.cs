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

using Common;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Callback methods to inform the user about the progress of tasks and ask the user questions.
    /// </summary>
    /// <remarks>The callbacks may be called from a background thread. Apply thread-synchronization to update UI elements.</remarks>
    public interface IHandler : ITaskHandler
    {
        /// <summary>
        /// Called to ask the user whether he wishes to trust a new GPG key.
        /// </summary>
        /// <param name="information">Comprehensive information about the new key, to help the user make an informed decision.</param>
        /// <returns><see langword="true"/> if the user accepted the new key; <see langword="false"/> if he rejected it.</returns>
        bool AcceptNewKey(string information);

        /// <summary>
        /// Close any asynchronous GUI threads to make room for a newly launched application.
        /// </summary>
        void CloseAsync();

        /// <summary>
        /// Display multi-line text to the user.
        /// </summary>
        /// <param name="title">A title for the information. Will only be displayed in GUIs, not on the console. Must not contain critical information!</param>
        /// <param name="information">The information to display.</param>
        /// <remarks>
        /// This will never be called during an interactive or long-running operation.
        /// <see cref="AcceptNewKey"/>, <see cref="Log"/> and exceptions are used for that.
        /// </remarks>
        void Output(string title, string information);
    }
}
