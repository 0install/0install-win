/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Common base class for tasks that walk an entire directory tree using template methods.
    /// </summary>
    public abstract class DirectoryWalkTask : TaskBase
    {
        /// <inheritdoc/>
        protected override bool UnitsByte { get { return true; } }

        /// <summary>Indicates whether <see cref="SourceDirectory"/> is located on a filesystem with support for Unixoid features such as executable bits.</summary>
        private readonly bool _isUnixFS;

        /// <summary>
        /// The directory to walk.
        /// </summary>
        [NotNull]
        public DirectoryInfo SourceDirectory { get; private set; }

        /// <summary>
        /// Creates a new directory walking task.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to walk.</param>
        protected DirectoryWalkTask([NotNull] string sourceDirectory)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourceDirectory)) throw new ArgumentNullException("sourceDirectory");
            #endregion

            SourceDirectory = new DirectoryInfo(Path.GetFullPath(sourceDirectory));
            _isUnixFS = FlagUtils.IsUnixFS(sourceDirectory);
        }

        /// <inheritdoc/>
        protected override void Execute()
        {
            State = TaskState.Header;
            var entries = GetSortedDirectoryEntries(SourceDirectory.FullName);
            UnitsTotal = entries.OfType<FileInfo>().Sum(file => file.Length);

            State = TaskState.Data;
            HandleEntries(entries);

            State = TaskState.Complete;
        }

        /// <summary>
        /// Creates a recursive list of all filesystem entries in a certain directory sorted in C order.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <returns>An array of filesystem entries.</returns>
        /// <exception cref="IOException">The directory could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the directory is not permitted.</exception>
        private static FileSystemInfo[] GetSortedDirectoryEntries([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Get separated lists for files and directories
            var files = Directory.GetFiles(path);
            var directories = Directory.GetDirectories(path);

            // C-sort the lists
            Array.Sort(files, StringComparer.Ordinal);
            Array.Sort(directories, StringComparer.Ordinal);

            // Create the combined result list (files first, then sub-diretories)
            var result = new List<FileSystemInfo>(files.Select(file => new FileInfo(file)).Cast<FileSystemInfo>());
            foreach (string directory in directories)
            {
                result.Add(new DirectoryInfo(directory));

                // Recurse into sub-direcories (but do not follow symlinks)
                if (!FileUtils.IsSymlink(directory)) result.AddRange(GetSortedDirectoryEntries(directory));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Iterates over all <paramref name="entries"/> and calls handler methods for them.
        /// </summary>
        /// <exception cref="NotSupportedException">A file has illegal properties (e.g. is a device file, has line breaks in the filename, etc.).</exception>
        /// <exception cref="IOException">There was an error reading a file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to read a file.</exception>
        private void HandleEntries([NotNull] IEnumerable<FileSystemInfo> entries)
        {
            var externalXbits = FlagUtils.GetFiles(FlagUtils.XbitFile, SourceDirectory.FullName);
            var externalSymlinks = FlagUtils.GetFiles(FlagUtils.SymlinkFile, SourceDirectory.FullName);

            foreach (var entry in entries)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var file = entry as FileInfo;
                if (file != null)
                {
                    if (file.Name == Manifest.ManifestFile || file.Name == FlagUtils.XbitFile || file.Name == FlagUtils.SymlinkFile) continue;

                    HandleEntry(file, externalXbits, externalSymlinks);
                    UnitsProcessed += file.Length;
                }
                else
                {
                    var directory = entry as DirectoryInfo;
                    if (directory != null) HandleEntry(directory);
                }
            }
        }

        /// <summary>
        /// Handles a file system entry the OS reports as a file.
        /// </summary>
        /// <param name="entry">The file entry to handle.</param>
        /// <param name="externalXbits">A list of fully qualified paths of files that are named in the <see cref="FlagUtils.SymlinkFile"/>.</param>
        /// <param name="externalSymlinks">A list of fully qualified paths of files that are named in the <see cref="FlagUtils.SymlinkFile"/>.</param>
        /// <exception cref="NotSupportedException">The <paramref name="entry"/> has illegal properties (e.g. is a device file, has line breaks in the filename, etc.).</exception>
        /// <exception cref="IOException">There was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to read the file.</exception>
        private void HandleEntry([NotNull] FileInfo entry, [NotNull] ICollection<string> externalXbits, [NotNull] ICollection<string> externalSymlinks)
        {
            if (_isUnixFS)
            {
                string symlinkTarget;
                if (FileUtils.IsSymlink(entry.FullName, out symlinkTarget))
                    HandleSymlink(entry, Encoding.UTF8.GetBytes(symlinkTarget));
                else if (FileUtils.IsExecutable(entry.FullName))
                    HandleFile(entry, executable: true);
                else if (!FileUtils.IsRegularFile(entry.FullName))
                    throw new NotSupportedException(string.Format(Resources.IllegalFileType, entry.FullName));
                else HandleFile(entry);
            }
            else
            {
                string symlinkTarget;
                if (CygwinUtils.IsSymlink(entry.FullName, out symlinkTarget))
                    HandleSymlink(entry, Encoding.UTF8.GetBytes(symlinkTarget));
                else if (externalSymlinks.Contains(entry.FullName))
                    HandleSymlink(entry, File.ReadAllBytes(entry.FullName));
                else if (externalXbits.Contains(entry.FullName))
                    HandleFile(entry, executable: true);
                else HandleFile(entry);
            }
        }

        /// <summary>
        /// Handles a file system entry the OS reports as a directory.
        /// </summary>
        /// <param name="entry">The directory entry to handles.</param>
        /// <exception cref="IOException">There was an error reading the directory.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to read the directory.</exception>
        private void HandleEntry([NotNull] DirectoryInfo entry)
        {
            string symlinkTarget;
            if (_isUnixFS && FileUtils.IsSymlink(entry.FullName, out symlinkTarget))
                HandleSymlink(entry, Encoding.UTF8.GetBytes(symlinkTarget));
            else
                HandleDirectory(entry);
        }

        /// <summary>
        /// Handles a file.
        /// </summary>
        /// <param name="file">The file to handle.</param>
        /// <param name="executable"><c>true</c> indicates that the file is marked as executable.</param>
        protected abstract void HandleFile([NotNull] FileInfo file, bool executable = false);

        /// <summary>
        /// Handles a symlink.
        /// </summary>
        /// <param name="symlink">The symlink to handle.</param>
        /// <param name="data">The encoded target of the symlink.</param>
        protected abstract void HandleSymlink([NotNull] FileSystemInfo symlink, [NotNull] byte[] data);

        /// <summary>
        /// Handles a directory.
        /// </summary>
        /// <param name="directory">The directory to handle.</param>
        protected abstract void HandleDirectory([NotNull] DirectoryInfo directory);
    }
}
