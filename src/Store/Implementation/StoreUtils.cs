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
using System.Linq;

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
    }
}
