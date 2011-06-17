/*
 * Copyright 2010-2011 Bastian Eicher, Roland Leopold Walkling
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
using System.Text;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Lists one or more <see cref="Model.Implementation"/>s that need to be downloaded and extracted into an <see cref="IStore"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    public class FetchRequest : IEquatable<FetchRequest>
    {
        #region Properties
        // Preserve order, duplicate entries are not allowed
        private readonly C5.ISequenced<Implementation> _implementations;
        /// <summary>
        /// The <see cref="Model.Implementation"/>s to be downloaded.
        /// </summary>
        public IEnumerable<Implementation> Implementations { get { return _implementations; } }

        /// <summary>
        /// A callback object used when the the user needs to be informed about progress.
        /// </summary>
        public ITaskHandler Handler { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download request.
        /// </summary>
        /// <param name="implementations">The <see cref="Model.Implementation"/>s to be downloaded.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        public FetchRequest(IEnumerable<Implementation> implementations, ITaskHandler handler)
        {
            #region Sanity checks
            if (implementations == null) throw new ArgumentNullException("implementations");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Defensive copy and remove duplicates
            var tempList = new C5.ArrayList<Implementation>();
            foreach (var implementation in implementations)
                if (implementation != null && !tempList.Contains(implementation)) tempList.Add(implementation);

            // Make the collection immutable
            _implementations = new C5.GuardedList<Implementation>(tempList);

            Handler = handler;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder("FetchRequest:");
            foreach (var implementation in Implementations)
                builder.AppendLine((implementation == null) ? null : implementation.ToString());
            return builder.ToString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FetchRequest other)
        {
            if (other == null) return false;

            if (!_implementations.SequencedEquals(other._implementations)) return false;
            return other.Handler == Handler;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(FetchRequest) && Equals((FetchRequest)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = _implementations.GetSequencedHashCode();
                result = (result * 397) ^ Handler.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
