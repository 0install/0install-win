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
using System.Linq;
using JetBrains.Annotations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Helper methods for <see cref="IStore"/>s and paths.
    /// </summary>
    public static class StoreUtils
    {
        /// <summary>
        /// Determines whether a path looks like it is inside a store known by <see cref="ManifestFormat"/>.
        /// </summary>
        public static bool PathInAStore([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return ManifestFormat.All.Any(format => path.Contains(format.Prefix + format.Separator));
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.ListAll"/>, handling exceptions.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<ManifestDigest> ListAllSafe([NotNull] this IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            try
            {
                return store.ListAll();
            }
                #region Error handling
            catch (UnauthorizedAccessException)
            {
                // Ignore authorization errors since listing is not a critical task
                return Enumerable.Empty<ManifestDigest>();
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.ListAllTemp"/>, handling exceptions.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<string> ListAllTempSafe([NotNull] this IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            try
            {
                return store.ListAllTemp();
            }
                #region Error handling
            catch (UnauthorizedAccessException)
            {
                // Ignore authorization errors since listing is not a critical task
                return Enumerable.Empty<string>();
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.GetPath"/>, handling exceptions.
        /// </summary>
        [CanBeNull]
        public static string GetPathSafe([NotNull] this IStore store, ManifestDigest manifestDigest)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            try
            {
                return store.GetPath(manifestDigest);
            }
                #region Error handling
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            #endregion
        }
    }
}
