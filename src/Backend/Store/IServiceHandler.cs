/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common.Tasks;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Callback methods to inform the user about tasks being run, ask the user questions and output results.
    /// </summary>
    /// <remarks>The methods may be called from a background thread. Implementations apply appropriate thread-synchronization to update UI elements.</remarks>
    public interface IServiceHandler : ITaskHandler
    {
        /// <summary>
        /// Do not show progress reports, questions or messages (except for non-intrusive background messages like tray icons) unless a critical error occurs.
        /// </summary>
        bool Batch { get; set; }

        /// <summary>
        /// Asks the user a Yes/No/Cancel question (e.g., whether to trust a new GPG key).
        /// </summary>
        /// <param name="question">The question and comprehensive information to help the user make an informed decision.</param>
        /// <param name="batchInformation">Information to be displayed if the question was automatically answered with 'No' because <see cref="Batch"/> was set to <see langword="true"/>; may be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the user answered with 'Yes'; <see langword="false"/> if the user answered with 'No'.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user selected 'Cancel'.</exception>
        bool AskQuestion(string question, string batchInformation = null);

        /// <summary>
        /// Displays multi-line text to the user.
        /// </summary>
        /// <param name="title">A title for the information. Will only be displayed in GUIs, not on the console. Must not contain critical information!</param>
        /// <param name="information">The information to display.</param>
        /// <remarks>Implementations may close the UI as a side effect. Therefore this should be your last call on the handler.</remarks>
        void Output(string title, string information);
    }
}
