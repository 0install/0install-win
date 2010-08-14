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
using Common;
using Common.Archive;
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
                if (FileHelper.DetermineTimeAccuracy(path) > 0)
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
                if (Directory.Exists(Path.Combine(DirectoryPath, digest))) return true;   
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
                string path = Path.Combine(DirectoryPath, digest);
                if (Directory.Exists(path)) return path;
            }

            throw new ImplementationNotFoundException(manifestDigest);
        }
        #endregion

        #region Verify and add
        /// <summary>
        /// Verifies the manifest digest of a directory temporarily stored inside the cache and moves it to the final location if it passes.
        /// </summary>
        /// <param name="tempID">The temporary identifier of the directory inside the cache.</param>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <param name="manifestProgress">Callback to track the progress of generating the manifest (hashing files); may be <see langword="null"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the temporary directory doesn't match the <paramref name="manifestDigest"/>.</exception>
        private void VerifyAndAdd(string tempID, ManifestDigest manifestDigest, ProgressCallback manifestProgress)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(tempID)) throw new ArgumentNullException("tempID");
            #endregion

            // Determine the digest method to use
            string expectedDigest = manifestDigest.BestDigest;
            if (string.IsNullOrEmpty(expectedDigest)) throw new ArgumentException(Resources.NoKnownDigestMethod, "manifestDigest");
            var format = ManifestFormat.FromPrefix(StringHelper.GetLeftPartAtFirstOccurrence(expectedDigest, '='));

            // Locate the source directory
            string source = Path.Combine(DirectoryPath, tempID);

            // Calculate the actual digest and compare it with the expected one
            string actualDigest = Manifest.CreateDotFile(source, format, manifestProgress);
            if (actualDigest != expectedDigest) throw new DigestMismatchException(expectedDigest, actualDigest);

            // Move directory to final store destination
            string target = Path.Combine(DirectoryPath, expectedDigest);
            Directory.Move(source, target);

            // Prevent any further changes to the directory
            try { FileHelper.WriteProtection(target); }
            catch (UnauthorizedAccessException)
            {
                Log.Warn("Unable to enable write protection for " + target);
            }
        }
        #endregion

        #region Add directory
        public void AddDirectory(string path, ManifestDigest manifestDigest, ProgressCallback manifestProgress)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Copy the source directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            FileHelper.CopyDirectory(path, tempDir);

            VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, manifestProgress);
        }
        #endregion

        #region Add archive
        public void AddArchive(ArchiveFileInfo archiveInfo, ManifestDigest manifestDigest, Action<IProgress> startingExtraction, ProgressCallback manifestProgress)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(archiveInfo.Path)) throw new ArgumentException(Resources.MissingPath, "archiveInfo");
            #endregion

            // Extract to temporary directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            using (var extractor = Extractor.CreateExtractor(archiveInfo.MimeType, archiveInfo.Path, archiveInfo.StartOffset, tempDir))
            {
                extractor.SubDir = archiveInfo.SubDir;

                // Prepare progress reporting
                if (startingExtraction != null) startingExtraction(extractor);
                
                extractor.RunSync();

                try
                {
                    VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, manifestProgress);
                }
                #region Error handling
                catch (Exception)
                {
                    // Remove extracted directory if validation or something else failed
                    Directory.Delete(tempDir, true);
                    throw;
                }
                #endregion
            }
        }

        public void AddMultipleArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, Action<IProgress> startingExtraction, ProgressCallback manifestProgress)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            #endregion

            // Extract to temporary directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            foreach (var archiveInfo in archiveInfos)
            {
                using (var extractor = Extractor.CreateExtractor(archiveInfo.MimeType, archiveInfo.Path, archiveInfo.StartOffset, tempDir))
                {
                    extractor.SubDir = archiveInfo.SubDir;

                    // Prepare progress reporting
                    if (startingExtraction != null) startingExtraction(extractor);

                    extractor.RunSync();
                }
            }

            try
            {
                VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, manifestProgress);
            }
            #region Error handling
            catch (Exception)
            {
                // Remove extracted directory if validation or something else failed
                Directory.Delete(tempDir, true);
                throw;
            }
            #endregion
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
