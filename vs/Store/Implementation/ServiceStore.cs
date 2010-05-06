/*
 * Copyright 2010 Bastian Eicher
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
using System.IO;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Provides direct pass-through read-access to a <see cref="DirectoryStore"/> and service-mediated write-acess.
    /// </summary>
    public class ServiceStore : IStore
    {
        #region Variables
        /// <summary>The store to use for read-access.</summary>
        private readonly DirectoryStore _backingStore;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new service store using an existing <see cref="DirectoryStore"/>.
        /// </summary>
        /// <param name="backingStore">The store to use for read-access.</param>
        public ServiceStore(DirectoryStore backingStore)
        {
            _backingStore = backingStore;
        }
        #endregion

        //--------------------//

        #region Contains
        /// <summary>
        /// Determines whether this store contains a local copy of an <see cref="Implementation"/> identified by a specific <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest of the <see cref="Implementation"/> to check for.</param>
        public bool Contains(ManifestDigest manifestDigest)
        {
            return _backingStore.Contains(manifestDigest);
        }
        #endregion

        #region Get
        /// <summary>
        /// Determines the local path of an <see cref="Implementation"/> with a given <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> to look for.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the requested <see cref="Implementation"/> could not be found in this store.</exception>
        /// <returns></returns>
        public string GetPath(ManifestDigest manifestDigest)
        {
            return _backingStore.GetPath(manifestDigest);
        }
        #endregion

        #region Add
        /// <summary>
        /// Moves a directory containing an <see cref="Implementation"/> into this store if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="source">The directory containing the <see cref="Implementation"/>.</param>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the <paramref name="source"/> directory doesn't match the <paramref name="manifestDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="source"/> directory cannot be moved or the digest cannot be calculated.</exception>
        public void Add(string source, ManifestDigest manifestDigest)
        {
            // ToDo: Communicate with service
            throw new NotImplementedException();
        }
        #endregion
    }
}
