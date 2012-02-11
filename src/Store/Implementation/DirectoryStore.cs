/*
 * Copyright 2010-2012 Bastian Eicher, Simon E. Silva Lauinger
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
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Models a cache directory that stores <see cref="Model.Implementation"/>s, each in its own sub-directory named by its <see cref="ManifestDigest"/>.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class DirectoryStore : MarshalByRefObject, IStore, IEquatable<DirectoryStore>
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
        /// <param name="path">A directory path. The directory will be created if it doesn't exist yet.</param>
        /// <exception cref="IOException">Thrown if the directory could not be created or if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating the directory <paramref name="path"/> is not permitted.</exception>
        public DirectoryStore(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            try
            {
                path = Path.GetFullPath(path);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            catch (NotSupportedException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion

            DirectoryPath = path;

            // Ensure the store is backed by a filesystem that can store file-changed times accurate to the second (otherwise ManifestDigets will break)
            try
            {
                if (FileUtils.DetermineTimeAccuracy(path) > 0)
                    throw new IOException(Resources.InsufficientFSTimeAccuracy);
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore if we cannot verify the timestamp accuracy of read-only stores
            }
        }
        #endregion

        //--------------------//

        #region Verify and add
        private readonly object _renameLock = new object();

        /// <summary>
        /// Verifies the <see cref="ManifestDigest"/> of a directory temporarily stored inside the cache and moves it to the final location if it passes.
        /// </summary>
        /// <param name="tempID">The temporary identifier of the directory inside the cache.</param>
        /// <param name="expectedDigest">The digest the <see cref="Model.Implementation"/> is supposed to match.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="expectedDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the temporary directory doesn't match the <paramref name="expectedDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="tempID"/> cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Model.Implementation"/> with the specified <paramref name="expectedDigest"/> in the store.</exception>
        private void VerifyAndAdd(string tempID, ManifestDigest expectedDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(tempID)) throw new ArgumentNullException("tempID");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Determine the digest method to use
            string expectedDigestValue = expectedDigest.BestDigest;
            if (string.IsNullOrEmpty(expectedDigestValue)) throw new ArgumentException(Resources.NoKnownDigestMethod, "expectedDigest");

            // Determine the source and target directories
            string source = Path.Combine(DirectoryPath, tempID);
            string target = Path.Combine(DirectoryPath, expectedDigestValue);

            // Calculate the actual digest, compare it with the expected one and create a manifest file
            VerifyDirectory(source, expectedDigest, handler).Save(Path.Combine(source, ".manifest"));

            lock (_renameLock) // Prevent race-conditions when adding the same digest twice
            {
                if (Directory.Exists(target)) throw new ImplementationAlreadyInStoreException(expectedDigest);

                // Move directory to final store destination
                try
                {
                    Directory.Move(source, target);
                }
                catch (IOException ex)
                {
                    // ToDo: Make language independent
                    if (ex.Message.Contains("already exists")) throw new ImplementationAlreadyInStoreException(expectedDigest);
                    throw;
                }
            }

            // Prevent any further changes to the directory
            try
            {
                FileUtils.EnableWriteProtection(target);
            }
                #region Error handling
            catch (IOException)
            {
                Log.Warn("Unable to enable write protection for " + target);
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warn("Unable to enable write protection for " + target);
            }
            #endregion
        }
        #endregion

        #region Verify directory
        /// <summary>
        /// Verifies the <see cref="ManifestDigest"/> of a directory.
        /// </summary>
        /// <param name="directory">The directory to generate a <see cref="Manifest"/> for.</param>
        /// <param name="expectedDigest">The digest the <see cref="Manifest"/> of the <paramref name="directory"/> should have.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>The generated <see cref="Manifest"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="expectedDigest"/> indicates no known hash methods.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="directory"/> could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the <paramref name="directory"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the <paramref name="directory"/> doesn't match the <paramref name="expectedDigest"/>.</exception>
        public static Manifest VerifyDirectory(string directory, ManifestDigest expectedDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException("directory");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var format = ManifestFormat.FromPrefix(expectedDigest.BestPrefix);
            var actualManifest = Manifest.Generate(directory, format, handler, expectedDigest);

            string expectedDigestValue = expectedDigest.BestDigest;
            string actualDigestValue = actualManifest.CalculateDigest();
            if (actualDigestValue != expectedDigestValue)
                throw new DigestMismatchException(expectedDigestValue, null, actualDigestValue, actualManifest);

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

            // Convert directory names to manifest digests
            return Array.ConvertAll(directories, directory => new ManifestDigest(Path.GetFileName(directory)));
        }

        /// <inheritdoc />
        public IEnumerable<string> ListAllTemp()
        {
            // Find all directories
            string[] directories = Directory.GetDirectories(DirectoryPath);

            // Turn into a C-sorted list
            Array.Sort(directories, StringComparer.Ordinal);

            // Filter non-temporary directories
            var result = new List<string>();
            for (int i = 0; i < directories.Length; i++)
                if (!directories[i].Contains("=")) result.Add(Path.GetFileName(directories[i]));
            return result;
        }
        #endregion

        #region Contains
        /// <inheritdoc />
        public bool Contains(ManifestDigest manifestDigest)
        {
            // Check for all supported digest algorithms
            foreach (string digest in manifestDigest.AvailableDigests)
                if (Directory.Exists(Path.Combine(DirectoryPath, digest))) return true;

            return false;
        }

        /// <inheritdoc />
        public bool Contains(string directory)
        {
            return Directory.Exists(Path.Combine(DirectoryPath, directory));
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

            return null;
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

            if (Contains(manifestDigest)) throw new ImplementationAlreadyInStoreException(manifestDigest);

            string tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            try
            {
                // Copy the source directory inside the cache so it can be validated safely (no manipulation of directory while validating)
                try
                {
                    FileUtils.CopyDirectory(path, tempDir, true, false);
                }
                    #region Error handling
                catch (IOException ex)
                {
                    // Wrap too generic exceptions
                    // ToDo: Make language independent
                    if (ex.Message.StartsWith("Access") && ex.Message.EndsWith("is denied.")) throw new UnauthorizedAccessException(ex.Message, ex);

                    // Pass other exceptions through
                    throw;
                }
                #endregion

                VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, handler);
            }
                #region Error handling
            catch (Exception)
            {
                // Remove temporary directory before passing exception on
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                throw;
            }
            #endregion
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

            if (Contains(manifestDigest)) throw new ImplementationAlreadyInStoreException(manifestDigest);

            // Extract to temporary directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            string tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());

            try
            {
                // Extract archives "over each other" in order
                foreach (var archiveInfo in archiveInfos)
                {
                    using (var extractor = Extractor.CreateExtractor(archiveInfo.MimeType, archiveInfo.Path, archiveInfo.StartOffset, tempDir))
                    {
                        extractor.SubDir = archiveInfo.SubDir;

                        // Defer task to handler
                        handler.RunTask(extractor, manifestDigest);
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
        public void Remove(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string path = GetPath(manifestDigest);
            if (path == null) throw new ImplementationNotFoundException(manifestDigest);

            // Defer deleting to handler
            handler.RunTask(
                new SimpleTask(string.Format(Resources.DeletingImplementation, manifestDigest.BestDigest),
                    delegate
                    {
                        try
                        {
                            FileUtils.DisableWriteProtection(path);

                            // Move the directory to be deleted to a temporary directory to ensure the removal operation is atomic
                            string tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
                            Directory.Move(path, tempDir);

                            Directory.Delete(tempDir, true);
                        }
                            #region Error handling
                        catch (UnauthorizedAccessException ex)
                        {
                            // Wrap exception since only certain exception types are allowed in tasks
                            throw new IOException(ex.Message, ex);
                        }
                        #endregion
                    }), manifestDigest);
        }

        /// <inheritdoc />
        public void Remove(string directory, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException("directory");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string path = Path.Combine(DirectoryPath, directory);
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException();

            // Defer deleting to handler
            handler.RunTask(
                new SimpleTask(string.Format(Resources.DeletingDirectory, directory),
                    delegate
                    {
                        try
                        {
                            FileUtils.DisableWriteProtection(path);
                            Directory.Delete(path, true);
                        }
                            #region Error handling
                        catch (UnauthorizedAccessException ex)
                        {
                            // Wrap exception since only certain exception types are allowed in tasks
                            throw new IOException(ex.Message, ex);
                        }
                        #endregion
                    }), null);
        }
        #endregion

        #region Optimise
        /// <inheritdoc />
        public void Optimise(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // ToDo: Implement
        }
        #endregion

        #region Verify
        /// <inheritdoc />
        public void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            VerifyDirectory(Path.Combine(DirectoryPath, manifestDigest.BestDigest), manifestDigest, handler);
        }
        #endregion

        #region Audit
        /// <inheritdoc />
        public IEnumerable<DigestMismatchException> Audit(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Iterate through all entries - their names are the expected digest values
            foreach (ManifestDigest digest in ListAll())
            {
                // Calculate the actual digest and compare it with the expected one
                DigestMismatchException problem = null;
                try
                {
                    Verify(digest, handler);
                }
                catch (DigestMismatchException ex)
                {
                    problem = ex;
                }
                if (problem != null) yield return problem;
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the Store in the form "DirectoryStore: DirectoryPath". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "DirectoryStore: " + DirectoryPath;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(DirectoryStore other)
        {
            if (other == null) return false;

            return DirectoryPath == other.DirectoryPath;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(DirectoryStore) && Equals((DirectoryStore)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (DirectoryPath != null ? DirectoryPath.GetHashCode() : 0);
        }
        #endregion
    }
}
