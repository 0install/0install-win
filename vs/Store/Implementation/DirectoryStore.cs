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
using Common.Utils;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Models a cache directory that stores <see cref="Implementation"/>s, each in its own sub-directory named by its <see cref="ManifestDigest"/>.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class DirectoryStore : MarshalByRefObject, IStore, IEquatable<DirectoryStore>
    {
        #region Properties
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
        /// Creates a new store using the default path (generally in the user-profile).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public DirectoryStore() : this(Locations.GetCachePath("0install.net", "implementations"))
        {}
        #endregion

        //--------------------//
        
        #region Verify and add
        /// <summary>
        /// Verifies the manifest digest of a directory temporarily stored inside the cache and moves it to the final location if it passes.
        /// </summary>
        /// <param name="tempID">The temporary identifier of the directory inside the cache.</param>
        /// <param name="expectedDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress; may be <see langword="null"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="expectedDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the temporary directory doesn't match the <paramref name="expectedDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="tempID"/> cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Implementation"/> with the specified <paramref name="expectedDigest"/> in the store.</exception>
        private void VerifyAndAdd(string tempID, ManifestDigest expectedDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(tempID)) throw new ArgumentNullException("tempID");
            #endregion

            // Determine the digest method to use
            string expectedDigestValue = expectedDigest.BestDigest;
            if (string.IsNullOrEmpty(expectedDigestValue)) throw new ArgumentException(Resources.NoKnownDigestMethod, "expectedDigest");

            // Determine the source and target directories
            string source = Path.Combine(DirectoryPath, tempID);
            string target = Path.Combine(DirectoryPath, expectedDigestValue);

            if (Directory.Exists(target)) throw new ImplementationAlreadyInStoreException(expectedDigest);

            // Calculate the actual digest, compare it with the expected one and create a manifest file
            VerifyDirectory(source, expectedDigest, handler).Save(Path.Combine(source, ".manifest"));

            // Move directory to final store destination
            try { Directory.Move(source, target); }
            catch (IOException)
            {
                if (Directory.Exists(target)) throw new ImplementationAlreadyInStoreException(expectedDigest);
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

        #region Verify directory
        /// <summary>
        /// Verifies the manifest digest of a directory.
        /// </summary>
        /// <param name="directory">The directory to generate a <see cref="Manifest"/> for.</param>
        /// <param name="expectedDigest">The digest the <see cref="Manifest"/> of the <paramref name="directory"/> should have.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress; may be <see langword="null"/>.</param>
        /// <returns>The generated <see cref="Manifest"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="expectedDigest"/> indicates no known hash methods.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="directory"/> could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the <paramref name="directory"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the <paramref name="directory"/> doesn't match the <paramref name="expectedDigest"/>.</exception>
        public static Manifest VerifyDirectory(string directory, ManifestDigest expectedDigest, IImplementationHandler handler)
        {
            Action<IProgress> startingManifest = null;
            if (handler != null) startingManifest = handler.StartingManifest;

            var format = ManifestFormat.FromPrefix(expectedDigest.BestPrefix);
            var actualManifest = Manifest.Generate(directory, format, startingManifest);

            string expectedDigestValue = expectedDigest.BestDigest;
            string actualDigestValue = actualManifest.CalculateDigest();
            if (actualDigestValue != expectedDigestValue)
            {
                Manifest expectedManifest = null;
                try { expectedManifest = Manifest.Load(Path.Combine(directory, ".manifest"), ManifestFormat.FromPrefix(expectedDigest.BestPrefix)); }
                #region Error handling
                catch (FormatException) {}
                catch (IOException) {}
                catch (UnauthorizedAccessException) {}
                #endregion
                throw new DigestMismatchException(expectedDigestValue, expectedManifest, actualDigestValue, actualManifest);
            }

            return actualManifest;
        }
        #endregion

        //--------------------//

        #region List all
        /// <inheritdoc />
        public IEnumerable<ManifestDigest> ListAll()
        {
            // Find all directories whose names contain an equals sign
            string[] directories = Directory.GetDirectories(DirectoryPath, "*=*");

            // Turn into a C-sorted list
            Array.Sort(directories, StringComparer.Ordinal);

            var result = new List<ManifestDigest>();
            for (int i = 0; i < directories.Length; i++)
            {
                // Exclude (temporary) dot-directories
                if (directories[i].StartsWith(".")) continue;

                result.Add(new ManifestDigest(Path.GetFileName(directories[i])));
            }

            return result;
        }
        #endregion

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
        public void AddDirectory(string path, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            if (Contains(manifestDigest)) throw new ImplementationAlreadyInStoreException(manifestDigest);

            // Copy the source directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            FileUtils.CopyDirectory(path, tempDir, false);

            VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, handler);
        }
        #endregion

        #region Add archive
        /// <inheritdoc />
        public void AddArchive(ArchiveFileInfo archiveInfo, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(archiveInfo.Path)) throw new ArgumentException(Resources.MissingPath, "archiveInfo");
            #endregion

            if (Contains(manifestDigest)) throw new ImplementationAlreadyInStoreException(manifestDigest);

            // Extract to temporary directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            using (var extractor = Extractor.CreateExtractor(archiveInfo.MimeType, archiveInfo.Path, archiveInfo.StartOffset, tempDir))
            {
                extractor.SubDir = archiveInfo.SubDir;

                // Set up progress reporting
                if (handler != null) handler.StartingExtraction(extractor);

                try
                {
                    extractor.RunSync();

                    VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, handler);
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
        public void AddMultipleArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, IImplementationHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            #endregion

            if (Contains(manifestDigest)) throw new ImplementationAlreadyInStoreException(manifestDigest);

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
                        if (handler != null) handler.StartingExtraction(extractor);

                        extractor.RunSync();
                    }
                }

                VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, handler);
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

        #region Remove
        /// <inheritdoc />
        public void Remove(ManifestDigest manifestDigest)
        {
            string path = GetPath(manifestDigest);

            // Remove write protection
            FileUtils.WriteProtection(path, false);

            var tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());

            // Move the directory to be deleted to a temporary directory to ensure the removal operation is atomic
            Directory.Move(path, tempDir);

            // Actually delete the files
            Directory.Delete(tempDir, true);
        }
        #endregion

        #region Optimise
        /// <inheritdoc />
        public void Optimise(IImplementationHandler handler)
        {
            // ToDo: Implemenet
        }
        #endregion

        #region Audit
        /// <inheritdoc />
        public IEnumerable<DigestMismatchException> Audit(IImplementationHandler handler)
        {
            // Iterate through all entries - their names are the expected digest values
            foreach (ManifestDigest digest in ListAll())
            {
                string directory = Path.Combine(DirectoryPath, digest.BestDigest);

                // Calculate the actual digest and compare it with the expected one
                DigestMismatchException problem = null;
                try { VerifyDirectory(directory, digest, handler); }
                catch (DigestMismatchException ex)
                {
                    problem = ex;
                }
                if (problem != null) yield return problem;
            }
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
