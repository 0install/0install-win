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
using Common.Helpers;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Models a cache directory residing on the disk.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class DirectoryStore : IStore, IEquatable<DirectoryStore>
    {
        #region Variables
        /// <summary>
        /// The directory containing the cached <see cref="Implementation"/>s.
        /// </summary>
        private readonly string _cacheDir;
        #endregion

        #region Properties
        /// <summary>
        /// The default directory in the user-profile to use for storing the cache.
        /// </summary>
        public static string UserProfileDirectory
        {
            get { return Locations.GetUserCacheDir(Path.Combine("0install.net", "implementations")); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store based on the given path to a cache folder.
        /// </summary>
        /// <param name="path">A fully qualified directory path. The directory will be created if it doesn't exist yet.</param>
        public DirectoryStore(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion
            
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            _cacheDir = path;
        }

        /// <summary>
        /// Creates a new store using a directory in the user-profile.
        /// </summary>
        public DirectoryStore() : this(UserProfileDirectory)
        {}
        #endregion

        //--------------------//

        #region Contains
        /// <summary>
        /// Determines whether this store contains a local copy of an <see cref="Implementation"/> identified by a specific <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest of the <see cref="Implementation"/> to check for.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the directory is not permitted.</exception>
        public bool Contains(ManifestDigest manifestDigest)
        {
            // Check for all supported digest algorithms
            foreach (string digest in manifestDigest.AvailableDigests)
            {
                if (Directory.Exists(Path.Combine(_cacheDir, digest))) return true;   
            }

            return false;
        }
        #endregion

        #region Get
        /// <summary>
        /// Determines the local path of an <see cref="Implementation"/> with a given <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> to look for.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the requested <see cref="Implementation"/> could not be found in this store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the directory is not permitted.</exception>
        /// <returns>A fully qualified path to the directory containing the <see cref="Implementation"/>.</returns>
        public string GetPath(ManifestDigest manifestDigest)
        {
            // Check for all supported digest algorithms
            foreach (string digest in manifestDigest.AvailableDigests)
            {
                string path = Path.Combine(_cacheDir, digest);
                if (Directory.Exists(path)) return path;
            }

            throw new ImplementationNotFoundException(manifestDigest);
        }
        #endregion

        #region Add
        /// <summary>
        /// Moves a directory containing an <see cref="Implementation"/> into this store if it matches the provided <see cref="ManifestDigest"/>
        /// else it deletes the directory.
        /// </summary>
        /// <param name="source">The directory containing the <see cref="Implementation"/>.</param>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the <paramref name="source"/> directory doesn't match the <paramref name="manifestDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="source"/> directory cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the directory is not permitted.</exception>
        public void Add(string source, ManifestDigest manifestDigest)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            #endregion

            string expectedDigest = manifestDigest.BestDigest;
            if (string.IsNullOrEmpty(expectedDigest)) throw new ArgumentException(Resources.NoKnownDigestMethod, "manifestDigest");

            try
            {
                // Select the manifest format to use
                var format = ManifestFormat.FromPrefix(StringHelper.GetLeftPartAtFirstOccurrence(expectedDigest, '='));

                // Calculate the actual digest and compare it with the expected one
                string actualDigest = Manifest.CreateDotFile(source, format);
                if (actualDigest != expectedDigest) throw new DigestMismatchException(expectedDigest, actualDigest);
            }
            catch (Exception)
            {
                Directory.Delete(source, true);
                throw;
            }

            // Move directory to final store destination
            Directory.Move(source, Path.Combine(_cacheDir, expectedDigest));
        }
        #endregion

        //--------------------//

        #region Equality
        public bool Equals(DirectoryStore other)
        {
            if (ReferenceEquals(null, other)) return false;

            return _cacheDir == other._cacheDir;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(DirectoryStore) && Equals((DirectoryStore)obj);
        }

        public override int GetHashCode()
        {
            return (_cacheDir != null ? _cacheDir.GetHashCode() : 0);
        }
        #endregion
    }
}
