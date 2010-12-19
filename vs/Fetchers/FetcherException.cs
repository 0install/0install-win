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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Common.Collections;
using ZeroInstall.Fetchers.Properties;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Represents errors that occured in <see cref="Fetcher"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    public sealed class FetcherException : Exception
    {
        #region Properties
        private readonly C5.ICollection<Exception> _problems;
        /// <summary>
        /// A list of all problems the <see cref="Fetcher"/> encountered while tying to process a <see cref="FetchRequest"/>.
        /// </summary>
        public IEnumerable<Exception> Problems { get { return _problems; } }
        #endregion

        #region Constructor
        public FetcherException() : base(Resources.FetcherProblem)
        {}

        public FetcherException(string message) : base(message) 
        {}

        public FetcherException(string message, Exception innerException) : base(message, innerException)
        {}
        
        /// <summary>
        /// Creates a new fetcher exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="problems">A list of all problems the <see cref="Fetcher"/> encountered while tying to process a <see cref="FetchRequest"/>.</param>
        public FetcherException(string message, IEnumerable<Exception> problems) : base(message, EnumUtils.GetFirst(problems))
        {
            // Defensive copy
            var tempList = new C5.ArrayList<Exception>();
            tempList.AddAll(problems);

            // Make the collections immutable
            _problems = new C5.GuardedList<Exception>(tempList);
        }

        private FetcherException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            #endregion

            var problems = (Exception[])info.GetValue("Problems", typeof(Exception[]));

            // Defensive copy
            var tempList = new C5.ArrayList<Exception>();
            tempList.AddAll(problems);

            // Make the collections immutable
            _problems = new C5.GuardedList<Exception>(tempList);
        }
        #endregion

        #region Serialization
        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            #endregion

            info.AddValue("Problems", _problems.ToArray());

            base.GetObjectData(info, context);
        }
        #endregion
    }
}
