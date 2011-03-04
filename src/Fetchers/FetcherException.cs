/*
 * Copyright 2010 Bastian Eicher, Roland Leopold Walkling
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ZeroInstall.Fetchers.Properties;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Represents errors that occurred in an <see cref="IFetcher"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    public sealed class FetcherException : Exception
    {
        #region Constructor
        /// <summary>
        /// Indicates that an unknown problem occurred in an <see cref="IFetcher"/>.
        /// </summary>
        public FetcherException() : base(Resources.FetcherProblem)
        {}

        /// <inheritdoc/>
        public FetcherException(string message) : base(message) 
        {}

        /// <inheritdoc/>
        public FetcherException(string message, Exception innerException) : base(message, innerException)
        {}

        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private FetcherException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
        #endregion
    }
}
