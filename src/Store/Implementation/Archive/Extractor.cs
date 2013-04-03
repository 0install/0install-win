/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Net;
using Common.Streams;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation.Archive
{
    /// <summary>
    /// Provides methods for extracting an archive (optionally as a background task).
    /// </summary>
    public abstract class Extractor : ThreadTask, IDisposable
    {
        #region Variables
        /// <summary>
        /// Stores the last write times for directories so they can be set by <see cref="SetDirectoryWriteTimes"/>.
        /// </summary>
        private readonly List<KeyValuePair<string, DateTime>> _directoryWriteTimes = new List<KeyValuePair<string, DateTime>>();
        #endregion

        #region Properties
        private string _name;

        /// <inheritdoc />
        public override string Name { get { return string.Format(Resources.ExtractingArchive, Path.GetFileName(_name)); } }

        /// <inheritdoc />
        public override bool UnitsByte { get { return true; } }

        /// <summary>
        /// The backing stream to extract the data from.
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// The sub-directory in the archive (with Unix-style slashes) to be extracted; <see langword="null"/> to extract entire archive.
        /// </summary>
        [Description("The sub-directory in the archive (with Unix-style slashes) to be extracted; null to extract entire archive.")]
        public string SubDir { get; set; }

        /// <summary>
        /// The path to the directory to extract into.
        /// </summary>
        [Description("The path to the directory to extract into.")]
        public string TargetDir { get; protected set; }

        /// <summary>
        /// Sub-path to be appended to <see cref="TargetDir"/> without affecting location of flag files; <see langword="null"/> for none.
        /// </summary>
        [Description("Sub-path to be appended to TargetDir without affecting location of flag files.")]
        public string Destination { get; set; }

        /// <summary>
        /// <see cref="TargetDir"/> and <see cref="Destination"/> combined.
        /// </summary>
        protected string EffectiveTargetDir { get { return string.IsNullOrEmpty(Destination) ? TargetDir : Path.Combine(TargetDir, Destination); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to extract an archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        protected Extractor(Stream stream, string target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            Stream = stream;
            TargetDir = target;

            UnitsTotal = stream.Length;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Verifies that a archives of a specific MIME type are supported.
        /// </summary>
        /// <param name="mimeType">The MIME type of archive format of the stream.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        public static void VerifySupport(string mimeType)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            switch (mimeType)
            {
                case "application/zip":
                case "application/x-tar":
                case "application/x-compressed-tar":
                case "application/x-bzip-compressed-tar":
                case "application/x-lzma-compressed-tar":
                case "application/x-ruby-gem":
                    return;

                default:
                    throw new NotSupportedException(string.Format(Resources.UnknownMimeType, mimeType));
            }
        }

        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting a sub-directory from an archive stream.
        /// </summary>
        /// <param name="mimeType">The MIME type of archive format of the stream.</param>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        public static Extractor CreateExtractor(string mimeType, Stream stream, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            // Select the correct extractor based on the MIME type
            Extractor extractor;
            switch (mimeType)
            {
                case "application/zip":
                    extractor = new ZipExtractor(stream, target);
                    break;
                case "application/x-tar":
                    extractor = new TarExtractor(stream, target);
                    break;
                case "application/x-compressed-tar":
                    extractor = new TarGzExtractor(stream, target);
                    break;
                case "application/x-bzip-compressed-tar":
                    extractor = new TarBz2Extractor(stream, target);
                    break;
                case "application/x-lzma-compressed-tar":
                    extractor = new TarLzmaExtractor(stream, target);
                    break;
                case "application/x-ruby-gem":
                    extractor = new RubyGemExtractor(stream, target);
                    break;

                default:
                    throw new NotSupportedException(string.Format(Resources.UnknownMimeType, mimeType));
            }

            return extractor;
        }

        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting a sub-directory from an archive file.
        /// </summary>
        /// <param name="mimeType">The MIME type of archive format of the file; <see langword="null"/> to guess.</param>
        /// <param name="path">The file to be extracted.</param>
        /// <param name="startOffset">The number of bytes at the beginning of the file which should be ignored.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        /// <exception cref="IOException">Thrown if the archive is damaged or if the file couldn't be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        public static Extractor CreateExtractor(string mimeType, string path, long startOffset, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            // Try to guess missing MIME type
            if (string.IsNullOrEmpty(mimeType)) mimeType = ArchiveUtils.GuessMimeType(path);

            var stream = File.OpenRead(path);
            try
            {
                var extractor = CreateExtractor(mimeType, new OffsetStream(stream, startOffset), target);
                extractor._name = Path.GetFileName(path);
                return extractor;
            }
                #region Error handling
            catch
            {
                stream.Close();
                throw;
            }
            #endregion
        }
        #endregion

        //--------------------//

        #region Control
        /// <inheritdoc/>
        public override void RunSync(CancellationToken cancellationToken)
        {
            if (cancellationToken != null) cancellationToken.CancellationRequested += Cancel;
            Start();
            Join();
            if (cancellationToken != null) cancellationToken.CancellationRequested -= Cancel;

            switch (State)
            {
                case TaskState.Complete:
                    return;

                case TaskState.WebError:
                    State = TaskState.Ready;
                    throw new WebException(ErrorMessage);

                case TaskState.IOError:
                    State = TaskState.Ready;
                    throw new IOException(ErrorMessage);

                default:
                    State = TaskState.Ready;
                    throw new OperationCanceledException();
            }
        }

        /// <inheritdoc />
        public override void Cancel()
        {
            lock (StateLock)
            {
                if (State == TaskState.Ready || State >= TaskState.Complete) return;

                Thread.Abort();
                Thread.Join();
            }
        }
        #endregion

        #region Get sub entries
        /// <summary>
        /// Returns the name of an archive entry trimmed by the selected sub-directory prefix.
        /// </summary>
        /// <param name="entryName">The path of the archive entry relative to the archive's root.</param>
        /// <returns>The trimmed path or <see langword="null"/> if the <paramref name="entryName"/> doesn't lie within the <see cref="SubDir"/>.</returns>
        protected string GetSubEntryName(string entryName)
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
        #endregion

        #region Write entries
        /// <summary>
        /// Creates a directory in the filesystem and sets its last write time.
        /// </summary>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        /// <param name="dateTime">The last write time to set.</param>
        protected void CreateDirectory(string relativePath, DateTime dateTime)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            string directoryPath = CombinePath(relativePath);

            Directory.CreateDirectory(directoryPath);
            _directoryWriteTimes.Add(new KeyValuePair<string, DateTime>(directoryPath, dateTime));
        }

        /// <summary>
        /// Writes a file to the filesystem and sets its last write time.
        /// </summary>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        /// <param name="dateTime">The last write time to set.</param>
        /// <param name="stream">The stream containing the file data to be written.</param>
        /// <param name="length">The length of the zip entries uncompressed data, needed because stream's Length property is always 0.</param>
        /// <param name="executable"><see langword="true"/> if the file's executable bit is set; <see langword="false"/> otherwise.</param>
        protected void WriteFile(string relativePath, DateTime dateTime, Stream stream, long length, bool executable)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            string filePath = CombinePath(relativePath);
            string directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            bool alreadyExists = File.Exists(filePath);

            // If a symlink is overwritten by a normal file, remove the symlink flag
            if (alreadyExists)
            {
                string flagRelativePath = string.IsNullOrEmpty(Destination) ? relativePath : Path.Combine(Destination, relativePath);
                FlagUtils.RemoveExternalFlag(Path.Combine(TargetDir, ".symlink"), flagRelativePath);
            }

            using (var fileStream = File.Create(filePath))
                if (length != 0) StreamToFile(stream, fileStream);

            if (executable) SetExecutableBit(relativePath);
            else if (alreadyExists) RemoveExecutableBit(relativePath); // If an executable file is overwritten by a non-executable file, remove the xbit flag

            File.SetLastWriteTimeUtc(filePath, dateTime);
        }

        /// <summary>
        /// Creates a symbolic link in the filesystem if possible; stores it in a .symlink file otherwise.
        /// </summary>
        /// <param name="source">A path relative to the archive's root.</param>
        /// <param name="target">The target the symbolic link shall point to relative to <paramref name="source"/>. May use non-native path separators!</param>
        protected void CreateSymlink(string source, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            string sourceAbsolute = CombinePath(source);
            string sourceDirectory = Path.GetDirectoryName(sourceAbsolute);
            if (sourceDirectory != null && !Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    FileUtils.CreateSymlink(sourceAbsolute, target);
                    break;

                case PlatformID.Win32NT: // NTFS symbolic links require admin privileges; do not use them here
                default:
                    // Write link data as a normal file
                    File.WriteAllText(sourceAbsolute, target);

                    // Non-Unixoid OSes (e.g. Windows) can't store the symlink flag directly in the filesystem; remember in a text-file instead
                    string flagRelativePath = string.IsNullOrEmpty(Destination) ? source : Path.Combine(Destination, source);
                    FlagUtils.SetExternalFlag(Path.Combine(TargetDir, ".symlink"), flagRelativePath);
                    break;
            }
        }

        /// <summary>
        /// Creates a hard link in the filesystem if possible; creates a copy otherwise.
        /// </summary>
        /// <param name="source">A path relative to the archive's root.</param>
        /// <param name="target">The target the hard link shall point to relative to <paramref name="source"/>. May use non-native path separators!</param>
        protected void CreateHardlink(string source, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            string sourceAbsolute = CombinePath(source);
            string sourceDirectory = Path.GetDirectoryName(sourceAbsolute);
            if (sourceDirectory != null && !Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);
            string targetAbsolute = CombinePath(FileUtils.UnifySlashes(target));

            try
            {
                FileUtils.CreateHardlink(sourceAbsolute, targetAbsolute);
            }
                #region Sanity checks
            catch (PlatformNotSupportedException)
            {
                File.Copy(targetAbsolute, sourceAbsolute);
            }
            catch (UnauthorizedAccessException)
            {
                File.Copy(targetAbsolute, sourceAbsolute);
            }
            #endregion
        }

        #region Helpers
        /// <summary>
        /// Helper method for <see cref="WriteFile"/>.
        /// </summary>
        /// <param name="stream">The stream to write to a file.</param>
        /// <param name="fileStream">Stream access to the file to write.</param>
        /// <remarks>Can be overwritten for archive formats that don't simply write a <see cref="Stream"/> to a file.</remarks>
        protected virtual void StreamToFile(Stream stream, FileStream fileStream)
        {
            stream.CopyTo(fileStream);
        }

        /// <summary>
        /// Combines the extraction <see cref="TargetDir"/> path with the relative path inside the archive (ensuring only valid paths are returned).
        /// </summary>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        /// <returns>The combined path as an absolute path.</returns>
        /// <exception cref="IOException">Thrown if <paramref name="relativePath"/> is invalid (e.g. is absolute, points outside the archive's root, contains invalid characters).</exception>
        private string CombinePath(string relativePath)
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
        /// Sets the last write times of the directories that were recorded during extraction.
        /// </summary>
        /// <remarks>This must be done in a separate step, since changing anything within a directory will affect its last write time.</remarks>
        protected void SetDirectoryWriteTimes()
        {
            // Run through list backwards to ensure directories are handled "from the inside out"
            for (int index = _directoryWriteTimes.Count - 1; index >= 0; index--)
            {
                var pair = _directoryWriteTimes[index];
                Directory.SetLastWriteTimeUtc(pair.Key, pair.Value);
            }
        }
        #endregion

        #endregion

        #region Executable flag
        /// <summary>
        /// Marks a file as executable using the filesystem if possible, an .xbit file otherwise.
        /// </summary>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        private void SetExecutableBit(string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    FileUtils.SetExecutable(Path.Combine(EffectiveTargetDir, relativePath), true);
                    break;

                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                default:
                    // Non-Unixoid OSes (e.g. Windows) can't store the executable flag directly in the filesystem; remember in a text-file instead
                    string flagRelativePath = string.IsNullOrEmpty(Destination) ? relativePath : Path.Combine(Destination, relativePath);
                    FlagUtils.SetExternalFlag(Path.Combine(TargetDir, ".xbit"), flagRelativePath);
                    break;
            }
        }

        /// <summary>
        /// Marks a file as no longer executable using the filesystem if possible, an .xbit file otherwise. 
        /// </summary>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        private void RemoveExecutableBit(string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    FileUtils.SetExecutable(Path.Combine(EffectiveTargetDir, relativePath), false);
                    break;

                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                default:
                    // Non-Unixoid OSes (e.g. Windows) can't store the executable flag directly in the filesystem; remember in a text-file instead
                    string flagRelativePath = string.IsNullOrEmpty(Destination) ? relativePath : Path.Combine(Destination, relativePath);
                    FlagUtils.RemoveExternalFlag(Path.Combine(TargetDir, ".xbit"), flagRelativePath);
                    break;
            }
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <summary>
        /// Disposes the underlying <see cref="Stream"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~Extractor()
        {
            Dispose(false);
        }

        /// <summary>
        /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called manually and not by the garbage collector.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (Stream != null) Stream.Dispose();
        }
        #endregion
    }
}
