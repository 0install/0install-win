/*
 * Copyright 2010-2011 Bastian Eicher
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
using Common.Storage;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Creates <see cref="IStore"/> instances.
    /// </summary>
    public static class StoreProvider
    {
        /// <summary>
        /// Creates an <see cref="IStore"/> instance that uses the default cache locations.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static IStore CreateDefault()
        {
            return new CompositeStore(GetStores());
        }

        /// <summary>
        /// Returns a list of <see cref="IStore"/>s representing all local cache directories and the <see cref="ServiceStore"/>.
        /// </summary>
        private static IEnumerable<IStore> GetStores()
        {
            foreach (var path in Locations.GetCachePath("0install.net", "implementations"))
                yield return new DirectoryStore(path);
            yield return new ServiceStore();
        }
    }
}
