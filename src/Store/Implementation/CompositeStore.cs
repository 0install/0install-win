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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
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
        /// </param>
        public CompositeStore(IEnumerable<IStore> stores)
        {
            #region Sanity checks
            if (stores == null) throw new ArgumentNullException("stores");
            #endregion

            _stores = stores.ToArray();
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
                result.AddSorted(store.ListAllSafe());

            return result;
        }

        /// <inheritdoc />
        public IEnumerable<string> ListAllTemp()
        {
            // Merge the lists from all contained stores, eliminating duplicates
            var result = new C5.TreeSet<string>(StringComparer.Ordinal);
            foreach (var store in _stores)
                result.AddSorted(store.ListAllTempSafe());

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

            // Check if any store contains the implementation
            if (_stores.Any(store => store.Contains(manifestDigest)))
            {
                lock (_containsCache) _containsCache[manifestDigest] = true;
                return true;
            }

            // If we reach this, none of the stores contains the implementation
            lock (_containsCache) _containsCache[manifestDigest] = false;
            return false;
        }

        /// <inheritdoc />
        public bool Contains(string directory)
        {
            // Check if any store contains the implementation
            return _stores.Any(store => store.Contains(directory));
        }
        #endregion

        #region Get path
        /// <inheritdoc />
        public string GetPath(ManifestDigest manifestDigest)
        {
            // Use the first store that contains the implementation
            return _stores.Select(store => store.GetPathSafe(manifestDigest)).FirstOrDefault(path => path != null);
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

            Flush();

            // Find the last store the implementation can be added to (some might be write-protected)
            Exception innerException = null;
            foreach (var store in _stores.Reverse())
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
                catch (RemotingException ex)
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

            Flush();

            // Find the last store the implementation can be added to (some might be write-protected)
            Exception innerException = null;
            foreach (var store in _stores.Reverse())
            {
                try
                {
                    // Try to add implementation to this store
                    // ReSharper disable PossibleMultipleEnumeration
                    store.AddArchives(archiveInfos, manifestDigest, handler);
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
                catch (RemotingException ex)
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
            Flush();

            // Remove from every store that contains the implementation
            bool removed = false;
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var store in _stores.Reverse())
                removed |= store.RemoveSafe(manifestDigest);
            // ReSharper restore LoopCanBeConvertedToQuery
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
                store.OptimiseSafe(handler);
        }
        #endregion

        #region Verify
        /// <inheritdoc />
        public void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Verify in every store that contains the implementation
            bool verified = false;
            foreach (var store in _stores.Where(store => store.Contains(manifestDigest)))
            {
                store.Verify(manifestDigest, handler);
                verified = true;
            }
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
            return _stores.Select(store => store.Audit(handler))
                .Where(problems => problems != null).SelectMany(problems => problems);
        }
        #endregion

        #region Caches
        // TODO: Replace with lock-less multi-threaded dictionary
        private readonly Dictionary<ManifestDigest, bool> _containsCache = new Dictionary<ManifestDigest, bool>();

        /// <inheritdoc />
        public void Flush()
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
