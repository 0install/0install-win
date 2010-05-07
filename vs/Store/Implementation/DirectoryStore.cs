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
using System.Security.Cryptography;
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
    public class DirectoryStore : IStore
    {
        #region Variables
        /// <summary>
        /// The directory containing the cached <see cref="Implementation"/>s.
        /// </summary>
        private readonly string _cacheDir;

        private readonly HashAlgorithm _sha1Algo = SHA1.Create(), _sha256Algo = SHA256.Create();
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
            // Check for all supported hashing algorithms
            if (Directory.Exists(Path.Combine(_cacheDir, "sha256=" + manifestDigest.Sha256))) return true;
            if (Directory.Exists(Path.Combine(_cacheDir, "sha1new=" + manifestDigest.Sha1New))) return true;
            if (Directory.Exists(Path.Combine(_cacheDir, "sha1=" + manifestDigest.Sha1))) return true;

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
            string path = Path.Combine(_cacheDir, "sha256=" + manifestDigest.Sha256);
            if (Directory.Exists(path)) return path;

            path = Path.Combine(_cacheDir, "sha1new=" + manifestDigest.Sha1New);
            if (Directory.Exists(path)) return path;

            path = Path.Combine(_cacheDir, "sha1=" + manifestDigest.Sha1);
            if (Directory.Exists(path)) return path;

            throw new ImplementationNotFoundException(manifestDigest);
        }
        #endregion

        #region Add
        /// <summary>
        /// Moves a directory containing an <see cref="Implementation"/> into this store if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="source">The directory containing the <see cref="Implementation"/>.</param>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the <paramref name="source"/> directory doesn't match the <paramref name="manifestDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="source"/> directory cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the directory is not permitted.</exception>
        public void Add(string source, ManifestDigest manifestDigest)
        {
            string hashID;
            string tempDir = FileHelper.GetUniqueFileName(_cacheDir);

            // Move source directory to temporary sub-directory and generate in-memory manifest from there
            // This prevents attackers from modifying the source directory between digest validation and moving to final store destination.
            Directory.Move(source, tempDir);

            try
            {
                #region Validate digest
                // Store the manifest to the disk to calculate its digest
                string manifestFile = Path.Combine(tempDir, ".manifest");
                switch (manifestDigest.BestMethod)
                {
                    case HashMethod.Sha1:
                    {
                        hashID = "sha1=" + manifestDigest.Sha1;
                        string sourceHash = OldManifest.Generate(tempDir, _sha1Algo).Save(manifestFile);
                        if (sourceHash != manifestDigest.Sha1)
                            throw new DigestMismatchException(hashID, "sha1=" + sourceHash);
                        break;
                    }

                    case HashMethod.Sha1New:
                    {
                        hashID = "sha1new=" + manifestDigest.Sha1New;
                        string sourceHash = NewManifest.Generate(tempDir, _sha1Algo).Save(manifestFile);
                        if (sourceHash != manifestDigest.Sha1New)
                            throw new DigestMismatchException(hashID, "sha1new=" + sourceHash);
                        break;
                    }

                    case HashMethod.Sha256:
                    {
                        hashID = "sha256=" + manifestDigest.Sha256;
                        string sourceHash = NewManifest.Generate(tempDir, _sha256Algo).Save(manifestFile);
                        if (sourceHash != manifestDigest.Sha256)
                            throw new DigestMismatchException(hashID, "sha256=" + sourceHash);
                        break;
                    }

                    default:
                        throw new ArgumentException(Resources.NoKnownHashes);
                }
                #endregion
            }
            #region Error handling
            catch (DigestMismatchException)
            {
                // Move the directory back to where it came from before passing the exception on
                Directory.Move(tempDir, source);
                throw;
            }
            #endregion

            // Move directory to final store destination
            Directory.Move(tempDir, Path.Combine(_cacheDir, hashID));
        }
        #endregion
    }
}
