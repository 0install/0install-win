﻿/*
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

using ZeroInstall.Model.Selection;
using ZeroInstall.Services;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall
{
    /// <summary>
    /// A minimalistic <see cref="IBackendHandler"/> that allows you to pre-record answers and retrieve output.
    /// </summary>
    public class MockHandler : SilentHandler
    {
        /// <summary>
        /// Do not show progress reports, questions or messages (except for non-intrusive background messages like tray icons) unless a critical error occurs.
        /// </summary>
        public override bool Batch { get; set; }

        /// <summary>
        /// The prerecorded result for <see cref="AskQuestion"/>.
        /// </summary>
        public bool AnswerQuestionWith;

        /// <summary>
        /// Fakes asking the user a question.
        /// </summary>
        /// <returns>The current value of <see cref="AnswerQuestionWith"/>.</returns>
        public override bool AskQuestion(string question, string batchInformation = null)
        {
            return AnswerQuestionWith;
        }

        /// <summary>
        /// Last information string passed to <see cref="Output"/>.
        /// </summary>
        public string LastOutput;

        /// <summary>
        /// Fakes showing an information string output to the user.
        /// </summary>
        public override void Output(string title, string information)
        {
            LastOutput = information;
        }

        /// <summary>
        /// Last <see cref="Selections"/> passed to <see cref="ShowSelections"/>.
        /// </summary>
        public Selections LastSelections;

        /// <summary>
        /// Fakes showing <see cref="Selections"/> to the user.
        /// </summary>
        public override void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            LastSelections = selections;
        }
    }
}
