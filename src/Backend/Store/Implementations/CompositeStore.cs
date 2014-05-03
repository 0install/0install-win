/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using System.Runtime.Remoting;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Combines multiple <see cref="IStore"/>s as a composite. Adds memory caching for <see cref="IStore.Contains(ManifestDigest)"/>.
    /// </summary>
    /// <remarks>
    ///   <para>When adding new <see cref="Store.Model.Implementation"/>s the last child <see cref="IStore"/> that doesn't throw an <see cref="UnauthorizedAccessException"/> is used.</para>
    ///   <para>When when retrieving existing <see cref="Store.Model.Implementation"/>s the first child <see cref="IStore"/> that returns <see langword="true"/> for <see cref="IStore.Contains(ZeroInstall.Store.Model.ManifestDigest)"/> is used.</para>
    /// </remarks>
    public class CompositeStore : IStore
    {
        #region Variables
        private readonly IStore[] _stores;
        private readonly TransparentCache<ManifestDigest, bool> _containsCache;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new composite implementation provider with a set of <see cref="IStore"/>s.
        /// </summary>
        /// <param name="stores">
        ///   A priority-sorted list of <see cref="IStore"/>s.
        ///   Queried last-to-first for adding new <see cref="Store.Model.Implementation"/>s, first-to-last otherwise.
        /// </param>
        public CompositeStore(IEnumerable<IStore> stores)
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
            return new SortedSet<ManifestDigest>(_stores.SelectMany(x => x.ListAllSafe()));
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListAllTemp()
        {
            // Merge the lists from all contained stores, eliminating duplicates
            return new SortedSet<string>(_stores.SelectMany(x => x.ListAllTempSafe()), StringComparer.Ordinal);
        }
        #endregion

        #region Contains
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
            if (innerException != null) Log.Error(innerException.Message);
            throw new IOException(Resources.UnableToAddImplementationToStore, innerException);
        }
        #endregion

        #region Add archive
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public long Optimise(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Try to optimize all contained stores
            return _stores.Sum(x => x.Optimise(handler));
        }
        #endregion

        #region Verify
        /// <inheritdoc/>
        public void Verify(ManifestDigest manifestDigest, IServiceHandler handler)
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
        /// Returns the Store in the form "CompositeStore: # of children". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "CompositeStore: " + _stores.Length + " children";
        }
        #endregion
    }
}
