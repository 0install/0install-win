/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common;
using Common.Tasks;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Model;
using ZeroInstall.Store;
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
        /// Provides information for a potential GUI backing this handler.
        /// </summary>
        /// <param name="actionTitle">A short title describing what the command being executed does; may be <see langword="null"/>.</param>
        /// <param name="delay">The number of milliseconds by which to delay the initial display of the GUI.</param>
        /// <remarks>Should be called before <see cref="ShowProgressUI"/>.</remarks>
        void SetGuiHints(string actionTitle, int delay);

        /// <summary>
        /// Prepares any UI elements necessary to track the progress of <see cref="ITask"/>s.
        /// </summary>
        /// <remarks>Do not call this from a background thread!</remarks>
        void ShowProgressUI();

        /// <summary>
        /// Disables any UI element created by <see cref="ShowProgressUI"/> but still leaves it visible.
        /// </summary>
        /// <remarks>
        ///   <para>Calling this method multiple times or without calling <see cref="ShowProgressUI"/> first is safe and has no effect.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is handled automatically.</para>
        /// </remarks>
        void DisableProgressUI();

        /// <summary>
        /// Closes any UI element created by <see cref="ShowProgressUI"/>.
        /// </summary>
        /// <remarks>
        ///   <para>Calling this method multiple times or without calling <see cref="ShowProgressUI"/> first is safe and has no effect.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is handled automatically.</para>
        /// </remarks>
        void CloseProgressUI();

        /// <summary>
        /// Asks the user a Yes/No/Cancel question (e.g., whether to trust a new GPG key).
        /// </summary>
        /// <param name="question">The question and comprehensive information to help the user make an informed decision.</param>
        /// <param name="batchInformation">Information to be displayed if the question was automatically answered with 'No' because <see cref="Batch"/> was set to <see langword="true"/>; may be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the user answered with 'Yes'; <see langword="false"/> if the user answered with 'No'.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user selected 'Cancel'.</exception>
        /// <remarks>
        ///   <para>Only call this between <see cref="ShowProgressUI"/> and <see cref="CloseProgressUI"/>.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is handled automatically.</para>
        /// </remarks>
        bool AskQuestion(string question, string batchInformation);

        /// <summary>
        /// Shows the user the <see cref="Selections"/> made by the solver.
        /// Returns immediately. Will be ignored by non-GUI intefaces.
        /// </summary>
        /// <param name="selections">The <see cref="Selections"/> as provided by the solver.</param>
        /// <param name="feedCache">The feed cache used to retrieve feeds for additional information about implementations.</param>
        /// <remarks>
        ///   <para>Only call this between <see cref="ShowProgressUI"/> and <see cref="CloseProgressUI"/>.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is handled automatically.</para>
        /// </remarks>
        void ShowSelections(Selections selections, IFeedCache feedCache);

        /// <summary>
        /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the solver if desired.
        /// Returns once the user is satisfied with her choice. Will be ignored by non-GUI intefaces.
        /// </summary>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the solver needs to be rerun.</param>
        /// <remarks>
        ///   <para>Only call this between <see cref="ShowSelections"/> and <see cref="CloseProgressUI"/>.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is handled automatically.</para>
        /// </remarks>
        void AuditSelections(Func<Selections> solveCallback);

        /// <summary>
        /// Displays multi-line text to the user.
        /// </summary>
        /// <param name="title">A title for the information. Will only be displayed in GUIs, not on the console. Must not contain critical information!</param>
        /// <param name="information">The information to display.</param>
        /// <remarks>
        ///   <para>This may trigger <see cref="DisableProgressUI"/> as a side effect. Use <see cref="AskQuestion"/>, <see cref="Log"/> or exceptions to avoid this.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is handled automatically.</para>
        /// </remarks>
        void Output(string title, string information);

        /// <summary>
        /// Displays application integration options to the user.
        /// </summary>
        /// <param name="integrationManager">The integration manager used to apply selected integration options.</param>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        /// <remarks>
        ///   <para>Only call this between <see cref="ShowProgressUI"/> and <see cref="CloseProgressUI"/>.</para>
        ///   <para>This may be called from a background thread. Thread-synchronization for UI elements is handled automatically.</para>
        /// </remarks>
        void ShowIntegrateApp(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed);

        /// <summary>
        /// Displays the configuration settings to the user.
        /// </summary>
        /// <param name="config">The configuration to show.</param>
        /// <returns><see langword="true"/> if the user modified any settings; <see langword="false"/> otherwise.</returns>
        bool ShowConfig(Config config);
    }
}
