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
using System.IO;
using JetBrains.Annotations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Creates an archive from a directory. Preserves execuable bits, symlinks and timestamps.
    /// </summary>
    public abstract class ArchiveGenerator : DirectoryWalkTask, IDisposable
    {
        /// <inheritdoc/>
        public override string Name { get { return string.Format(Resources.CreatingArchive, OutputArchive); } }

        /// <summary>
        /// The path of the file to create.
        /// </summary>
        [CanBeNull]
        public string OutputArchive { get; private set; }

        /// <summary>
        /// Prepares to generate an archive from a directory.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to capture/store in the archive.</param>
        protected ArchiveGenerator([NotNull] string sourceDirectory)
            : base(sourceDirectory)
        {}

        #region Factory methods
        /// <summary>
        /// Creates a new <see cref="ArchiveGenerator"/> for creating an archive from a directory and writing it to a stream.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to capture/store in the archive.</param>
        /// <param name="stream">The stream to write the generated archive to. Will be disposed when the generator is disposed.</param>
        /// <param name="mimeType">The MIME type of archive format to create.</param>
        /// <exception cref="NotSupportedException">The <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        [NotNull]
        public static ArchiveGenerator Create([NotNull] string sourceDirectory, [NotNull] Stream stream, string mimeType)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourceDirectory)) throw new ArgumentNullException("sourceDirectory");
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            switch (mimeType)
            {
                case Archive.MimeTypeZip:
                    return new ZipGenerator(sourceDirectory, stream);
                case Archive.MimeTypeTar:
                    return new TarGenerator(sourceDirectory, stream);
                case Archive.MimeTypeTarGzip:
                    return new TarGzGenerator(sourceDirectory, stream);
                case Archive.MimeTypeTarBzip:
                    return new TarBz2Generator(sourceDirectory, stream);
                default:
                    throw new NotSupportedException(string.Format(Resources.UnsupportedArchiveMimeType, mimeType));
            }
        }

        /// <summary>
        /// Creates a new <see cref="ArchiveGenerator"/> for creating an archive from a directory and writing it to a file.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to capture/store in the archive.</param>
        /// <param name="path">The path of the archive file to create.</param>
        /// <param name="mimeType">The MIME type of archive format to create. Leave <see langword="null"/> to guess based on <paramref name="path"/>.</param>
        /// <exception cref="NotSupportedException">The <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        /// <exception cref="IOException">Failed to create the archive file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the archive file was denied.</exception>
        [NotNull]
        public static ArchiveGenerator Create([NotNull] string sourceDirectory, [NotNull] string path, [CanBeNull] string mimeType = null)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourceDirectory)) throw new ArgumentNullException("sourceDirectory");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            var generator = Create(sourceDirectory, File.Create(path), mimeType ?? Archive.GuessMimeType(path));
            generator.OutputArchive = path;
            return generator;
        }
        #endregion

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