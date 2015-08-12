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
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains extension methods for <see cref="Feed"/>s.
    /// </summary>
    public static class FeedExtensions
    {
        /// <summary>
        /// Tries to find an <see cref="Implementation"/> with a specific <see cref="ManifestDigest"/> in a list of <see cref="Feed"/>s.
        /// </summary>
        /// <param name="feeds">The list of <see cref="Feed"/>s to search in.</param>
        /// <param name="digest">The digest to search for.</param>
        /// <param name="feed">Returns the <see cref="Feed"/> a match was found in; <c>null</c> if no match found.</param>
        /// <returns>The matching <see cref="Store.Model.Implementation"/>; <c>null</c> if no match found.</returns>
        [ContractAnnotation("=>null,feed:null; =>notnull,feed:notnull")]
        public static Implementation GetImplementation([NotNull] this IEnumerable<Feed> feeds, ManifestDigest digest, out Feed feed)
        {
            #region Sanity checks
            if (feeds == null) throw new ArgumentNullException(nameof(feeds));
            #endregion

            foreach (var curFeed in feeds)
            {
                var impl = curFeed.Elements.OfType<Implementation>().FirstOrDefault(implementation => implementation.ManifestDigest.PartialEquals(digest));
                if (impl != null)
                {
                    feed = curFeed;
                    return impl;
                }
            }

            feed = null;
            return null;
        }
    }
}
