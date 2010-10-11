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
using System.ComponentModel;
using System.IO;
using System.Text;
using Common;
using Common.Compression;
using Common.Streams;
using Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation.Archive
{
    /// <summary>
    /// Provides methods for extracting an archive (optionally as a background task).
    /// </summary>
    public abstract class Extractor : IOProgress, IDisposable
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
        public override string Name { get { return _name; } }

        /// <summary>
        /// The backing stream to extract the data from.
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// The sub-directory in the archive to be extracted; <see langword="null"/> for entire archive.
        /// </summary>
        [Description("The sub-directory in the archive to be extracted; null for entire archive.")]
        public string SubDir { get; set; }

        /// <summary>
        /// The path to the directory to extract into.
        /// </summary>
        [Description("The path to the directory to extract into.")]
        public string Target { get; protected set; }
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
            Target = target;

            BytesTotal = stream.Length;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting a sub-directory from an archive stream.
        /// </summary>
        /// <param name="mimeType">The MIME type of archive format of the stream; must not be <see langword="null"/>.</param>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        public static Extractor CreateExtractor(string mimeType, Stream stream, string target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            // Select the correct extractor based on the MIME type
            Extractor extractor;
            switch (mimeType)
            {
                case "application/zip": extractor = new ZipExtractor(stream, target); break;
                case "application/x-tar": extractor = new TarExtractor(stream, target); break;
                case "application/x-compressed-tar": extractor = new TarGzExtractor(stream, target); break;
                case "application/x-bzip-compressed-tar": extractor = new TarBz2Extractor(stream, target); break;
                case "application/x-lzma-compressed-tar": extractor = new TarLzmaExtractor(stream, target); break;
                default: throw new NotSupportedException(Resources.UnknownMimeType);
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

            var extractor = CreateExtractor(mimeType, new OffsetStream(File.OpenRead(path), startOffset), target);
            extractor._name = Path.GetFileName(path);
            return extractor;
        }
        #endregion

        //--------------------//

        #region Control
        /// <inheritdoc />
        public override void Cancel()
        {
            lock (StateLock)
            {
                if (State == ProgressState.Ready || State >= ProgressState.Complete) return;

                Thread.Abort();
            }

            Thread.Join();

            lock (StateLock)
            {
                // Reset the state so the task can be started again
                State = ProgressState.Ready;
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
            entryName = StringUtils.UnifySlashes(entryName);

            if (!string.IsNullOrEmpty(SubDir))
            {
                // Remove leading slashes
                SubDir = SubDir.TrimStart(new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar});

                // Only extract objects within the selected sub-directory
                entryName = entryName.StartsWith(SubDir) ? entryName.Substring(SubDir.Length) : null;
            }

            if (entryName != null) entryName = entryName.TrimStart(Path.DirectorySeparatorChar);

            return entryName;
        }
        #endregion

        #region Write entries
        /// <summary>
        /// Creates a directory in the file system and sets its last write time.
        /// </summary>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        /// <param name="dateTime">The last write time to set.</param>
        protected void CreateDirectory(string relativePath, DateTime dateTime)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            string directoryPath = CombinePath(Target, relativePath);

            Directory.CreateDirectory(directoryPath);
            _directoryWriteTimes.Add(new KeyValuePair<string, DateTime>(directoryPath, dateTime));
        }

        /// <summary>
        /// Writes a file to the file system and sets its last write time.
        /// </summary>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        /// <param name="dateTime">The last write time to set.</param>
        /// <param name="stream">The stream containing the file data to be written.</param>
        /// <param name="length">The length of the zip entries uncompressed data, needed because stream's Length property is always 0.</param>
        /// <param name="executable"><see langword="true"/> if the file's executable bit was set; <see langword="false"/> otherwise.</param>
        protected void WriteFile(string relativePath, DateTime dateTime, Stream stream, long length, bool executable)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            string filePath = CombinePath(Target, relativePath);
            string directoryPath = Path.GetDirectoryName(filePath);

            bool alreadyExists = File.Exists(filePath);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            using (var fileStream = File.Create(filePath))
                if (length != 0) StreamToFile(stream, fileStream);

            if (executable) SetExecutableBit(relativePath);
            // If an executable file is overwritten by a non-executable file, remove the xbit flag
            else if (alreadyExists) RemoveExecutableBit(relativePath);

            File.SetLastWriteTimeUtc(filePath, dateTime);
        }

        /// <summary>
        /// Helper method for <see cref="WriteFile"/>.
        /// </summary>
        /// <param name="stream">The stream to write to a file.</param>
        /// <param name="fileStream">Stream access to the file to write.</param>
        /// <remarks>Can be overwritten for archive formats that don't simply write a <see cref="Stream"/> to a file.</remarks>
        protected virtual void StreamToFile(Stream stream, FileStream fileStream)
        {
            StreamUtils.Copy(stream, fileStream, 4096);
        }

        /// <summary>
        /// Combines the extraction target path with the relative path inside the archive.
        /// </summary>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        /// <returns>The combined path.</returns>
        /// <exception cref="IOException">Thrown if <paramref name="relativePath"/> is absolute or points outside the archive's root.</exception>
        private static string CombinePath(string target, string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            if (Path.IsPathRooted(relativePath) || relativePath.Contains(".." + Path.DirectorySeparatorChar)) throw new IOException(Resources.ArchiveInvalid);

            return Path.Combine(target, relativePath);
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

        #region Executable flag
        /// <summary>
        /// Marks a file as executable using the file-system if possible, an .xbit file otherwise.
        /// </summary>
        /// <param name="path">A path relative to the archive's root.</param>
        private void SetExecutableBit(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    FileUtils.SetExecutable(Path.Combine(Target, path), true);
                    break;

                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                default:
                {
                    // Non-Unixoid OSes (e.g. Windows) can't store the executable bit in the filesystem directly
                    // Remember in a text-file instead
                    string xbitFilePath = Path.Combine(Target, ".xbit");

                    // Use default encoding: UTF-8 without BOM
                    using (var xbitWriter = File.AppendText(xbitFilePath))
                    {
                        xbitWriter.NewLine = "\n";
                        xbitWriter.WriteLine("/" + path.Replace(Path.DirectorySeparatorChar, '/'));
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Marks a file as no longer executable using the file-system if possible, an .xbit file otherwise. 
        /// </summary>
        /// <param name="path">A path relative to the archive's root.</param>
        private void RemoveExecutableBit(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    FileUtils.SetExecutable(Path.Combine(Target, path), false);
                    break;

                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                default:
                    // Non-Unixoid OSes (e.g. Windows) can't store the executable bit in the filesystem directly
                    // Remember in a text-file instead
                    string xbitFilePath = Path.Combine(Target, ".xbit");
                    if (!File.Exists(xbitFilePath)) return;

                    string xbitFileContent = File.ReadAllText(xbitFilePath);
                    xbitFileContent = xbitFileContent.Replace("/" + path + "\n", "");
                    File.WriteAllText(xbitFilePath, xbitFileContent, new UTF8Encoding(false));
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
        }

        ~Extractor()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Stream != null) Stream.Dispose();
            }
        }
        #endregion
    }
}
