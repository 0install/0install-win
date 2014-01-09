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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Common.Tasks;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Helper methods for <see cref="IStore"/>s and paths.
    /// </summary>
    public static class StoreUtils
    {
        /// <summary>
        /// Determines whether a path looks like it is inside a store known by <see cref="ManifestFormat"/>.
        /// </summary>
        public static bool PathInAStore(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return ManifestFormat.All.Any(format => path.Contains(format.Prefix + format.Separator));
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.ListAll"/>, handling exceptions.
        /// </summary>
        public static IEnumerable<ManifestDigest> ListAllSafe(this IStore store)
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
                return new ManifestDigest[0];
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.ListAllTemp"/>, handling exceptions.
        /// </summary>
        public static IEnumerable<string> ListAllTempSafe(this IStore store)
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
                return new string[0];
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.GetPath"/>, handling exceptions.
        /// </summary>
        public static string GetPathSafe(this IStore store, ManifestDigest manifestDigest)
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

        /// <summary>
        /// Wrapper for <see cref="IStore.Remove"/>, handling exceptions.
        /// </summary>
        public static bool RemoveSafe(this IStore store, ManifestDigest manifestDigest)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            if (store.Contains(manifestDigest))
            {
                store.Remove(manifestDigest);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.Optimise"/>, handling exceptions.
        /// </summary>
        public static void OptimiseSafe(this IStore store, ITaskHandler handler)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            try
            {
                store.Optimise(handler);
            }
                #region Sanity checks
            catch (IOException ex)
            {
                Log.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
            }
            #endregion
        }
    }
}
