/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Combines multiple <see cref="IStore"/>s as a composite. Adds memory caching for <see cref="IStore.Contains(ManifestDigest)"/>.
    /// </summary>
    /// <remarks>
    ///   <para>When adding new <see cref="Store.Model.Implementation"/>s the last child <see cref="IStore"/> that doesn't throw an <see cref="UnauthorizedAccessException"/> is used.</para>
    ///   <para>When when retrieving existing <see cref="Store.Model.Implementation"/>s the first child <see cref="IStore"/> that returns <c>true</c> for <see cref="IStore.Contains(ZeroInstall.Store.Model.ManifestDigest)"/> is used.</para>
    /// </remarks>
    public class CompositeStore : MarshalByRefObject, IStore
    {
        #region Properties
        private readonly IStore[] _stores;

        /// <summary>
        /// The <see cref="IStore"/>s this store is internally composed of.
        /// </summary>
        public IEnumerable<IStore> Stores { get { return new ReadOnlyCollection<IStore>(_stores); } }

        /// <inheritdoc/>
        public StoreKind Kind { get { return StoreKind.ReadWrite; } }

        /// <inheritdoc/>
        public string DirectoryPath { get { return null; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new composite implementation provider with a set of <see cref="IStore"/>s.
        /// </summary>
        /// <param name="stores">
        ///   A priority-sorted list of <see cref="IStore"/>s.
        ///   Queried last-to-first for adding new <see cref="Store.Model.Implementation"/>s, first-to-last otherwise.
        /// </param>
        public CompositeStore([NotNull, ItemNotNull] IEnumerable<IStore> stores)
        {
            #region Sanity checks
            if (stores == null) throw new ArgumentNullException("stores");
            #endregion

            _stores = stores.ToArray();
            _containsCache = new TransparentCache<ManifestDigest, bool>(manifestDigest => _stores.Any(store => store.Contains(manifestDigest)));
        }
        #endregion

        //--------------------//

        #region List all
        /// <inheritdoc/>
        public IEnumerable<ManifestDigest> ListAll()
        {
            // Merge the lists from all contained stores, eliminating duplicates
            return new HashSet<ManifestDigest>(_stores.SelectMany(x => x.ListAllSafe()));
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListAllTemp()
        {
            // Merge the lists from all contained stores, eliminating duplicates
            return new HashSet<string>(_stores.SelectMany(x => x.ListAllTempSafe()), StringComparer.Ordinal);
        }
        #endregion

        #region Contains
        private readonly TransparentCache<ManifestDigest, bool> _containsCache;

        /// <inheritdoc/>
        public bool Contains(ManifestDigest manifestDigest)
        {
            return _containsCache[manifestDigest];
        }

        /// <inheritdoc/>
        public bool Contains(string directory)
        {
            return _stores.Any(store => store.Contains(directory));
        }

        /// <inheritdoc/>
        public void Flush()
        {
            _containsCache.Clear();
        }
        #endregion

        #region Get path
        /// <inheritdoc/>
        public string GetPath(ManifestDigest manifestDigest)
        {
            // Use the first store that contains the implementation
            return _stores.Select(store => store.GetPathSafe(manifestDigest))
                .WhereNotNull().FirstOrDefault();
        }
        #endregion

        #region Add directory
        /// <inheritdoc/>
        public string AddDirectory(string path, ManifestDigest manifestDigest, ITaskHandler handler)
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
                    return store.AddDirectory(path, manifestDigest, handler);
                }
                    #region Error handling
                catch (ImplementationAlreadyInStoreException)
                {
                    throw; // Do not try any further
                }
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

            // If we reach this, the implementation could not be added to any store
            if (innerException != null) innerException.Rethrow();
            throw new InvalidOperationException();
        }
        #endregion

        #region Add archive
        /// <inheritdoc/>
        public string AddArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler)
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
                    return store.AddArchives(archiveInfos, manifestDigest, handler);
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
            if (innerException != null) innerException.Rethrow();
            throw new InvalidOperationException();
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public bool Remove(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            Flush();

            // Remove from _every_ store that contains the implementation
            bool removed = false;
            foreach (var store in _stores.Reverse())
                removed |= store.Remove(manifestDigest, handler);

            return removed;
        }
        #endregion

        #region Optimise
        /// <inheritdoc/>
        public long Optimise(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Try to optimize all contained stores
            return _stores.Reverse().Sum(x => x.Optimise(handler));
        }
        #endregion

        #region Verify
        /// <inheritdoc/>
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

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the names of the child stores. Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "CompositeStore: " + StringUtils.Join(", ", _stores.Select(x => x.ToString()));
        }
        #endregion
    }
}
