/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts an archive.
    /// </summary>
    public abstract class Extractor : TaskBase, IDisposable
    {
        /// <inheritdoc/>
        public override string Name { get { return Resources.ExtractingArchive; } }

        /// <inheritdoc/>
        protected override bool UnitsByte { get { return true; } }

        /// <summary>
        /// The sub-directory in the archive (with Unix-style slashes) to be extracted; <c>null</c> to extract entire archive.
        /// </summary>
        [Description("The sub-directory in the archive (with Unix-style slashes) to be extracted; null to extract entire archive.")]
        [CanBeNull]
        public string SubDir { get; set; }

        /// <summary>
        /// The path to the directory to extract into.
        /// </summary>
        [Description("The path to the directory to extract into.")]
        [NotNull]
        public string TargetDir { get; private set; }

        /// <summary>
        /// Sub-path to be appended to <see cref="TargetDir"/> without affecting location of flag files; <c>null</c> for none.
        /// </summary>
        [Description("Sub-path to be appended to TargetDir without affecting location of flag files.")]
        [CanBeNull]
        public string Destination { get; set; }

        /// <summary>
        /// <see cref="TargetDir"/> and <see cref="Destination"/> combined.
        /// </summary>
        [NotNull]
        protected string EffectiveTargetDir { get { return string.IsNullOrEmpty(Destination) ? TargetDir : Path.Combine(TargetDir, Destination); } }

        /// <summary>
        /// Indicates whether <see cref="TargetDir"/> is located on a filesystem with support for Unixoid features such as executable bits.
        /// </summary>
        private readonly bool _isUnixFS;

        /// <summary>
        /// Prepares to extract an archive contained in a stream.
        /// </summary>
        /// <param name="target">The path to the directory to extract into.</param>
        protected Extractor([NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            TargetDir = target;

            if (Directory.Exists(target)) Directory.CreateDirectory(target);
            _isUnixFS = FlagUtils.IsUnixFS(target);
        }

        #region Factory methods
        /// <summary>
        /// Verifies that a archives of a specific MIME type are supported.
        /// </summary>
        /// <param name="mimeType">The MIME type of archive format of the stream.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        /// <exception cref="NotSupportedException">The <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        public static void VerifySupport([NotNull] string mimeType)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            switch (mimeType)
            {
                case Archive.MimeTypeZip:
                case Archive.MimeTypeTar:
                case Archive.MimeTypeTarGzip:
                case Archive.MimeTypeTarBzip:
                case Archive.MimeTypeTarLzma:
                case Archive.MimeTypeRubyGem:
                    return;

                case Archive.MimeType7Z:
                case Archive.MimeTypeCab:
                case Archive.MimeTypeMsi:
                    if (!WindowsUtils.IsWindows) throw new NotSupportedException(Resources.ExtractionOnlyOnWindows);
                    return;

                default:
                    throw new NotSupportedException(string.Format(Resources.UnsupportedArchiveMimeType, mimeType));
            }
        }

        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting from an archive stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <param name="mimeType">The MIME type of archive format of the stream.</param>
        /// <exception cref="IOException">Failed to read the archive file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the archive file was denied.</exception>
        [NotNull]
        public static Extractor Create([NotNull] Stream stream, [NotNull] string target, [CanBeNull] string mimeType)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            Extractor extractor;
            switch (mimeType)
            {
                case Archive.MimeTypeZip:
                    extractor = new ZipExtractor(stream, target);
                    break;
                case Archive.MimeTypeTar:
                    extractor = new TarExtractor(stream, target);
                    break;
                case Archive.MimeTypeTarGzip:
                    extractor = new TarGzExtractor(stream, target);
                    break;
                case Archive.MimeTypeTarBzip:
                    extractor = new TarBz2Extractor(stream, target);
                    break;
                case Archive.MimeTypeTarLzma:
                    extractor = new TarLzmaExtractor(stream, target);
                    break;
                case Archive.MimeTypeTarXz:
                    extractor = new TarXzExtractor(stream, target);
                    break;
                case Archive.MimeTypeRubyGem:
                    extractor = new RubyGemExtractor(stream, target);
                    break;
                case Archive.MimeType7Z:
                    extractor = new SevenZipExtractor(stream, target);
                    break;
                case Archive.MimeTypeCab:
                    extractor = new CabExtractor(stream, target);
                    break;
                case Archive.MimeTypeMsi:
                    throw new NotSupportedException("MSIs can only be accessed as local files, not as streams!");
                default:
                    throw new NotSupportedException(string.Format(Resources.UnsupportedArchiveMimeType, mimeType));
            }

            return extractor;
        }

        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting from an archive file.
        /// </summary>
        /// <param name="path">The path of the archive file to be extracted.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <param name="mimeType">The MIME type of archive format of the stream. Leave <c>null</c> to guess based on file name.</param>
        /// <param name="startOffset"></param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        /// <exception cref="IOException">The archive is damaged.</exception>
        /// <exception cref="NotSupportedException">The <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        [NotNull]
        public static Extractor Create([NotNull] string path, [NotNull] string target, [CanBeNull] string mimeType = null, long startOffset = 0)
        {
            if (string.IsNullOrEmpty(mimeType)) mimeType = Archive.GuessMimeType(path);

            // MSI Extractor does not support Stream-based access
            if (mimeType == Archive.MimeTypeMsi) return new MsiExtractor(path, target);

            Stream stream = File.OpenRead(path);
            if (startOffset != 0) stream = new OffsetStream(stream, startOffset);

            try
            {
                return Create(stream, target, mimeType);
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }
        #endregion

        /// <summary>
        /// Returns the path of an archive entry relative to <see cref="SubDir"/>.
        /// </summary>
        /// <param name="entryName">The Unix-style path of the archive entry relative to the archive's root.</param>
        /// <returns>The relative path or <c>null</c> if the <paramref name="entryName"/> doesn't lie within the <see cref="SubDir"/>.</returns>
        [CanBeNull]
        protected virtual string GetRelativePath([NotNull] string entryName)
        {
            entryName = FileUtils.UnifySlashes(entryName);

            // Remove leading slashes
            entryName = entryName.TrimStart(Path.DirectorySeparatorChar);
            if (entryName.StartsWith("." + Path.DirectorySeparatorChar)) entryName = entryName.Substring(2);

            if (!string.IsNullOrEmpty(SubDir))
            {
                // Remove leading and trailing slashes
                string subDir = FileUtils.UnifySlashes(SubDir).Trim(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                // Only extract objects within the selected sub-directory
                entryName = entryName.StartsWith(subDir) ? entryName.Substring(subDir.Length) : null;
            }

            // Remove leading slashes left over after trimming away the SubDir
            if (entryName != null) entryName = entryName.TrimStart(Path.DirectorySeparatorChar);

            return entryName;
        }

        private readonly List<KeyValuePair<string, DateTime>> _directoryWriteTimes = new List<KeyValuePair<string, DateTime>>();

        /// <summary>
        /// Creates a directory in the filesystem and sets its last write time.
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="SubDir"/>.</param>
        /// <param name="lastWriteTime">The last write time to set.</param>
        protected void CreateDirectory([NotNull] string relativePath, DateTime lastWriteTime)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            string fullPath = CombinePath(relativePath);

            Directory.CreateDirectory(fullPath);
            _directoryWriteTimes.Add(new KeyValuePair<string, DateTime>(fullPath, lastWriteTime));
        }

        /// <summary>
        /// Combines the extraction <see cref="TargetDir"/> path with the relative path inside the archive (ensuring only valid paths are returned).
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="SubDir"/>.</param>
        /// <returns>The combined path as an absolute path.</returns>
        /// <exception cref="IOException"><paramref name="relativePath"/> is invalid (e.g. is absolute, points outside the archive's root, contains invalid characters).</exception>
        protected string CombinePath([NotNull] string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            if (FileUtils.IsBreakoutPath(relativePath)) throw new IOException(string.Format(Resources.ArchiveInvalidPath, relativePath));

            try
            {
                return Path.GetFullPath(Path.Combine(EffectiveTargetDir, relativePath));
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                throw new IOException(Resources.ArchiveInvalidPath, ex);
            }
            #endregion
        }

        /// <summary>
        /// Writes a file to the filesystem and sets its last write time.
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="SubDir"/>.</param>
        /// <param name="fileSize">The length of the zip entries uncompressed data, needed because stream's Length property is always 0.</param>
        /// <param name="lastWriteTime">The last write time to set.</param>
        /// <param name="stream">The stream containing the file data to be written.</param>
        /// <param name="executable"><c>true</c> if the file's executable bit is set; <c>false</c> otherwise.</param>
        protected void WriteFile([NotNull] string relativePath, long fileSize, DateTime lastWriteTime, [NotNull] Stream stream, bool executable = false)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var fileStream = OpenFileWriteStream(relativePath, executable: executable))
                if (fileSize != 0) StreamToFile(stream, fileStream);

            File.SetLastWriteTimeUtc(CombinePath(relativePath), DateTime.SpecifyKind(lastWriteTime, DateTimeKind.Utc));
        }

        /// <summary>
        /// Creates a stream for writing an extracted file to the filesystem.
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="SubDir"/>.</param>
        /// <param name="executable"><c>true</c> if the file's executable bit is set; <c>false</c> otherwise.</param>
        /// <returns>A stream for writing the extracted file.</returns>
        protected FileStream OpenFileWriteStream([NotNull] string relativePath, bool executable = false)
        {
            CancellationToken.ThrowIfCancellationRequested();

            string fullPath = CombinePath(relativePath);
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            bool alreadyExists = File.Exists(fullPath);
            var stream = File.Create(fullPath);

            // If a symlink is overwritten by a normal file, remove the symlink flag
            if (alreadyExists)
            {
                string flagRelativePath = string.IsNullOrEmpty(Destination) ? relativePath : Path.Combine(Destination, relativePath);
                FlagUtils.Remove(Path.Combine(TargetDir, FlagUtils.SymlinkFile), flagRelativePath);
            }

            if (executable) SetExecutableBit(relativePath);
            else if (alreadyExists) RemoveExecutableBit(relativePath); // If an executable file is overwritten by a non-executable file, remove the xbit flag

            return stream;
        }

        /// <summary>
        /// Marks a file as executable using the filesystem if possible; stores it in a <see cref="FlagUtils.XbitFile"/> otherwise.
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="SubDir"/>.</param>
        private void SetExecutableBit(string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            if (_isUnixFS) FileUtils.SetExecutable(Path.Combine(EffectiveTargetDir, relativePath), true);
            else
            {
                // Non-Unixoid OSes (e.g. Windows) can't store the executable flag directly in the filesystem; remember in a text-file instead
                string flagRelativePath = string.IsNullOrEmpty(Destination) ? relativePath : Path.Combine(Destination, relativePath);
                FlagUtils.Set(Path.Combine(TargetDir, FlagUtils.XbitFile), flagRelativePath);
            }
        }

        /// <summary>
        /// Marks a file as no longer executable using the filesystem if possible, an <see cref="FlagUtils.XbitFile"/> file otherwise.
        /// </summary>
        /// <param name="relativePath">A path relative to <see cref="SubDir"/>.</param>
        private void RemoveExecutableBit(string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            if (_isUnixFS) FileUtils.SetExecutable(Path.Combine(EffectiveTargetDir, relativePath), false);
            else
            {
                // Non-Unixoid OSes (e.g. Windows) can't store the executable flag directly in the filesystem; remember in a text-file instead
                string flagRelativePath = string.IsNullOrEmpty(Destination) ? relativePath : Path.Combine(Destination, relativePath);
                FlagUtils.Remove(Path.Combine(TargetDir, FlagUtils.XbitFile), flagRelativePath);
            }
        }

        private struct PendingHardlink
        {
            public string Source, Target;
            public bool Executable;
        }

        private readonly List<PendingHardlink> _pendingHardlinks = new List<PendingHardlink>();

        /// <summary>
        /// Queues a hardlink for creation at the end of the extracton process. This enables handling links to files that have not been extracted yet.
        /// </summary>
        /// <param name="source">A path relative to <see cref="SubDir"/>.</param>
        /// <param name="target">A path relative to <see cref="SubDir"/>.</param>
        /// <param name="executable"><c>true</c> if the hardlink's executable bit is set; <c>false</c> otherwise.</param>
        protected void QueueHardlink([NotNull] string source, [NotNull] string target, bool executable = false)
        {
            _pendingHardlinks.Add(new PendingHardlink {Source = source, Target = target, Executable = executable});
        }

        /// <summary>
        /// Creates a symbolic link in the filesystem if possible; stores it in a <see cref="FlagUtils.SymlinkFile"/> otherwise.
        /// </summary>
        /// <param name="source">A path relative to <see cref="SubDir"/>.</param>
        /// <param name="target">The target the symbolic link shall point to relative to <paramref name="source"/>. May use non-native path separators!</param>
        protected void CreateSymlink([NotNull] string source, [NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            string sourceAbsolute = CombinePath(source);
            string sourceDirectory = Path.GetDirectoryName(sourceAbsolute);
            if (sourceDirectory != null && !Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);

            if (_isUnixFS) FileUtils.CreateSymlink(sourceAbsolute, target);
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
                string flagRelativePath = string.IsNullOrEmpty(Destination) ? source : Path.Combine(Destination, source);
                FlagUtils.Set(Path.Combine(TargetDir, FlagUtils.SymlinkFile), flagRelativePath);
            }
        }

        /// <summary>
        /// Helper method for <see cref="WriteFile"/>.
        /// </summary>
        /// <param name="stream">The stream to write to a file.</param>
        /// <param name="fileStream">Stream access to the file to write.</param>
        /// <remarks>Can be overwritten for archive formats that don't simply write a <see cref="Stream"/> to a file.</remarks>
        protected virtual void StreamToFile([NotNull] Stream stream, [NotNull] FileStream fileStream)
        {
            stream.CopyTo(fileStream, cancellationToken: CancellationToken);
        }

        /// <summary>
        /// Performs any tasks that were deferred to the end of the extraction process.
        /// </summary>
        protected void Finish()
        {
            foreach (var hardlink in _pendingHardlinks)
            {
                CreateHardlink(hardlink.Source, hardlink.Target);
                if (hardlink.Executable) SetExecutableBit(hardlink.Source);
            }

            SetDirectoryWriteTimes();
        }

        /// <summary>
        /// Creates a hardlink in the filesystem if possible; creates a copy otherwise.
        /// </summary>
        /// <param name="source">A path relative to <see cref="SubDir"/>.</param>
        /// <param name="target">A path relative to <see cref="SubDir"/>.</param>
        private void CreateHardlink([NotNull] string source, [NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            string sourceAbsolute = CombinePath(source);
            string sourceDirectory = Path.GetDirectoryName(sourceAbsolute);
            if (sourceDirectory != null && !Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);
            string targetAbsolute = CombinePath(target);

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

        /// <summary>
        /// Sets the last write times of the directories that were recorded during extraction.
        /// </summary>
        /// <remarks>This must be done in a separate step, since changing anything within a directory will affect its last write time.</remarks>
        private void SetDirectoryWriteTimes()
        {
            // Run through list backwards to ensure directories are handled "from the inside out"
            for (int index = _directoryWriteTimes.Count - 1; index >= 0; index--)
            {
                var pair = _directoryWriteTimes[index];
                Directory.SetLastWriteTimeUtc(pair.Key, pair.Value);
            }
        }

        #region Dispose
        /// <summary>
        /// Disposes the underlying <see cref="Stream"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }
}
