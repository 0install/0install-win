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
using System.Runtime.Serialization;
using ZeroInstall.DownloadBroker.Properties;

namespace ZeroInstall.DownloadBroker
{
    /// <summary>
    /// Represents errors that occured in <see cref="Fetcher"/>.
    /// </summary>
    [Serializable]
    public sealed class FetcherException : Exception
    {
        #region Constructor
        public FetcherException() : base(Resources.FetcherProblem)
        {}

        public FetcherException(string message) : base(message) 
        {}

        public FetcherException(string message, Exception innerException) : base (message, innerException)
        {}

        private FetcherException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
        #endregion
    }
}
