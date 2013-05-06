/*
 * Copyright 2010-2012 
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

namespace Common
{
    /// <summary>
    /// Like a <see cref="UnauthorizedAccessException"/> but with the additional hint that retrying the operation as an administrator would fix the problem.
    /// </summary>
    [Serializable]
    public class NotAdminException : UnauthorizedAccessException
    {
        /// <inheritdoc/>
        public NotAdminException()
        {}

        /// <inheritdoc/>
        public NotAdminException(string message, Exception inner) : base(message, inner)
        {}

        /// <inheritdoc/>
        public NotAdminException(string message) : base(message)
        {}

        /// <inheritdoc/>
        protected NotAdminException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
}
