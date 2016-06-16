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
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Creates <see cref="IStore"/> instances.
    /// </summary>
    public static class StoreFactory
    {
        /// <summary>
        /// Creates an <see cref="IStore"/> instance that uses the default cache locations.
        /// </summary>
        /// <exception cref="IOException">There was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file or one of the stores was not permitted.</exception>
        [NotNull]
        public static IStore CreateDefault()
        {
            return new CompositeStore(GetStores());
        }

        /// <summary>
        /// Returns a list of <see cref="IStore"/>s representing all local cache directories.
        /// </summary>
        /// <exception cref="IOException">A directory could not be created or the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Creating a directory was not permitted.</exception>
        [NotNull, ItemNotNull]
        private static IEnumerable<IStore> GetStores()
        {
            var stores = new List<IStore>();

            foreach (var path in StoreConfig.GetImplementationDirs())
            {
                try
                {
                    stores.Add(new DirectoryStore(path));
                }
                    #region Error handling
                catch (IOException ex)
                {
                    // Wrap exception to add context information
                    throw new IOException(string.Format(Resources.ProblemAccessingStore, path), ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Wrap exception to add context information
                    throw new UnauthorizedAccessException(string.Format(Resources.ProblemAccessingStore, path), ex);
                }
                #endregion
            }

            if (WindowsUtils.IsWindowsNT && !Locations.IsPortable)
                stores.Add(new IpcStore());

            return stores;
        }
    }
}
