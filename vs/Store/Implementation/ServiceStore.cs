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
using System.IO;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Uses a background service to add new entries to a backing <see cref="DirectoryStore"/> that requires elevated privileges to write.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class ServiceStore : MarshalByRefObject, IStore
    {
        #region Variables
        /// <summary>The store to use for read-access.</summary>
        private readonly DirectoryStore _backingStore;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new service store based on the given path to a cache folder.
        /// </summary>
        /// <param name="path">A fully qualified directory path. The directory must already exist.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown if there is no directory at <paramref name="path"/>.</exception>
        public ServiceStore(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException();
            #endregion

            _backingStore = new DirectoryStore(path);
        }
        #endregion

        //--------------------//

        #region List all
        /// <inheritdoc />
        public IEnumerable<string> ListAll()
        {
            return _backingStore.ListAll();
        }
        #endregion

        #region Contains
        /// <inheritdoc />
        public bool Contains(ManifestDigest manifestDigest)
        {
            return _backingStore.Contains(manifestDigest);
        }
        #endregion

        #region Get
        /// <inheritdoc />
        public string GetPath(ManifestDigest manifestDigest)
        {
            return _backingStore.GetPath(manifestDigest);
        }
        #endregion

        #region Add directory
        /// <inheritdoc />
        public void AddDirectory(string path, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            throw new NotImplementedException();
        }
        #endregion

        #region Add archive
        /// <inheritdoc />
        public void AddArchive(ArchiveFileInfo archiveInfo, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(archiveInfo.Path)) throw new ArgumentException(Resources.MissingPath, "archiveInfo");
            #endregion

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void AddMultipleArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            #endregion

            throw new NotImplementedException();
        }
        #endregion

        #region Remove
        /// <inheritdoc />
        public void Remove(ManifestDigest manifestDigest)
        {
            throw new NotImplementedException(); ;
        }
        #endregion

        #region Optimise
        /// <inheritdoc />
        public void Optimise()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
