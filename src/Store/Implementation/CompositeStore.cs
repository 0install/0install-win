/*
 * Copyright 2010-2012 Bastian Eicher
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
    /// Combines multiple <see cref="IStore"/>s as a composite. Adds memory caching for <see cref="IStore.Contains(ManifestDigest)"/>.
    /// </summary>
    /// <remarks>
    ///   <para>When adding new <see cref="Model.Implementation"/>s the last child <see cref="IStore"/> that doesn't throw an <see cref="UnauthorizedAccessException"/> is used.</para>
    ///   <para>When when retrieving existing <see cref="Model.Implementation"/>s the first child <see cref="IStore"/> that returns <see langword="true"/> for <see cref="IStore.Contains(ZeroInstall.Model.ManifestDigest)"/> is used.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class CompositeStore : MarshalByRefObject, IStore
    {
        #region Variables
        private readonly IStore[] _stores;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new composite implementation provider with a set of <see cref="IStore"/>s.
        /// </summary>
        /// <param name="stores">
        ///   A priority-sorted list of <see cref="IStore"/>s.
        ///   Queried last-to-first for adding new <see cref="Model.Implementation"/>s, first-to-last otherwise.
        ///   This array must _not_ be modified once it has been passed into this constructor!
        /// </param>
        public CompositeStore(params IStore[] stores)
        {
            #region Sanity checks
            if (stores == null) throw new ArgumentNullException("stores");
            #endregion

            _stores = stores;
        }
        #endregion

        //--------------------//

        #region List all
        /// <inheritdoc />
        public IEnumerable<ManifestDigest> ListAll()
        {
            // Merge the lists from all contained stores, eliminating duplicates
            var result = new C5.TreeSet<ManifestDigest>();
            foreach (var store in _stores)
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

        /// <inheritdoc />
        public IEnumerable<string> ListAllTemp()
        {
            // Merge the lists from all contained stores, eliminating duplicates
            var result = new C5.TreeSet<string>();
            foreach (var store in _stores)
            {
                try
                {
                    result.AddSorted(store.ListAllTemp());
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
            lock (_containsCache)
            {
                bool result;
                if (_containsCache.TryGetValue(manifestDigest, out result)) return result;
            }

            foreach (var store in _stores)
            {
                // Check if any store contains the implementation
                if (store.Contains(manifestDigest))
                {
                    lock (_containsCache) _containsCache[manifestDigest] = true;
                    return true;
                }
            }

            // If we reach this, none of the stores contains the implementation
            lock (_containsCache) _containsCache[manifestDigest] = false;
            return false;
        }

        /// <inheritdoc />
        public bool Contains(string directory)
        {
            foreach (var store in _stores)
            {
                // Check if any store contains the implementation
                if (store.Contains(directory)) return true;
            }

            // If we reach this, none of the stores contains the implementation
            return false;
        }
        #endregion

        #region Get
        /// <inheritdoc />
        public string GetPath(ManifestDigest manifestDigest)
        {
            foreach (var store in _stores)
            {
                // Use the first store that contains the implementation
                string path = store.GetPath(manifestDigest);
                if (path != null) return path;
            }

            // If we reach this, none of the stores contains the implementation
            return null;
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

            ClearCaches();

            // Find the last store the implementation can be added to (some might be write-protected)
            Exception innerException = null;
            for (int i = _stores.Length - 1; i >= 0; i--) // Iterate backwards
            {
                try
                {
                    // Try to add implementation to this store
                    _stores[i].AddDirectory(path, manifestDigest, handler);
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
            throw new IOException(Resources.UnableToAddImplementationToStore, innerException);
        }
        #endregion

        #region Add archive
        /// <inheritdoc />
        public void AddArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            ClearCaches();

            // Find the last store the implementation can be added to (some might be write-protected)
            Exception innerException = null;
            for (int i = _stores.Length - 1; i >= 0; i--) // Iterate backwards
            {
                try
                {
                    // Try to add implementation to this store
                    // ReSharper disable PossibleMultipleEnumeration
                    _stores[i].AddArchives(archiveInfos, manifestDigest, handler);
                    // ReSharper restore PossibleMultipleEnumeration
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
            throw new IOException(Resources.UnableToAddImplementationToStore, innerException);
        }
        #endregion

        #region Remove
        /// <inheritdoc />
        public void Remove(ManifestDigest manifestDigest)
        {
            ClearCaches();

            bool removed = false;
            foreach (var store in _stores)
            {
                // Remove from every store that contains the implementation
                if (store.Contains(manifestDigest))
                {
                    store.Remove(manifestDigest);
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
            foreach (var store in _stores)
            {
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
        #endregion

        #region Verify
        /// <inheritdoc />
        public void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Verify in all contained stores
            bool verified = false;
            foreach (var store in _stores)
            {
                if (!store.Contains(manifestDigest)) continue;
                store.Verify(manifestDigest, handler);
                verified = true;
            }

            // If we reach this, none of the stores contains the implementation
            if (!verified) throw new ImplementationNotFoundException(manifestDigest);
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
            foreach (var store in _stores)
            {
                var problems = store.Audit(handler);
                if (problems != null)
                {
                    // Combine problems from all stores
                    foreach (var problem in problems)
                        yield return problem;
                }
            }
        }
        #endregion

        #region Caches
        // ToDo: Replace with lock-less multi-threaded dictionary
        private readonly Dictionary<ManifestDigest, bool> _containsCache = new Dictionary<ManifestDigest, bool>();

        /// <inheritdoc />
        public void ClearCaches()
        {
            lock (_containsCache) _containsCache.Clear();
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the Store in the form "CompositeStore: # of children". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "CompositeStore: " + _stores.Length + " children";
        }
        #endregion
    }
}
