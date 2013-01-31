/*
 * Copyright 2010-2013 Bastian Eicher
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
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// A mock implementation of <see cref="IFeedManager"/>.
    /// </summary>
    public class FeedManagerMock : FeedManagerBase, IEquatable<FeedManagerMock>
    {
        #region Constructor
        /// <summary>
        /// Creates a new cache based on the given path to a cache directory.
        /// </summary>
        /// <param name="cache">The disk-based cache to store downloaded <see cref="Feed"/>s.</param>
        public FeedManagerMock(IFeedCache cache) : base(cache)
        {}
        #endregion

        //--------------------//

        #region Get feed
        /// <summary>
        /// Always throws <see cref="NotImplementedException"/>.
        /// </summary>
        public override Feed GetFeed(string feedID, Policy policy, ref bool stale)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Import feed
        /// <summary>
        /// Always throws <see cref="NotImplementedException"/>.
        /// </summary>
        public override void ImportFeed(Uri uri, Uri mirrorUri, byte[] data, Policy policy)
        {
            throw new NotImplementedException();
        }
        #endregion

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FeedManagerMock other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(FeedManagerMock) && Equals((FeedManagerMock)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
