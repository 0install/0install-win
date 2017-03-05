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
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations.Manifests;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

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
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
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
            if (store == null) throw new ArgumentNullException(nameof(store));
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
            if (store == null) throw new ArgumentNullException(nameof(store));
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
            if (store == null) throw new ArgumentNullException(nameof(store));
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

        /// <summary>
        /// Determines the local path of an implementation.
        /// </summary>
        /// <param name="store">The store to get the implementation from.</param>
        /// <param name="implementation">The implementation to be located.</param>
        /// <returns>A fully qualified path to the directory containing the implementation.</returns>
        /// <exception cref="ImplementationNotFoundException">The <paramref name="implementation"/> is not cached yet.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the store is not permitted.</exception>
        [NotNull]
        public static string GetPath([NotNull] this IStore store, [NotNull] ImplementationBase implementation)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            #endregion

            if (string.IsNullOrEmpty(implementation.LocalPath))
            {
                string path = store.GetPath(implementation.ManifestDigest);
                if (path == null) throw new ImplementationNotFoundException(implementation.ManifestDigest);
                return path;
            }
            else return implementation.LocalPath;
        }

        /// <summary>
        /// Removes all implementations from a store.
        /// </summary>
        /// <param name="store">The store to be purged.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">An implementation could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the store is not permitted.</exception>
        public static void Purge([NotNull] this IStore store, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            foreach (var manifestDigest in store.ListAll())
                store.Remove(manifestDigest, handler);
        }
    }
}
