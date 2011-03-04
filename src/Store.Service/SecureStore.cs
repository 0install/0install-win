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
using System.IO;
using Common;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Service.Properties;

namespace ZeroInstall.Store.Service
{
    /// <summary>
    /// Provides a background service to add new entries to a store that requires elevated privileges to write.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public sealed class SecureStore : MarshalByRefObject, IStore
    {
        #region Properties
        /// <summary>
        /// The directory containing the cached <see cref="Model.Implementation"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store based on the given path to a cache directory.
        /// </summary>
        /// <param name="path">A fully qualified directory path. The directory will be created if it doesn't exist yet.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="path"/> is invalid.</exception>
        /// <exception cref="IOException">Thrown if the directory <paramref name="path"/> could not be created.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating the directory <paramref name="path"/> is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem for <paramref name="path"/> can not store file-changed times accurate to the second.</exception>
        public SecureStore(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion
            
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            DirectoryPath = path;

            // Ensure the store is backed by a filesystem that can store file-changed times accurate to the second (otherwise ManifestDigets will break)
            try
            {
                if (FileUtils.DetermineTimeAccuracy(path) > 0)
                    throw new InvalidOperationException(Resources.InsufficientFSTimeAccuracy);
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore if we cannot verify the time accuracy of read-only stores
            }
        }

        /// <summary>
        /// Creates a new store using the default path (generally in the user-profile).
        /// </summary>
        /// <exception cref="IOException">Thrown if the directory could not be created.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        public SecureStore() : this(Locations.GetCachePath("0install.net", "implementations"))
        {}
        #endregion

        //--------------------//

        #region List all
        /// <inheritdoc />
        public IEnumerable<ManifestDigest> ListAll()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Contains
        /// <inheritdoc />
        public bool Contains(ManifestDigest manifestDigest)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Get
        /// <inheritdoc />
        public string GetPath(ManifestDigest manifestDigest)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Add directory
        /// <inheritdoc />
        public void AddDirectory(string path, ManifestDigest manifestDigest, IIOHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            throw new NotImplementedException();
        }
        #endregion

        #region Add archive
        /// <inheritdoc />
        public void AddArchive(ArchiveFileInfo archiveInfo, ManifestDigest manifestDigest, IIOHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(archiveInfo.Path)) throw new ArgumentException(Resources.MissingPath, "archiveInfo");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void AddMultipleArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, IIOHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            throw new NotImplementedException();
        }
        #endregion

        #region Remove
        /// <inheritdoc />
        public void Remove(ManifestDigest manifestDigest, IIOHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            throw new NotImplementedException();
        }
        #endregion

        #region Optimise
        /// <inheritdoc />
        public void Optimise(IIOHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // ToDo: Implemenet
        }
        #endregion

        #region Verify
        /// <inheritdoc />
        public void Verify(ManifestDigest manifestDigest, IIOHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // ToDo: Implemenet
        }
        #endregion

        #region Audit
        /// <inheritdoc />
        public IEnumerable<DigestMismatchException> Audit(IIOHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // ToDo: Implemenet
            return null;
        }
        #endregion
    }
}
