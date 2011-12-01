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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Combines multiple <see cref="IStore"/>s as a composite.
    /// </summary>
    /// <remarks>
    ///   <para>When adding new <see cref="Model.Implementation"/>s the last child <see cref="IStore"/> that doesn't throw an <see cref="UnauthorizedAccessException"/> is used.</para>
    ///   <para>When when retrieving existing <see cref="Model.Implementation"/>s the first child <see cref="IStore"/> that returns <see langword="true"/> for <see cref="IStore.Contains"/> is used.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class CompositeStore : MarshalByRefObject, IStore
    {
        #region Variables
        // Preserve order, duplicate entries are not allowed
        private readonly C5.IList<IStore> _stores = new C5.HashedLinkedList<IStore>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new composite implementation provider with a set of <see cref="IStore"/>s.
        /// </summary>
        /// <param name="stores">A priority-sorted list of <see cref="IStore"/>s. Queried last-to-first for adding new <see cref="Model.Implementation"/>s, first-to-last otherwise. Duplicates are ignored.</param>
        public CompositeStore(IEnumerable<IStore> stores)
        {
            #region Sanity checks
            if (stores == null) throw new ArgumentNullException("stores");
            #endregion

            // Defensive copy and remove duplicates
            foreach (var store in stores)
                if (store != null && !_stores.Contains(store)) _stores.Add(store);
        }

        /// <summary>
        /// Creates a new composite implementation provider with a set of <see cref="IStore"/>s.
        /// </summary>
        /// <param name="stores">A priority-sorted list of <see cref="IStore"/>s. Queried last-to-first for adding new <see cref="Model.Implementation"/>s, first-to-last otherwise.</param>
        public CompositeStore(params IStore[] stores) : this((IEnumerable<IStore>)stores)
        {}
        #endregion

        //--------------------//

        #region List all
        /// <inheritdoc />
        public IEnumerable<ManifestDigest> ListAll()
        {
            // Merge the lists from all contained stores, eliminating duplicates
            var result = new C5.TreeSet<ManifestDigest>();
            foreach (IStore store in _stores)
            {
                try
                {
                    result.AddSorted(store.ListAll());
                }
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
            foreach (IStore store in _stores)
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
            foreach (IStore store in _stores)
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
        public void AddDirectory(string path, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Find the last store the implementation can be added to (some might be write-protected)
            Exception innerException = null;
            foreach (IStore store in _stores.Backwards())
            {
                try
                {
                    // Try to add implementation to this store
                    store.AddDirectory(path, manifestDigest, handler);
                    return;
                }
                    #region Error handling
                catch (IOException ex)
                {
                    innerException = ex; // Remember the last error
                }
                catch (UnauthorizedAccessException ex)
                {
                    innerException = ex; // Remember the last error
                }
                #endregion
            }

            // If we reach this, the implementation couldn't be added to any store
            if (innerException != null) Log.Error(innerException.Message);
            throw new IOException(Resources.UnableToAddImplementionToStore, innerException);
        }
        #endregion

        #region Add archive
        /// <inheritdoc />
        public void AddArchive(ArchiveFileInfo archiveInfo, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(archiveInfo.Path)) throw new ArgumentException(Resources.MissingPath, "archiveInfo");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Find the last store the implementation can be added to (some might be write-protected)
            Exception innerException = null;
            foreach (IStore store in _stores.Backwards())
            {
                try
                {
                    // Try to add implementation to this store
                    store.AddArchive(archiveInfo, manifestDigest, handler);
                    return;
                }
                    #region Error handling
                catch (IOException ex)
                {
                    innerException = ex; // Remember the last error
                }
                catch (UnauthorizedAccessException ex)
                {
                    innerException = ex; // Remember the last error
                }
                #endregion
            }

            // If we reach this, the implementation couldn't be added to any store
            if (innerException != null) Log.Error(innerException.Message);
            throw new IOException(Resources.UnableToAddImplementionToStore, innerException);
        }

        /// <inheritdoc />
        public void AddMultipleArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Find the last store the implementation can be added to (some might be write-protected)
            Exception innerException = null;
            foreach (IStore store in _stores.Backwards())
            {
                try
                {
                    // Try to add implementation to this store
                    store.AddMultipleArchives(archiveInfos, manifestDigest, handler);
                    return;
                }
                    #region Error handling
                catch (IOException ex)
                {
                    innerException = ex; // Remember the last error
                }
                catch (UnauthorizedAccessException ex)
                {
                    innerException = ex; // Remember the last error
                }
                #endregion
            }

            // If we reach this, the implementation couldn't be added to any store
            throw new IOException(Resources.UnableToAddImplementionToStore, innerException);
        }
        #endregion

        #region Remove
        /// <inheritdoc />
        public void Remove(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            bool removed = false;
            foreach (IStore store in _stores)
            {
                // Remove from every that contains the implementation
                if (store.Contains(manifestDigest))
                {
                    store.Remove(manifestDigest, handler);
                    removed = true;
                }
            }
            if (!removed) throw new ImplementationNotFoundException(manifestDigest);
        }
        #endregion

        #region Optimise
        /// <inheritdoc />
        public void Optimise(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Try to optimize all contained stores
            foreach (IStore store in _stores)
            {
                try
                {
                    store.Optimise(handler);
                }
                    #region Sanity checks
                catch (IOException ex)
                {
                    Log.Error(ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Error(ex.Message);
                }
                #endregion
            }
        }
        #endregion

        #region Verify
        /// <inheritdoc />
        public void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Verify in all contained stores
            foreach (IStore store in _stores)
                store.Verify(manifestDigest, handler);
        }
        #endregion

        #region Audit
        /// <inheritdoc />
        public IEnumerable<DigestMismatchException> Audit(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Try to audit all contained stores
            foreach (IStore store in _stores)
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
