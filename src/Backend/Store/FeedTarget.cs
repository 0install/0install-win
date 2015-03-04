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

using JetBrains.Annotations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Associates a <see cref="FeedUri"/> with the <see cref="Feed"/> data aquired from there.
    /// </summary>
    /// <remarks><see cref="Model.Feed.Uri"/> is only mandatory for remote feeds. This structure associates a <see cref="FeedUri"/> with all kinds of feeds, local and remote.</remarks>
    public struct FeedTarget
    {
        /// <summary>
        /// The URI or local path (must be absolute) the feed was aquired from.
        /// </summary>
        [NotNull]
        public readonly FeedUri Uri;

        /// <summary>
        /// The data aquired from <see cref="Uri"/>. <see cref="Store.Model.Feed.Normalize"/> has already been called.
        /// </summary>
        [NotNull]
        public readonly Feed Feed;

        /// <summary>
        /// Creates a new feed target.
        /// </summary>
        /// <param name="uri">The URI or local path (must be absolute) to the feed.</param>
        /// <param name="feed">The data aquired from <paramref name="uri"/>. <see cref="Store.Model.Feed.Normalize"/> has already been called.</param>
        public FeedTarget([NotNull] FeedUri uri, [NotNull] Feed feed)
        {
            Uri = uri;
            Feed = feed;
        }
    }
}
