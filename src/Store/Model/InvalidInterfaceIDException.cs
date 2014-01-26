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
using System.Runtime.Serialization;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Indicates an interface ID was not valid. Needs to be an HTTP(S) URI or an absolute path.
    /// </summary>
    [Serializable]
    public sealed class InvalidInterfaceIDException : Exception
    {
        #region Constructor
        /// <summary>
        /// Indicates an interface ID was not valid.
        /// </summary>
        public InvalidInterfaceIDException() : base(string.Format(Resources.InvalidInterfaceID, "unknown"))
        {}

        /// <summary>
        /// Indicates an interface ID was not valid.
        /// </summary>
        public InvalidInterfaceIDException(string message) : base(message)
        {}

        /// <summary>
        /// Indicates an interface ID was not valid.
        /// </summary>
        public InvalidInterfaceIDException(string message, Exception innerException) : base(message, innerException)
        {}

        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private InvalidInterfaceIDException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
        #endregion
    }
}
