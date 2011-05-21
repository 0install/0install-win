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
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Feeds;

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
        /// Prepares any UI elements necessary to track the progress of <see cref="ITask"/>s.
        /// </summary>
        /// <param name="cancelCallback">To be called when the user wishes to cancel the current process.</param>
        /// <remarks>Do not call this from a background thread!</remarks>
        void ShowProgressUI(SimpleEventHandler cancelCallback);

        /// <summary>
        /// Disables any UI element created by <see cref="ShowProgressUI"/> but still leaves it visible.
        /// </summary>
        /// <remarks>
        ///   <para>Calling this method multiple times or without calling <see cref="ShowProgressUI"/> first is safe and has no effect.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        /// </remarks>
        void DisableProgressUI();

        /// <summary>
        /// Closes any UI element created by <see cref="ShowProgressUI"/>.
        /// </summary>
        /// <remarks>
        ///   <para>Calling this method multiple times or without calling <see cref="ShowProgressUI"/> first is safe and has no effect.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        /// </remarks>
        void CloseProgressUI();

        /// <summary>
        /// Asks the whether to trust a new GPG key.
        /// </summary>
        /// <param name="information">Comprehensive information about the new key, to help the user make an informed decision.</param>
        /// <returns><see langword="true"/> if the user accepted the new key; <see langword="false"/> if it was rejected.</returns>
        /// <remarks>
        ///   <para>Only call this between <see cref="ShowProgressUI"/> and <see cref="CloseProgressUI"/>.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        /// </remarks>
        bool AcceptNewKey(string information);

        /// <summary>
        /// Shows the user the <see cref="Selections"/> made by the <see cref="ISolver"/>.
        /// Returns immediately. Will be ignored by non-GUI intefaces.
        /// </summary>
        /// <param name="selections">The <see cref="Selections"/> as provided by the <see cref="ISolver"/>.</param>
        /// <param name="feedCache">The feed cache used to retreive feeds for additional information about imlementations.</param>
        /// <remarks>
        ///   <para>Only call this between <see cref="ShowProgressUI"/> and <see cref="CloseProgressUI"/>.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        /// </remarks>
        void ShowSelections(Selections selections, IFeedCache feedCache);

        /// <summary>
        /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the <see cref="ISolver"/> if desired.
        /// Returns once the user is satisfied with her choice. Will be ignored by non-GUI intefaces.
        /// </summary>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <remarks>
        ///   <para>Only call this between <see cref="ShowSelections"/> and <see cref="CloseProgressUI"/>.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        /// </remarks>
        void AuditSelections(SimpleResult<Selections> solveCallback);

        /// <summary>
        /// Displays multi-line text to the user.
        /// </summary>
        /// <param name="title">A title for the information. Will only be displayed in GUIs, not on the console. Must not contain critical information!</param>
        /// <param name="information">The information to display.</param>
        /// <remarks>
        ///   <para>This may trigger <see cref="DisableProgressUI"/> and <see cref="CloseProgressUI"/> as a side effect. Use <see cref="AcceptNewKey"/>, <see cref="Log"/> or exceptions to avoid this.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        /// </remarks>
        void Output(string title, string information);

        /// <summary>
        /// Displays the configuration settings to the user.
        /// </summary>
        /// <param name="config">The configuration to show.</param>
        /// <returns><see langword="true"/> if the user modified any settings; <see langword="false"/> otherwise.</returns>
        /// <remarks>
        ///   <para>Don't call this between <see cref="ShowProgressUI"/> and <see cref="CloseProgressUI"/>.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is automatically handled.</para>
        /// </remarks>
        bool ShowConfig(Config config);
    }
}
