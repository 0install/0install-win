/*
 * Copyright 2010 Bastian Eicher, Simon E. Silva Lauinger
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
using ZeroInstall.Store.Implementation.Archive;
using Common.Utils;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Models a cache directory that stores <see cref="Implementation"/>s, each in its own sub-directory named by its <see cref="ManifestDigest"/>.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class DirectoryStore : IStore, IEquatable<DirectoryStore>
    {
        #region Properties
        /// <summary>
        /// The default directory in the user-profile to use for storing the cache.
        /// </summary>
        public static string UserProfileDirectory
        {
            get { return Path.Combine(Locations.GetUserCacheDir("0install.net"), "implementations"); }
        }

        /// <summary>
        /// The directory containing the cached <see cref="Implementation"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store based on the given path to a cache directory.
        /// </summary>
        /// <param name="path">A fully qualified directory path. The directory will be created if it doesn't exist yet.</param>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem for <paramref name="path"/> can not store file-changed times accurate to the second.</exception>
        public DirectoryStore(string path)
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
        /// Creates a new store using a directory in the user-profile.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        public DirectoryStore() : this(UserProfileDirectory)
        {}
        #endregion

        //--------------------//

        #region Contains
        /// <inheritdoc />
        public bool Contains(ManifestDigest manifestDigest)
        {
            // Check for all supported digest algorithms
            foreach (string digest in manifestDigest.AvailableDigests)
            {
                if (Directory.Exists(Path.Combine(DirectoryPath, digest))) return true;   
            }

            return false;
        }
        #endregion

        #region Get
        /// <inheritdoc />
        public string GetPath(ManifestDigest manifestDigest)
        {
            // Check for all supported digest algorithms
            foreach (string digest in manifestDigest.AvailableDigests)
            {
                string path = Path.Combine(DirectoryPath, digest);
                if (Directory.Exists(path)) return path;
            }

            throw new ImplementationNotFoundException(manifestDigest);
        }
        #endregion

        #region Add directory
        /// <inheritdoc />
        public void AddDirectory(string path, ManifestDigest manifestDigest, Action<IProgress> startingManifest)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Copy the source directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            FileUtils.CopyDirectory(path, tempDir, false);

            VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, startingManifest);
        }
        #endregion

        #region Add archive
        /// <inheritdoc />
        public void AddArchive(ArchiveFileInfo archiveInfo, ManifestDigest manifestDigest, Action<IProgress> startingExtraction, Action<IProgress> startingManifest)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(archiveInfo.Path)) throw new ArgumentException(Resources.MissingPath, "archiveInfo");
            #endregion

            // Extract to temporary directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            using (var extractor = Extractor.CreateExtractor(archiveInfo.MimeType, archiveInfo.Path, archiveInfo.StartOffset, tempDir))
            {
                extractor.SubDir = archiveInfo.SubDir;

                // Set up progress reporting
                if (startingExtraction != null) startingExtraction(extractor);
                
                try
                {
                    extractor.RunSync();

                    VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, startingManifest);
                }
                #region Error handling
                catch (Exception)
                {
                    // Remove extracted directory if validation or something else failed
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                    throw;
                }
                #endregion
            }
        }

        /// <inheritdoc />
        public void AddMultipleArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, Action<IProgress> startingExtraction, Action<IProgress> startingManifest)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            #endregion

            // Extract to temporary directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());

            try
            {
                // Extract archives "over each other" in order
                foreach (var archiveInfo in archiveInfos)
                {
                    using (var extractor = Extractor.CreateExtractor(archiveInfo.MimeType, archiveInfo.Path, archiveInfo.StartOffset, tempDir))
                    {
                        extractor.SubDir = archiveInfo.SubDir;

                        // Set up progress reporting
                        if (startingExtraction != null) startingExtraction(extractor);

                        extractor.RunSync();
                    }
                }

                VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, startingManifest);
            }
            #region Error handling
            catch (Exception)
            {
                // Remove extracted directory if validation or something else failed
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                throw;
            }
            #endregion
        }
        #endregion
        
        #region Verify and add
        /// <summary>
        /// Verifies the manifest digest of a directory temporarily stored inside the cache and moves it to the final location if it passes.
        /// </summary>
        /// <param name="tempID">The temporary identifier of the directory inside the cache.</param>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <param name="startingManifest">Callback to be called when a new manifest generation task (hashing files) is about to be started; may be <see langword="null"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the temporary directory doesn't match the <paramref name="manifestDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="tempID"/> cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        private void VerifyAndAdd(string tempID, ManifestDigest manifestDigest, Action<IProgress> startingManifest)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(tempID)) throw new ArgumentNullException("tempID");
            #endregion

            // Determine the digest method to use
            string expectedDigest = manifestDigest.BestDigest;
            if (string.IsNullOrEmpty(expectedDigest)) throw new ArgumentException(Resources.NoKnownDigestMethod, "manifestDigest");
            var format = ManifestFormat.FromPrefix(StringUtils.GetLeftPartAtFirstOccurrence(expectedDigest, '='));

            // Determine the source and target directories
            string source = Path.Combine(DirectoryPath, tempID);
            string target = Path.Combine(DirectoryPath, expectedDigest);

            if (Directory.Exists(target)) throw new ImplementationAlreadyInStoreException(manifestDigest);

            // Calculate the actual digest and compare it with the expected one
            string actualDigest = Manifest.CreateDotFile(source, format, startingManifest);
            if (actualDigest != expectedDigest) throw new DigestMismatchException(expectedDigest, actualDigest, File.ReadAllText(Path.Combine(source, ".manifest")));

            // Move directory to final store destination
            try { Directory.Move(source, target); }
            catch (IOException)
            {
                if (Directory.Exists(target)) throw new ImplementationAlreadyInStoreException(manifestDigest);
                throw;
            }

            // Prevent any further changes to the directory
            try { FileUtils.WriteProtection(target, true); }
            catch (UnauthorizedAccessException)
            {
                Log.Warn("Unable to enable write protection for " + target);
            }
        }
        #endregion

        #region Remove
        /// <inheritdoc />
        public void Remove(ManifestDigest manifestDigest)
        {
            string path = GetPath(manifestDigest);

            // Remove write protection
            FileUtils.WriteProtection(path, false);

            Directory.Delete(path, true);
        }
        #endregion

        //--------------------//

        #region Equality
        public bool Equals(DirectoryStore other)
        {
            if (ReferenceEquals(null, other)) return false;

            return DirectoryPath == other.DirectoryPath;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(DirectoryStore) && Equals((DirectoryStore)obj);
        }

        public override int GetHashCode()
        {
            return (DirectoryPath != null ? DirectoryPath.GetHashCode() : 0);
        }
        #endregion
    }
}
