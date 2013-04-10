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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains helper methods for filtering <see cref="Selections"/>.
    /// </summary>
    public static class SelectionsUtils
    {
        /// <summary>
        /// Returns a list of any downloadable <see cref="ImplementationSelection"/>s that are missing from an <see cref="IStore"/>.
        /// </summary>
        /// <param name="selections">The selections to search for <see cref="ImplementationSelection"/>s that are missing.</param>
        /// <param name="searchStore">The locations to search for cached <see cref="Implementation"/>s.</param>
        /// <remarks>Feed files may be downloaded, no implementations are downloaded.</remarks>
        /// <exception cref="KeyNotFoundException">Thrown if a <see cref="Feed"/> or <see cref="Implementation"/> is missing.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the feed file could not be parsed.</exception>
        public static IEnumerable<ImplementationSelection> GetUncachedImplementationSelections(this Selections selections, IStore searchStore)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (searchStore == null) throw new ArgumentNullException("searchStore");
            #endregion

            return selections.Implementations.Where(implementation =>
                // Local paths are considered to be always available
                string.IsNullOrEmpty(implementation.LocalPath) &&
                    // Don't try to download PackageImplementations
                    string.IsNullOrEmpty(implementation.Package) &&
                    // Don't try to fetch virutal feeds
                    (string.IsNullOrEmpty(implementation.FromFeed) || !implementation.FromFeed.StartsWith(ImplementationSelection.DistributionFeedPrefix)) &&
                    // Don't download implementations that are already in the store
                    !searchStore.Contains(implementation.ManifestDigest) &&
                    // Ignore implementations without an ID
                    !string.IsNullOrEmpty(implementation.ID));
        }

        /// <summary>
        /// Retrieves the original <see cref="Implementation"/>s these selections were based on.
        /// </summary>
        /// <param name="implementationSelections">The implementation selections to base the search of</param>
        /// <param name="feedCache">Used to load <see cref="Feed"/>s containing the original <see cref="Implementation"/>s.</param>
        /// <returns></returns>
        public static IEnumerable<Implementation> GetOriginalImplementations(this IEnumerable<ImplementationSelection> implementationSelections, IFeedCache feedCache)
        {
            #region Sanity checks
            if (implementationSelections == null) throw new ArgumentNullException("implementationSelections");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            return implementationSelections.Select(impl => feedCache.GetFeed(impl.FromFeed ?? impl.InterfaceID)[impl.ID].CloneImplementation());
        }

        /// <summary>
        /// Combines <see cref="GetUncachedImplementationSelections"/> and <see cref="GetOriginalImplementations"/>.
        /// </summary>
        /// <param name="selections">The selections to search for <see cref="ImplementationSelection"/>s that are missing.</param>
        /// <param name="policy">Provides the <see cref="IStore"/> and <see cref="IFeedCache"/>.</param>
        public static IEnumerable<Implementation> GetUncachedImplementations(this Selections selections, Policy policy)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            return selections.GetUncachedImplementationSelections(policy.Fetcher.Store).GetOriginalImplementations(policy.FeedManager.Cache);
        }
    }
}