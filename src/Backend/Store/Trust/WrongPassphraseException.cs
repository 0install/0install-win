/*
 * Copyright 2010-2015 Bastian Eicher
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

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Indicates that an incorrect passphrase was passed to <see cref="GnuPG"/>.
    /// </summary>
    [Serializable]
    public sealed class WrongPassphraseException : Exception
    {
        /// <summary>
        /// Indicates that an incorrect passphrase was passed to <see cref="GnuPG"/>.
        /// </summary>
        public WrongPassphraseException() : base(Resources.WrongPassphrase)
        {}

        /// <inheritdoc/>
        public WrongPassphraseException(string message) : base(message)
        {}

        /// <inheritdoc/>
        public WrongPassphraseException(string message, Exception innerException) : base(message, innerException)
        {}

        #region Serialization
        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private WrongPassphraseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
        #endregion
    }
}
