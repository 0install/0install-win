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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Manages a set of <see cref="IStore"/>s, allowing the retrieval of <see cref="Implementation"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class StoreSet : IStore
    {
        #region Properties
        // Preserve order, duplicate entries are not allowed
        private readonly C5.IList<IStore> _stores = new C5.HashedLinkedList<IStore>();
        /// <summary>
        /// A priority-sorted list of <see cref="IStore"/>s used to provide <see cref="Implementation"/>s.
        /// </summary>
        public C5.ISequenced<IStore> Stores { get { return _stores; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new implementation provider with a set of <see cref="IStore"/>s.
        /// </summary>
        /// <param name="stores"></param>
        public StoreSet(IEnumerable<IStore> stores)
        {
            #region Sanity checks
            if (stores == null) throw new ArgumentNullException("stores");
            #endregion

            // Defensive copy
            _stores.AddAll(stores);
        }
        #endregion

        //--------------------//

        #region Contains
        /// <summary>
        /// Determines whether one of the <see cref="Stores"/> contains a local copy of an <see cref="Implementation"/> identified by a specific <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest of the <see cref="Implementation"/> to check for.</param>
        public bool Contains(ManifestDigest manifestDigest)
        {
            foreach (IStore store in Stores)
            {
                // Check if any store contains the implementation
                if (store.Contains(manifestDigest)) return true;
            }

            // If we reach this, none of the stores contains the implementation
            return false;
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
            foreach (IStore store in Stores)
            {
                // Use the first store that contains the implementation
                if (store.Contains(manifestDigest)) return store.GetPath(manifestDigest);
            }

            // If we reach this, none of the stores contains the implementation
            throw new ImplementationNotFoundException(manifestDigest);
        }
        #endregion

        #region Add
        /// <summary>
        /// Moves a directory containing an <see cref="Implementation"/> into the best available <see cref="IStore"/> if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="source">The directory containing the <see cref="Implementation"/>.</param>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the <paramref name="source"/> directory doesn't match the <paramref name="manifestDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="source"/> directory cannot be moved or the digest cannot be calculated.</exception>
        public void Add(string source, ManifestDigest manifestDigest)
        {
            IOException lastIOError = null;
            foreach (IStore store in Stores)
            {
                try
                {
                    // Add implementation only to one store
                    store.Add(source, manifestDigest);
                    return;
                }
                catch (IOException ex)
                {
                    // Ignore IO errors and try the next store
                    lastIOError = ex;
                }
            }

            // If we reach this, the implementation couldn't be added to any store
            throw new IOException(Resources.UnableToAddImplementionToStore, lastIOError);
        }
        #endregion
    }
}
