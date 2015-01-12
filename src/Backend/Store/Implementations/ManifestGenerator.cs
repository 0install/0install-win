/*
 * Copyright 2010-2014 Bastian Eicher, Roland Leopold Walkling
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
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Generates a <see cref="Manifest"/> for a directory in the filesystem as a background task.
    /// </summary>
    public class ManifestGenerator : TaskBase
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return string.Format(Resources.GeneratingManifest, Format); } }

        /// <inheritdoc/>
        protected override bool UnitsByte { get { return true; } }

        /// <summary>
        /// The path of the directory to analyze. No trailing <see cref="Path.DirectorySeparatorChar"/>!
        /// </summary>
        [NotNull]
        public string TargetDir { get; private set; }

        /// <summary>
        /// The format of the manifest to generate.
        /// </summary>
        [NotNull]
        public ManifestFormat Format { get; private set; }

        /// <summary>
        /// If <see cref="TaskBase.State"/> is <see cref="TaskState.Complete"/> this property contains the generated <see cref="Manifest"/>; otherwise it's <see langword="null"/>.
        /// </summary>
        public Manifest Result { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to generate a manifest for a directory in the filesystem.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest to generate.</param>
        public ManifestGenerator([NotNull] string path, [NotNull] ManifestFormat format)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            TargetDir = path.TrimEnd(Path.DirectorySeparatorChar);
            Format = format;
        }
        #endregion

        /// <inheritdoc/>
        protected override void Execute()
        {
            State = TaskState.Header;
            var entries = Format.GetSortedDirectoryEntries(TargetDir);
            UnitsTotal = entries.OfType<FileInfo>().Sum(file => file.Length);

            State = TaskState.Data;
            Result = new Manifest(Format, GetNodes(entries));

            State = TaskState.Complete;
        }

        /// <summary>
        /// Creates manifest nodes for a set of file system elements.
        /// </summary>
        /// <param name="entries">The file system elements to create nodes for.</param>
        /// <returns>The nodes for the elements.</returns>
        private IEnumerable<ManifestNode> GetNodes(IEnumerable<FileSystemInfo> entries)
        {
            var externalXbits = FlagUtils.GetFiles(FlagUtils.XbitFile, TargetDir);
            var externalSymlinks = FlagUtils.GetFiles(FlagUtils.SymlinkFile, TargetDir);

            // Iterate through the directory listing to build a list of manifets entries
            var nodes = new List<ManifestNode>();
            foreach (var entry in entries)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var file = entry as FileInfo;
                if (file != null)
                {
                    // Don't include manifest management files in manifest
                    if (file.Name == Manifest.ManifestFile || file.Name == FlagUtils.XbitFile || file.Name == FlagUtils.SymlinkFile) continue;

                    nodes.Add(GetFileNode(file, Format, externalXbits, externalSymlinks));
                    UnitsProcessed += file.Length;
                }
                else
                {
                    var directory = entry as DirectoryInfo;
                    if (directory != null) nodes.Add(GetDirectoryNode(directory, Format, Path.GetFullPath(TargetDir)));
                }
            }
            return nodes;
        }

        /// <summary>
        /// Creates a manifest node for a file.
        /// </summary>
        /// <param name="file">The file object to create a node for.</param>
        /// <param name="format">The manifest format containing digest rules.</param>
        /// <param name="externalXbits">A list of fully qualified paths of files that are named in the <see cref="FlagUtils.SymlinkFile"/>.</param>
        /// <param name="externalSymlinks">A list of fully qualified paths of files that are named in the <see cref="FlagUtils.SymlinkFile"/>.</param>
        /// <returns>The node for the list.</returns>
        /// <exception cref="NotSupportedException">The <paramref name="file"/> has illegal properties (e.g. is a device file, has line breaks in the filename, etc.).</exception>
        /// <exception cref="IOException">There was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to read the file.</exception>
        private static ManifestNode GetFileNode(FileInfo file, ManifestFormat format, ICollection<string> externalXbits, ICollection<string> externalSymlinks)
        {
            // Real symlinks
            string symlinkContents;
            if (FileUtils.IsSymlink(file.FullName, out symlinkContents))
            {
                var symlinkData = Encoding.UTF8.GetBytes(symlinkContents);
                return new ManifestSymlink(format.DigestContent(symlinkData), symlinkData.Length, file.Name);
            }

            // Invalid file type
            if (!FileUtils.IsRegularFile(file.FullName))
                throw new NotSupportedException(string.Format(Resources.IllegalFileType, file.FullName));

            using (var stream = File.OpenRead(file.FullName))
            {
                // External symlinks
                if (externalSymlinks.Contains(file.FullName))
                    return new ManifestSymlink(format.DigestContent(stream), file.Length, file.Name);

                // Executable file
                if (externalXbits.Contains(file.FullName) || FileUtils.IsExecutable(file.FullName))
                    return new ManifestExecutableFile(format.DigestContent(stream), file.LastWriteTimeUtc.ToUnixTime(), file.Length, file.Name);

                // Normal file
                return new ManifestNormalFile(format.DigestContent(stream), file.LastWriteTimeUtc.ToUnixTime(), file.Length, file.Name);
            }
        }

        /// <summary>
        /// Creates a manifest node for a directory.
        /// </summary>
        /// <param name="directory">The directory object to create a node for.</param>
        /// <param name="format">The manifest format containing digest rules.</param>
        /// <param name="rootPath">The fully qualified path of the root directory the manifest is being generated for.</param>
        /// <returns>The node for the list.</returns>
        /// <exception cref="IOException">There was an error reading the directory.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to read the directory.</exception>
        private static ManifestNode GetDirectoryNode(DirectoryInfo directory, ManifestFormat format, string rootPath)
        {
            // Directory symlinks
            string symlinkContents;
            if (FileUtils.IsSymlink(directory.FullName, out symlinkContents))
            {
                var symlinkData = Encoding.UTF8.GetBytes(symlinkContents);
                return new ManifestSymlink(format.DigestContent(symlinkData), symlinkData.Length, directory.Name);
            }

            // Remove leading portion of path and use Unix slashes
            string trimmedName = directory.FullName.Substring(rootPath.Length).Replace(Path.DirectorySeparatorChar, '/');
            return new ManifestDirectory(directory.LastWriteTime.ToUnixTime(), trimmedName);
        }
    }
}
