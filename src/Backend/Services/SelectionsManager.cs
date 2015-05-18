/*
 * Copyright 2010-2015 Bastian Eicher
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
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services
{
    /// <summary>
    /// Contains helper methods for filtering <see cref="Selections"/>.
    /// </summary>
    public class SelectionsManager
    {
        #region Dependencies
        private readonly IFeedCache _feedCache;
        private readonly IStore _store;
        private readonly IPackageManager _packageManager;

        /// <summary>
        /// Creates a new selections manager
        /// </summary>
        /// <param name="feedCache">Used to load <see cref="Feed"/>s containing the original <see cref="Implementation"/>s.</param>
        /// <param name="store">The locations to search for cached <see cref="Implementation"/>s.</param>
        /// <param name="packageManager">An external package manager that can install <see cref="PackageImplementation"/>s.</param>
        public SelectionsManager([NotNull] IFeedCache feedCache, [NotNull] IStore store, [NotNull] IPackageManager packageManager)
        {
            #region Sanity checks
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            if (store == null) throw new ArgumentNullException("store");
            if (packageManager == null) throw new ArgumentNullException("packageManager");
            #endregion

            _feedCache = feedCache;
            _store = store;
            _packageManager = packageManager;
        }
        #endregion

        /// <summary>
        /// Returns a list of any downloadable <see cref="ImplementationSelection"/>s that are missing from an <see cref="IStore"/>.
        /// </summary>
        /// <param name="selections">The selections to search for <see cref="ImplementationSelection"/>s that are missing.</param>
        /// <remarks>Feed files may be downloaded, no implementations are downloaded.</remarks>
        /// <exception cref="KeyNotFoundException">A <see cref="Feed"/> or <see cref="Implementation"/> is missing.</exception>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidDataException">The feed file could not be parsed.</exception>
        [NotNull, ItemNotNull]
        public IEnumerable<ImplementationSelection> GetUncachedSelections([NotNull] Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            foreach (ImplementationSelection implementation in selections.Implementations)
            {
                // Local paths are considered to be always cached
                if (!string.IsNullOrEmpty(implementation.LocalPath)) continue;

                if (implementation.ID.StartsWith(ExternalImplementation.PackagePrefix))
                {
                    if (!File.Exists(implementation.QuickTestFile) && !_packageManager.Lookup(implementation).IsInstalled)
                        yield return implementation;
                }
                else
                {
                    if (!_store.Contains(implementation.ManifestDigest))
                        yield return implementation;
                }
            }
        }

        /// <summary>
        /// Retrieves the original <see cref="Implementation"/>s these selections were based on.
        /// </summary>
        /// <param name="selections">The <see cref="ImplementationSelection"/>s to map back to <see cref="Implementation"/>s.</param>
        [NotNull, ItemNotNull]
        public IEnumerable<Implementation> GetImplementations([NotNull] IEnumerable<ImplementationSelection> selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            foreach (var selection in selections)
            {
                yield return selection.ID.StartsWith(ExternalImplementation.PackagePrefix)
                    ? _packageManager.Lookup(selection)
                    : _feedCache.GetFeed(selection.FromFeed ?? selection.InterfaceUri)[selection.ID].CloneImplementation();
            }
        }

        /// <summary>
        /// Combines <see cref="GetUncachedSelections"/> and <see cref="GetImplementations"/>.
        /// </summary>
        /// <param name="selections">The selections to search for <see cref="ImplementationSelection"/>s that are missing.</param>
        [NotNull, ItemNotNull]
        public ICollection<Implementation> GetUncachedImplementations([NotNull] Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            return GetImplementations(GetUncachedSelections(selections)).ToList();
        }
    }
}
