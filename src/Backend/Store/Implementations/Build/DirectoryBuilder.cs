/*
 * Copyright 2010-2017 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Provides methods for building a directory with support for flag files.
    /// </summary>
    public class DirectoryBuilder
    {
        /// <summary>
        /// The path to the directory to build.
        /// </summary>
        [NotNull]
        public string TargetPath { get; }

        [CanBeNull]
        private string _targetSuffix;

        /// <summary>
        /// Sub-path to be appended to <see cref="TargetPath"/> without affecting location of flag files; <c>null</c> for none.
        /// </summary>
        [CanBeNull]
        public string TargetSuffix
        {
            get => _targetSuffix;
            set
            {
                _targetSuffix = value;
                _effectiveTargetPath = null;
            }
        }

        [CanBeNull]
        private string _effectiveTargetPath;

        /// <summary>
        /// <see cref="TargetPath"/> and <see cref="TargetSuffix"/> combined.
        /// </summary>
        [NotNull]
        public string EffectiveTargetPath
            => _effectiveTargetPath ??
               (_effectiveTargetPath = string.IsNullOrEmpty(TargetSuffix) ? TargetPath : Path.Combine(TargetPath, TargetSuffix));

        /// <summary>
        /// Indicates whether <see cref="TargetPath"/> is located on a filesystem with support for Unixoid features such as executable bits.
        /// </summary>
        private readonly bool _targetIsUnixFS;

        /// <summary>Used to track exeuctable bits in <see cref="TargetPath"/> if <see cref="_targetIsUnixFS"/> is <c>false</c>.</summary>
        private readonly string _targetXbitFile;

        /// <summary>Used to track symlinks if in <see cref="TargetPath"/> <see cref="_targetIsUnixFS"/> is <c>false</c>.</summary>
        private readonly string _targetSymlinkFile;

        /// <summary>
        /// Creates a new directory builder.
        /// </summary>
        /// <param name="targetPath">The path to the directory to build.</param>
        public DirectoryBuilder([NotNull] string targetPath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(targetPath)) throw new ArgumentNullException(nameof(targetPath));
            #endregion

            TargetPath = targetPath;

            _targetIsUnixFS = FlagUtils.IsUnixFS(targetPath);
            _targetXbitFile = Path.Combine(targetPath, FlagUtils.XbitFile);
            _targetSymlinkFile = Path.Combine(targetPath, FlagUtils.SymlinkFile);
        }

        /// <summary>
        /// Performs preparartion tasks, such as creating the empty directory if it does not exist yet.
        /// </summary>
        public void Initialize()
        {
            if (!Directory.Exists(EffectiveTargetPath)) Directory.CreateDirectory(EffectiveTargetPath);
        }

        /// <summary>Maps paths relative to <see cref="EffectiveTargetPath"/> to timestamps for directory write times. Preserves the order.</summary>
        private readonly List<KeyValuePair<string, DateTime>> _pendingDirectoryWriteTimes = new List<KeyValuePair<string, DateTime>>();

        /// <summary>Maps paths relative to <see cref="EffectiveTargetPath"/> to timestamps for file write times.</summary>
        private readonly Dictionary<string, DateTime> _pendingFileWriteTimes = new Dictionary<string, DateTime>();

        /// <summary>Lists paths relative to <see cref="EffectiveTargetPath"/> for files to be marked as executable.</summary>
        private readonly HashSet<string> _pendingExecutableFiles = new HashSet<string>();

        /// <summary>Maps from and to paths relative to <see cref="EffectiveTargetPath"/>. The key is a new hardlink to be created. The value is the existing file to point to.</summary>
        private readonly Dictionary<string, string> _pendingHardlinks = new Dictionary<string, string>();

        /// <summary>
        /// Creates a subdirectory.
        /// </summary>
        /// <param name="relativePath">The path of the directory to create (relative to <see cref="EffectiveTargetPath"/>).</param>
        /// <param name="lastWriteTime">The last write time to set for the directory.</param>
        public void CreateDirectory([NotNull] string relativePath, DateTime lastWriteTime)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException(nameof(relativePath));
            #endregion

            Directory.CreateDirectory(GetFullPath(relativePath));
            _pendingDirectoryWriteTimes.Add(new KeyValuePair<string, DateTime>(relativePath, lastWriteTime));
        }

        /// <summary>
        /// Prepares a new file path in the directory without creating the file itself yet.
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="EffectiveTargetPath"/>.</param>
        /// <param name="lastWriteTime">The last write time to set for the file later. This value is optional.</param>
        /// <param name="executable"><c>true</c> if the file's executable bit is to be set later; <c>false</c> otherwise.</param>
        /// <returns>An absolute file path.</returns>
        public string NewFilePath(string relativePath, DateTime? lastWriteTime, bool executable = false)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException(nameof(relativePath));
            #endregion

            // Delete any preexisting file to reset xbits, etc.
            DeleteFile(relativePath);

            string fullPath = GetFullPath(relativePath);
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            if (lastWriteTime.HasValue) _pendingFileWriteTimes[relativePath] = lastWriteTime.Value;
            if (executable) _pendingExecutableFiles.Add(relativePath);
            return fullPath;
        }

        /// <summary>
        /// Creates a symbolic link in the filesystem if possible; stores it in a <see cref="FlagUtils.SymlinkFile"/> otherwise.
        /// </summary>
        /// <param name="source">A path relative to <see cref="EffectiveTargetPath"/>.</param>
        /// <param name="target">The target the symbolic link shall point to relative to <paramref name="source"/>. May use non-native path separators!</param>
        public void CreateSymlink([NotNull] string source, [NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            #endregion

            // Delete any preexisting file to reset xbits, etc.
            DeleteFile(source);

            string sourceAbsolute = GetFullPath(source);
            string sourceDirectory = Path.GetDirectoryName(sourceAbsolute);
            if (!Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);

            if (_targetIsUnixFS) FileUtils.CreateSymlink(sourceAbsolute, target);
            else if (WindowsUtils.IsWindowsNT)
            {
                // NOTE: NTFS symbolic links require admin privileges; use Cygwin symlinks instead
                CygwinUtils.CreateSymlink(sourceAbsolute, target);
            }
            else
            {
                // Write link data as a normal file
                File.WriteAllText(sourceAbsolute, target);

                // Some OSes can't store the symlink flag directly in the filesystem; remember in a text-file instead
                FlagUtils.Set(_targetSymlinkFile, GetFlagRelativePath(source));
            }
        }

        /// <summary>
        /// Queues a hardlink for creation at the end of the creation process.
        /// </summary>
        /// <param name="source">The path of the hardlink to create (relative to <see cref="EffectiveTargetPath"/>).</param>
        /// <param name="target">The path of the target the hardlink shall point to (relative to <see cref="EffectiveTargetPath"/>).</param>
        /// <param name="executable"><c>true</c> if the hardlink's executable bit is set; <c>false</c> otherwise.</param>
        public void QueueHardlink([NotNull] string source, [NotNull] string target, bool executable = false)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            #endregion

            _pendingHardlinks.Add(source, target);
            if (executable) _pendingExecutableFiles.Add(source);
        }

        /// <summary>
        /// Performs any tasks that were deferred to the end of the creation process.
        /// </summary>
        public void CompletePending()
        {
            CreatePendingHardlinks();
            SetPendingExecutableBits();
            SetPendingLastWriteTimes();
        }

        /// <summary>
        /// Creates all pending hardlinks if possible; creates copies otherwise.
        /// This must be done in a separate step, to allow links to be requested before the files they point to have been created.
        /// </summary>
        private void CreatePendingHardlinks()
        {
            foreach (var pair in _pendingHardlinks.ToList()) // NOTE: Must clone list because it may be modified during enumeration
            {
                string sourceAbsolute = GetFullPath(pair.Key);
                string sourceDirectory = Path.GetDirectoryName(sourceAbsolute);
                if (sourceDirectory != null && !Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);
                string targetAbsolute = GetFullPath(pair.Value);

                // Delete any preexisting file to reset xbits, etc.
                DeleteFile(pair.Key);

                try
                {
                    FileUtils.CreateHardlink(sourceAbsolute, targetAbsolute);
                }
                catch (PlatformNotSupportedException)
                {
                    File.Copy(targetAbsolute, sourceAbsolute);
                }
                catch (UnauthorizedAccessException)
                {
                    File.Copy(targetAbsolute, sourceAbsolute);
                }
            }
            _pendingHardlinks.Clear();
        }

        /// <summary>
        /// Marks files as executable using the filesystem if possible; stores them in a <see cref="FlagUtils.XbitFile"/> otherwise.
        /// </summary>
        private void SetPendingExecutableBits()
        {
            foreach (string relativePath in _pendingExecutableFiles)
            {
                if (_targetIsUnixFS) FileUtils.SetExecutable(GetFullPath(relativePath), true);
                else
                {
                    // Non-Unixoid OSes (e.g. Windows) can't store the executable flag directly in the filesystem; remember in a text-file instead
                    FlagUtils.Set(_targetXbitFile, GetFlagRelativePath(relativePath));
                }
            }
            _pendingExecutableFiles.Clear();
        }

        /// <summary>
        /// Sets the recorded last write times of files amd directories.
        /// This must be done in a separate step, since changing anything within a directory will affect its last write time.
        /// </summary>
        private void SetPendingLastWriteTimes()
        {
            foreach (var pair in _pendingFileWriteTimes)
                File.SetLastWriteTimeUtc(GetFullPath(pair.Key), DateTime.SpecifyKind(pair.Value, DateTimeKind.Utc));
            _pendingFileWriteTimes.Clear();

            // Run through list backwards to ensure directories are handled "from the inside out"
            for (int index = _pendingDirectoryWriteTimes.Count - 1; index >= 0; index--)
            {
                var pair = _pendingDirectoryWriteTimes[index];
                Directory.SetLastWriteTimeUtc(GetFullPath(pair.Key), DateTime.SpecifyKind(pair.Value, DateTimeKind.Utc));
            }
            _pendingDirectoryWriteTimes.Clear();
        }

        /// <summary>
        /// Deletes a file if it exists and removes any pending steps registered for it.
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="EffectiveTargetPath"/>.</param>
        private void DeleteFile([NotNull] string relativePath)
        {
            _pendingFileWriteTimes.Remove(relativePath);
            _pendingExecutableFiles.Remove(relativePath);
            _pendingHardlinks.Remove(relativePath);

            string flagRelativePath = GetFlagRelativePath(relativePath);
            FlagUtils.Remove(_targetSymlinkFile, flagRelativePath);
            FlagUtils.Remove(_targetXbitFile, flagRelativePath);

            string fullPath = GetFullPath(relativePath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        /// <summary>
        /// Resolves a path relative to <see cref="EffectiveTargetPath"/> to a full path.
        /// </summary>
        /// <exception cref="IOException"><paramref name="relativePath"/> is invalid (e.g. is absolute, points outside the archive's root, contains invalid characters).</exception>
        private string GetFullPath([NotNull] string relativePath)
        {
            if (FileUtils.IsBreakoutPath(relativePath)) throw new IOException(string.Format(Resources.ArchiveInvalidPath, relativePath));

            try
            {
                return Path.GetFullPath(Path.Combine(EffectiveTargetPath, relativePath));
            }
            #region Error handling
            catch (ArgumentException ex)
            {
                throw new IOException(Resources.ArchiveInvalidPath, ex);
            }
            #endregion
        }

        /// <summary>
        /// Resolves a path relative to <see cref="EffectiveTargetPath"/> to a path relative to the location of flag files.
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="EffectiveTargetPath"/>.</param>
        /// <exception cref="IOException"><paramref name="relativePath"/> is invalid (e.g. is absolute, points outside the archive's root, contains invalid characters).</exception>
        private string GetFlagRelativePath([NotNull] string relativePath)
        {
            if (FileUtils.IsBreakoutPath(relativePath)) throw new IOException(string.Format(Resources.ArchiveInvalidPath, relativePath));

            return string.IsNullOrEmpty(TargetSuffix)
                ? relativePath
                : Path.Combine(TargetSuffix, relativePath);
        }
    }
}