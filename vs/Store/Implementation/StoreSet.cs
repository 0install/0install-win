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
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Combines multiple <see cref="IStore"/>s as a composite.
    /// </summary>
    /// <remarks>
    ///   <para>When adding new <see cref="Implementation"/>s the first child <see cref="IStore"/> that doesn't throw an <see cref="UnauthorizedAccessException"/> is used.</para>
    ///   <para>When when retreiving existing <see cref="Implementation"/>s the first child <see cref="IStore"/> that returns <see langword="true"/> for <see cref="IStore.Contains"/> is used.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class StoreSet : MarshalByRefObject, IStore
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

        #region List all
        /// <inheritdoc />
        public IEnumerable<ManifestDigest> ListAll()
        {
            // Merge the lists from all contained stores, eliminating duplicates
            var result = new C5.TreeSet<ManifestDigest>();
            foreach (IStore store in Stores)
            {
                try { result.AddSorted(store.ListAll()); }
                catch (UnauthorizedAccessException)
                {
                    // Ignore authorization errors, since listing is not a critical task
                }
            }

            return result;
        }
        #endregion

        #region Contains
        /// <inheritdoc />
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
        /// <inheritdoc />
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

        #region Add directory
        /// <inheritdoc />
        public void AddDirectory(string path, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Find the first store the implementation can be added to (some might be write-protected)
            UnauthorizedAccessException innerException = null;
            foreach (IStore store in Stores)
            {
                try
                {
                    // Try to add implementation to this store
                    store.AddDirectory(path, manifestDigest, handler);
                    return;
                }
                #region Error handling
                catch (UnauthorizedAccessException ex)
                {
                    // Remember the first authorization error and try the next store
                    if (innerException == null) innerException = ex;
                }
                #endregion
            }

            // If we reach this, the implementation couldn't be added to any store
            throw new UnauthorizedAccessException(Resources.UnableToAddImplementionToStore, innerException);
        }
        #endregion

        #region Add archive
        /// <inheritdoc />
        public void AddArchive(ArchiveFileInfo archiveInfo, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(archiveInfo.Path)) throw new ArgumentException(Resources.MissingPath, "archiveInfo");
            #endregion

            // Find the first store the implementation can be added to (some might be write-protected)
            UnauthorizedAccessException innerException = null;
            foreach (IStore store in Stores)
            {
                try
                {
                    // Try to add implementation to this store
                    store.AddArchive(archiveInfo, manifestDigest, handler);
                    return;
                }
                #region Error handling
                catch (UnauthorizedAccessException ex)
                {
                    // Remember the first authorization error and try the next store
                    if (innerException == null) innerException = ex;
                }
                #endregion
            }

            // If we reach this, the implementation couldn't be added to any store
            throw new UnauthorizedAccessException(Resources.UnableToAddImplementionToStore, innerException);
        }

        /// <inheritdoc />
        public void AddMultipleArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            #endregion

            // Find the first store the implementation can be added to (some might be write-protected)
            UnauthorizedAccessException innerException = null;
            foreach (IStore store in Stores)
            {
                try
                {
                    // Try to add implementation to this store
                    store.AddMultipleArchives(archiveInfos, manifestDigest, handler);
                    return;
                }
                #region Error handling
                catch (UnauthorizedAccessException ex)
                {
                    // Remember the first authorization error and try the next store
                    if (innerException == null) innerException = ex;
                }
                #endregion
            }

            // If we reach this, the implementation couldn't be added to any store
            throw new UnauthorizedAccessException(Resources.UnableToAddImplementionToStore, innerException);
        }
        #endregion

        #region Remove
        /// <inheritdoc />
        public void Remove(ManifestDigest manifestDigest)
        {
            foreach (IStore store in Stores)
            {
                // Remove from every that contains the implementation
                if (store.Contains(manifestDigest)) store.Remove(manifestDigest);
            }
        }
        #endregion

        #region Optimise
        /// <inheritdoc />
        public void Optimise()
        {
            // Try to optimize all contained stores
            foreach (IStore store in Stores)
            {
                try { store.Optimise(); }
                catch (UnauthorizedAccessException)
                {
                    // Ignore authorization errors, since optimisation is not a critical task
                }
            }
        }
        #endregion

        #region Audit
        /// <inheritdoc />
        public IEnumerable<DigestMismatchException> Audit(IImplementationHandler handler)
        {
            // Try to audit all contained stores
            foreach (IStore store in Stores)
            {
                var problems = store.Audit(handler);
                if (problems != null)
                {
                    // Combine problems from all stores
                    foreach (DigestMismatchException problem in problems)
                        yield return problem;
                }
            }
        }
        #endregion
    }
}
