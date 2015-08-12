/*
 * Copyright 2010-2016 Bastian Eicher, Simon E. Silva Lauinger
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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Manages a cache directory that stores <see cref="Implementation"/>s, each in its own sub-directory named by its <see cref="ManifestDigest"/>.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class DirectoryStore : MarshalNoTimeout, IStore, IEquatable<DirectoryStore>
    {
        /// <inheritdoc/>
        public StoreKind Kind { get; }

        /// <inheritdoc/>
        [NotNull]
        public string DirectoryPath { get; }

        /// <summary>Controls whether implementation directories are made write-protected once added to the cache to prevent unintentional modification (which would invalidate the manifest digests).</summary>
        private readonly bool _useWriteProtection;

        /// <summary>Indicates whether <see cref="DirectoryPath"/> is located on a filesystem with support for Unixoid features such as executable bits.</summary>
        private readonly bool _isUnixFS;

        #region Constructor
        /// <summary>
        /// Creates a new store using a specific path to a cache directory.
        /// </summary>
        /// <param name="path">A fully qualified directory path. The directory will be created if it doesn't exist yet.</param>
        /// <param name="useWriteProtection">Controls whether implementation directories are made write-protected once added to the cache to prevent unintentional modification (which would invalidate the manifest digests).</param>
        /// <exception cref="IOException">The directory <paramref name="path"/> could not be created or the underlying filesystem can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Creating the directory <paramref name="path"/> is not permitted.</exception>
        public DirectoryStore([NotNull] string path, bool useWriteProtection = true)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            #endregion

            try
            {
                DirectoryPath = Path.GetFullPath(path);
                if (!Directory.Exists(DirectoryPath)) Directory.CreateDirectory(DirectoryPath);
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

            Kind = DetermineKind(DirectoryPath);
            _useWriteProtection = useWriteProtection;
            _isUnixFS = FlagUtils.IsUnixFS(DirectoryPath);

            if (Kind == StoreKind.ReadWrite)
            {
                try
                {
                    if (!_isUnixFS) FlagUtils.MarkAsNoUnixFS(DirectoryPath);
                    if (_useWriteProtection && WindowsUtils.IsWindowsNT) WriteDeleteInfoFile(DirectoryPath);
                }
                    #region Error handling
                catch (IOException ex)
                {
                    // Writing these files is not critical
                    Log.Warn(ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Writing these files is not critical
                    Log.Warn(ex);
                }
                #endregion
            }
        }

        private static StoreKind DetermineKind(string path)
        {
            try
            {
                // Ensure the store is backed by a filesystem that can store file-changed times accurate to the second (otherwise ManifestDigets will break)
                if (FileUtils.DetermineTimeAccuracy(path) > 0)
                    throw new IOException(Resources.InsufficientFSTimeAccuracy);

                // As a side-effect the test above told us we have write access
                return StoreKind.ReadWrite;
            }
            catch (UnauthorizedAccessException)
            {
                // The test could not be performed because we have no write access, which is fine
                return StoreKind.ReadOnly;
            }
        }

        private static void WriteDeleteInfoFile(string path)
        {
            File.WriteAllText(
                Path.Combine(path, Resources.DeleteInfoFileName + ".txt"),
                string.Format(Resources.DeleteInfoFileContent, path), Encoding.UTF8);
        }
        #endregion

        //--------------------//

        #region Temp dir
        /// <summary>
        /// Creates a temporary directory within <see cref="DirectoryPath"/>.
        /// </summary>
        /// <returns>The path to the new temporary directory.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Returns a new value on each call")]
        protected virtual string GetTempDir()
        {
            string path = Path.Combine(DirectoryPath, Path.GetRandomFileName());
            Log.Debug("Creating temp directory for extracting: " + path);
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Deletes a temporary directory.
        /// </summary>
        /// <param name="path">The path to the temporary directory.</param>
        protected virtual void DeleteTempDir([NotNull] string path)
        {
            if (Directory.Exists(path))
            {
                Log.Debug("Deleting left-over temp directory: " + path);
                Directory.Delete(path, recursive: true);
            }
        }
        #endregion

        #region Write protection
        /// <summary>
        /// Makes a directory read-only using platform-specific mechanisms. Logs any errors and continues.
        /// </summary>
        /// <param name="path">The directory to protect.</param>
        public static void EnableWriteProtection([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            #endregion

            try
            {
                Log.Debug("Enabling write protection for: " + path);
                FileUtils.EnableWriteProtection(path);
            }
                #region Error handling
            catch (IOException)
            {
                Log.Warn(string.Format(Resources.UnableToWriteProtect, path));
            }
            catch (UnauthorizedAccessException)
            {
                // Only warn if even an Admin is unable to set ACLs
                if (WindowsUtils.IsAdministrator) Log.Warn(string.Format(Resources.UnableToWriteProtect, path));
                else Log.Info(string.Format(Resources.UnableToWriteProtect, path));
            }
            catch (InvalidOperationException)
            {
                Log.Warn(string.Format(Resources.UnableToWriteProtect, path));
            }
            #endregion
        }

        /// <summary>
        /// Removes write-protection from a directory read-only using platform-specific mechanisms. Logs any errors and continues.
        /// </summary>
        /// <param name="path">The directory to unprotect.</param>
        public static void DisableWriteProtection([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            #endregion

            try
            {
                Log.Debug("Disabling write protection for: " + path);
                FileUtils.DisableWriteProtection(path);
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex);
            }
            #endregion
        }
        #endregion

        #region Verify and add
        private readonly object _renameLock = new object();

        /// <summary>
        /// Verifies the <see cref="ManifestDigest"/> of a directory temporarily stored inside the cache and moves it to the final location if it passes.
        /// </summary>
        /// <param name="tempID">The temporary identifier of the directory inside the cache.</param>
        /// <param name="expectedDigest">The digest the <see cref="Store.Model.Implementation"/> is supposed to match.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>The final location of the directory.</returns>
        /// <exception cref="DigestMismatchException">The temporary directory doesn't match the <paramref name="expectedDigest"/>.</exception>
        /// <exception cref="IOException"><paramref name="tempID"/> cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">There is already an <see cref="Store.Model.Implementation"/> with the specified <paramref name="expectedDigest"/> in the store.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        protected virtual string VerifyAndAdd(string tempID, ManifestDigest expectedDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(tempID)) throw new ArgumentNullException(nameof(tempID));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            // Determine the digest method to use
            string expectedDigestValue = expectedDigest.Best;
            if (string.IsNullOrEmpty(expectedDigestValue)) throw new NotSupportedException(Resources.NoKnownDigestMethod);

            // Determine the source and target directories
            string source = Path.Combine(DirectoryPath, tempID);
            string target = Path.Combine(DirectoryPath, expectedDigestValue);

            if (_isUnixFS) FlagUtils.ConvertToFS(source);

            // Calculate the actual digest, compare it with the expected one and create a manifest file
            VerifyDirectory(source, expectedDigest, handler).Save(Path.Combine(source, Manifest.ManifestFile));

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
                    // TODO: Make language independent
                    if (ex.Message.Contains("already exists")) throw new ImplementationAlreadyInStoreException(expectedDigest);
                    throw;
                }
            }

            // Prevent any further changes to the directory
            if (_useWriteProtection) EnableWriteProtection(target);
            return target;
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
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">The <paramref name="directory"/> could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the <paramref name="directory"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">The <paramref name="directory"/> doesn't match the <paramref name="expectedDigest"/>.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public static Manifest VerifyDirectory(string directory, ManifestDigest expectedDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException(nameof(directory));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            string expectedDigestValue = expectedDigest.Best;
            if (string.IsNullOrEmpty(expectedDigestValue)) throw new NotSupportedException(Resources.NoKnownDigestMethod);
            var format = ManifestFormat.FromPrefix(expectedDigestValue);

            var generator = new ManifestGenerator(directory, format) {Tag = expectedDigest};
            handler.RunTask(generator);
            var actualManifest = generator.Manifest;
            string actualDigestValue = actualManifest.CalculateDigest();

            string manifestFilePath = Path.Combine(directory, Manifest.ManifestFile);
            var expectedManifest = File.Exists(manifestFilePath) ? Manifest.Load(manifestFilePath, format) : null;

            if (actualDigestValue != expectedDigestValue)
                throw new DigestMismatchException(expectedDigestValue, actualDigestValue, expectedManifest, actualManifest);

            return actualManifest;
        }
        #endregion

        //--------------------//

        #region List all
        /// <inheritdoc/>
        public IEnumerable<ManifestDigest> ListAll()
        {
            if (!Directory.Exists(DirectoryPath)) return Enumerable.Empty<ManifestDigest>();

            var result = new List<ManifestDigest>();
            foreach (string path in Directory.GetDirectories(DirectoryPath))
            {
                Debug.Assert(path != null);
                var digest = new ManifestDigest();
                digest.ParseID(Path.GetFileName(path));
                if (digest.Best != null) result.Add(new ManifestDigest(Path.GetFileName(path)));
            }
            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListAllTemp()
        {
            if (!Directory.Exists(DirectoryPath)) return Enumerable.Empty<string>();

            var result = new List<string>();
            foreach (string path in Directory.GetDirectories(DirectoryPath))
            {
                Debug.Assert(path != null);
                try
                {
                    // ReSharper disable once ObjectCreationAsStatement
                    new ManifestDigest(Path.GetFileName(path));
                }
                catch (NotSupportedException)
                {
                    // Anything that is not a valid digest is considered a temp directory
                    result.Add(path);
                }
            }
            return result;
        }
        #endregion

        #region Contains
        /// <inheritdoc/>
        public bool Contains(ManifestDigest manifestDigest)
        {
            // Check for all supported digest algorithms
            return manifestDigest.AvailableDigests.Any(digest => Directory.Exists(Path.Combine(DirectoryPath, digest)));
        }

        /// <inheritdoc/>
        public bool Contains(string directory)
        {
            return Directory.Exists(Path.Combine(DirectoryPath, directory));
        }

        /// <inheritdoc/>
        public void Flush()
        {
            // No internal caching
        }
        #endregion

        #region Get
        /// <inheritdoc/>
        public string GetPath(ManifestDigest manifestDigest)
        {
            // Check for all supported digest algorithms
            return manifestDigest.AvailableDigests.Select(digest => Path.Combine(DirectoryPath, digest)).FirstOrDefault(Directory.Exists);
        }
        #endregion

        #region Add
        /// <inheritdoc/>
        public string AddDirectory(string path, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (manifestDigest.Best == null) throw new ArgumentException("No known digest method", nameof(manifestDigest));
            #endregion

            if (Contains(manifestDigest)) throw new ImplementationAlreadyInStoreException(manifestDigest);
            Log.Info("Caching implementation: " + manifestDigest.Best);

            // Copy to temporary directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            string tempDir = GetTempDir();
            try
            {
                // Copy the source directory inside the cache so it can be validated safely (no manipulation of directory while validating)
                try
                {
                    handler.RunTask(new CopyDirectoryPosix(path, tempDir) {Tag = manifestDigest});
                }
                    #region Error handling
                catch (IOException ex)
                {
                    // Wrap too generic exceptions
                    // TODO: Make language independent
                    if (ex.Message.StartsWith("Access") && ex.Message.EndsWith("is denied.")) throw new UnauthorizedAccessException(ex.Message, ex);

                    // Pass other exceptions through
                    throw;
                }
                #endregion

                return VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, handler);
            }
            finally
            {
                DeleteTempDir(tempDir);
            }
        }

        /// <inheritdoc/>
        public string AddArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException(nameof(archiveInfos));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (manifestDigest.Best == null) throw new ArgumentException("No known digest method", nameof(manifestDigest));
            #endregion

            if (Contains(manifestDigest)) throw new ImplementationAlreadyInStoreException(manifestDigest);
            Log.Info("Caching implementation: " + manifestDigest.Best);

            // Extract to temporary directory inside the cache so it can be validated safely (no manipulation of directory while validating)
            string tempDir = GetTempDir();
            try
            {
                // Extract archives "over each other" in order
                foreach (var archiveInfo in archiveInfos)
                {
                    try
                    {
                        using (var extractor = ArchiveExtractor.Create(archiveInfo.Path, tempDir, archiveInfo.MimeType, archiveInfo.StartOffset))
                        {
                            extractor.SubDir = archiveInfo.SubDir;
                            extractor.Destination = archiveInfo.Destination;
                            extractor.Tag = manifestDigest;
                            handler.RunTask(extractor);
                        }
                    }
                        #region Error handling
                    catch (IOException ex)
                    {
                        string source = archiveInfo.OriginalSource?.ToStringRfc() ?? archiveInfo.Path;
                        throw new IOException(string.Format(Resources.FailedToExtractArchive, source), ex);
                    }
                    #endregion
                }

                return VerifyAndAdd(Path.GetFileName(tempDir), manifestDigest, handler);
            }
            finally
            {
                DeleteTempDir(tempDir);
            }
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public virtual bool Remove(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            string path = GetPath(manifestDigest);
            if (path == null) return false;
            if (Kind == StoreKind.ReadOnly && !WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminToRemove);
            if (path == Locations.InstallBase && WindowsUtils.IsWindows)
            {
                Log.Warn(Resources.NoStoreSelfRemove);
                return false;
            }

            if (WindowsUtils.IsWindowsVista) UseRestartManager(path, handler);

            handler.RunTask(new SimpleTask(string.Format(Resources.DeletingImplementation, manifestDigest), () =>
            {
                DisableWriteProtection(path);

                string tempDir = Path.Combine(DirectoryPath, Path.GetRandomFileName());
                Log.Info("Attempting atomic delete: " + path);
                Directory.Move(path, tempDir);
                Directory.Delete(tempDir, recursive: true);
            }));

            return true;
        }

        /// <summary>
        /// Ensures that there are no applications running with open file handles in <paramref name="path"/>.
        /// </summary>
        /// <returns>A Restart Manager session that is to be disposed once exclusive access to the directory is no longer required; may be <c>null</c>.</returns>
        private static void UseRestartManager(string path, ITaskHandler handler)
        {
            try
            {
                using (var restartManager = new WindowsRestartManager())
                {
                    // Look for handles to well-known executable types in the top-level directory
                    // Searching for all file types and/or in subdirectories takes too long
                    restartManager.RegisterResources(Directory.GetFiles(path, "*.exe"));
                    restartManager.RegisterResources(Directory.GetFiles(path, "*.dll"));

                    var apps = restartManager.ListApps(handler);
                    if (apps.Length != 0)
                    {
                        string appsList = string.Join(Environment.NewLine, apps);
                        if (handler.Ask(Resources.FilesInUse + @" " + Resources.FilesInUseAskClose + Environment.NewLine + appsList,
                            defaultAnswer: false, alternateMessage: Resources.FilesInUse + @" " + Resources.FilesInUseInform + Environment.NewLine + appsList))
                            restartManager.ShutdownApps(handler);
                        else throw new OperationCanceledException();
                    }
                }
            }
                #region Error handling
            catch (Win32Exception ex)
            {
                Log.Error(ex);
            }
            catch (TimeoutException ex)
            {
                Log.Warn(ex);
            }
            #endregion
        }
        #endregion

        #region Optimise
        /// <inheritdoc/>
        public virtual long Optimise(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            if (!Directory.Exists(DirectoryPath)) return 0;
            if (Kind == StoreKind.ReadOnly && !WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminToOptimise);

            using (var run = new OptimiseRun(DirectoryPath))
            {
                handler.RunTask(ForEachTask.Create(
                    name: string.Format(Resources.FindingDuplicateFiles, DirectoryPath),
                    target: ListAll(),
                    work: run.Work));
                return run.SavedBytes;
            }
        }
        #endregion

        #region Verify
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public virtual void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            if (!Contains(manifestDigest)) throw new ImplementationNotFoundException(manifestDigest);

            string digest = manifestDigest.Best;
            if (digest == null) throw new NotSupportedException(Resources.NoKnownDigestMethod);
            string target = Path.Combine(DirectoryPath, digest);
            try
            {
                VerifyDirectory(target, manifestDigest, handler);

                // Reseal the directory in case the write protection got lost
                if (_useWriteProtection) EnableWriteProtection(target);
            }
            catch (DigestMismatchException ex)
            {
                if (ex.ExpectedDigest == null) throw;
                Log.Error(ex);
                if (handler.Ask(
                    question: string.Format(Resources.ImplementationDamaged + Environment.NewLine + Resources.ImplementationDamagedAskRemove, ex.ExpectedDigest),
                    defaultAnswer: false, alternateMessage: string.Format(Resources.ImplementationDamaged + Environment.NewLine + Resources.ImplementationDamagedBatchInformation, ex.ExpectedDigest)))
                    Remove(new ManifestDigest(ex.ExpectedDigest), handler);
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns <see cref="Kind"/> and <see cref="DirectoryPath"/>. Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Kind + ": " + DirectoryPath;
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
            if (obj == null) return false;
            if (obj == this) return true;
            return obj.GetType() == typeof(DirectoryStore) && Equals((DirectoryStore)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return DirectoryPath.GetHashCode();
        }
        #endregion
    }
}
