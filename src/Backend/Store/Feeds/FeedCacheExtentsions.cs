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
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides extensions methods for <see cref="IFeedCache"/>.
    /// </summary>
    public static class FeedCacheExtentsions
    {
        /// <summary>
        /// Loads all <see cref="Feed"/>s stored in <see cref="IFeedCache"/> into memory.
        /// </summary>
        /// <param name="cache">The <see cref="IFeedCache"/> to load <see cref="Feed"/>s from.</param>
        /// <returns>The parsed <see cref="Feed"/>s. Damaged files are logged and skipped.</returns>
        /// <exception cref="IOException">A problem occured while reading from the cache.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the cache is not permitted.</exception>
        public static IEnumerable<Feed> GetAll([NotNull] this IFeedCache cache)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            #endregion

            var feeds = new List<Feed>();
            foreach (var feedUri in cache.ListAll())
            {
                try
                {
                    feeds.Add(cache.GetFeed(feedUri));
                }
                    #region Error handling
                catch (KeyNotFoundException)
                {
                    // Feed file no longer exists
                }
                catch (InvalidDataException ex)
                {
                    Log.Error(ex);
                }
                #endregion
            }
            return feeds;
        }
    }
}
