/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Linq;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Management
{
    /// <summary>
    /// Provides utiltity methods for managing <see cref="Model.Implementation"/>s.
    /// </summary>
    public static class ImplementationUtils
    {
        /// <summary>
        /// Tries to find an <see cref="Model.Implementation"/> with a specific <see cref="ManifestDigest"/> in a list of <see cref="Feed"/>s.
        /// </summary>
        /// <param name="digest">The digest to search for.</param>
        /// <param name="feeds">The list of <see cref="Feed"/>s to search in.</param>
        /// <param name="feed">Returns the <see cref="Feed"/> a match was found in; <see langword="null"/> if no match found.</param>
        /// <returns>The matching <see cref="Model.Implementation"/>; <see langword="null"/> if no match found.</returns>
        public static Model.Implementation GetImplementation(ManifestDigest digest, IEnumerable<Feed> feeds, out Feed feed)
        {
            #region Sanity checks
            if (feeds == null) throw new ArgumentNullException("feeds");
            #endregion

            foreach (var curFeed in feeds)
            {
                var impl = curFeed.Elements.OfType<Model.Implementation>().
                    FirstOrDefault(implementation => implementation.ManifestDigest.PartialEquals(digest));
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
