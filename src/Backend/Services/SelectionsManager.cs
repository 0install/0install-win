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
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services
{
    /// <summary>
    /// Provides methods for filtering <see cref="Selections"/>.
    /// </summary>
    public class SelectionsManager : ISelectionsManager
    {
        #region Dependencies
        private readonly IFeedManager _feedManager;
        private readonly IStore _store;
        private readonly IPackageManager _packageManager;

        /// <summary>
        /// Creates a new selections manager
        /// </summary>
        /// <param name="feedManager">Used to load <see cref="Feed"/>s containing the original <see cref="Implementation"/>s.</param>
        /// <param name="store">The locations to search for cached <see cref="Implementation"/>s.</param>
        /// <param name="packageManager">An external package manager that can install <see cref="PackageImplementation"/>s.</param>
        public SelectionsManager([NotNull] IFeedManager feedManager, [NotNull] IStore store, [NotNull] IPackageManager packageManager)
        {
            #region Sanity checks
            if (feedManager == null) throw new ArgumentNullException(nameof(feedManager));
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
            #endregion

            _feedManager = feedManager;
            _store = store;
            _packageManager = packageManager;
        }
        #endregion

        /// <inheritdoc/>
        public IEnumerable<ImplementationSelection> GetUncachedSelections(Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException(nameof(selections));
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

        /// <inheritdoc/>
        public IEnumerable<Implementation> GetImplementations(IEnumerable<ImplementationSelection> selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException(nameof(selections));
            #endregion

            foreach (var selection in selections)
            {
                yield return selection.ID.StartsWith(ExternalImplementation.PackagePrefix)
                    ? _packageManager.Lookup(selection)
                    : _feedManager[selection.FromFeed ?? selection.InterfaceUri][selection.ID].CloneImplementation();
            }
        }
    }
}
