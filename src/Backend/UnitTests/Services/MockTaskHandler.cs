/*
 * Copyright 2010-2016 Bastian Eicher
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

using System.Collections;
using System.Collections.Generic;
using NanoByte.Common;
using NanoByte.Common.Tasks;

namespace ZeroInstall.Services
{
    /// <summary>
    /// A minimalistic <see cref="ITaskHandler"/> that allows you to pre-record answers and retrieve output.
    /// </summary>
    public class MockTaskHandler : SilentTaskHandler
    {
        public override Verbosity Verbosity { get; set; }

        /// <summary>
        /// The prerecorded result for <see cref="Ask"/>.
        /// </summary>
        public bool AnswerQuestionWith { get; set; }

        /// <summary>
        /// Last question passed to <see cref="Ask"/>.
        /// </summary>
        public string LastQuestion { get; private set; }

        /// <summary>
        /// Fakes asking the user a question.
        /// </summary>
        /// <returns>The current value of <see cref="AnswerQuestionWith"/>.</returns>
        protected override bool Ask(string question, MsgSeverity severity)
        {
            LastQuestion = question;
            return AnswerQuestionWith;
        }

        /// <summary>
        /// Last information string passed to <see cref="Output"/>.
        /// </summary>
        public string LastOutput { get; private set; }

        /// <summary>
        /// Fakes showing an information string output to the user.
        /// </summary>
        public override void Output(string title, string message)
        {
            LastOutput = message;
        }

        /// <summary>
        /// Last data objects passed to <see cref="Output{T}"/>.
        /// </summary>
        public IEnumerable LastOutputObjects { get; set; }

        /// <summary>
        /// Fakes showing tabular data to the user.
        /// </summary>
        public override void Output<T>(string title, IEnumerable<T> data)
        {
            LastOutputObjects = data;
        }
    }
}
