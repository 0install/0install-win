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
using Common.Tasks;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Callback methods to inform the user about tasks being run and ask the user questions.
    /// </summary>
    /// <remarks>The methods may be called from a background thread. Apply appropriate thread-synchronization to update UI elements.</remarks>
    public interface IHandler : ITaskHandler
    {
        /// <summary>
        /// Do not show progress reports, questions or messages (except for non-intrusive background messages like tray icons) unless a critical error occurs.
        /// </summary>
        /// <remarks>Do not change this from a background thread!</remarks>
        bool Batch { get; set; }

        /// <summary>
        /// Asks the whether to trust a new GPG key.
        /// </summary>
        /// <param name="information">Comprehensive information about the new key, to help the user make an informed decision.</param>
        /// <returns><see langword="true"/> if the user accepted the new key; <see langword="false"/> if it was rejected.</returns>
        /// <remarks>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        ///   <para>Only call this between <see cref="ShowProgressUI"/> and <see cref="CloseProgressUI"/>.</para>
        /// </remarks>
        bool AcceptNewKey(string information);

        /// <summary>
        /// Prepares any UI elements necessary to track the progress of <see cref="ITask"/>s.
        /// </summary>
        /// <remarks>Do not call this from a background thread!</remarks>
        void ShowProgressUI();

        /// <summary>
        /// Closes any UI element created by <see cref="ShowProgressUI"/>.
        /// </summary>
        /// <remarks>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        ///   <para>Calling this method multiple times or without calling <see cref="ShowProgressUI"/> first is safe and has no effect.</para>
        /// </remarks>
        void CloseProgressUI();

        /// <summary>
        /// Displays multi-line text to the user.
        /// </summary>
        /// <param name="title">A title for the information. Will only be displayed in GUIs, not on the console. Must not contain critical information!</param>
        /// <param name="information">The information to display.</param>
        /// <remarks>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        ///   <para>Don't call this between <see cref="ShowProgressUI"/> and <see cref="CloseProgressUI"/>. Use <see cref="AcceptNewKey"/>, <see cref="Log"/> or exceptions instead.</para>
        /// </remarks>
        void Output(string title, string information);
    }
}
