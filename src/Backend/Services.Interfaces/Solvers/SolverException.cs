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

using System;
using System.Runtime.Serialization;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Indicates the <see cref="ISolver"/> was unable to provide <see cref="Selections"/> that fulfill the <see cref="Requirements"/>.
    /// </summary>
    [Serializable]
    public sealed class SolverException : Exception
    {
        /// <summary>
        /// Indicates that the <see cref="ISolver"/> encountered an unknown problem.
        /// </summary>
        public SolverException()
        {}

        /// <summary>
        /// Indicates that the <see cref="ISolver"/> encountered a specific problem.
        /// </summary>
        public SolverException(string message) : base(message)
        {}

        /// <summary>
        /// Indicates that there was a problem parsing the <see cref="ISolver"/>'s output.
        /// </summary>
        public SolverException(string message, Exception innerException) : base(message, innerException)
        {}

        #region Serialization
        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private SolverException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
        #endregion
    }
}
