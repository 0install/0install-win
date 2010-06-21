using System;
using System.Collections.Generic;
using System.IO;
using Common.Properties;

namespace Common.Archive
{
    /// <summary>
    /// Provides methods for extracting an archive.
    /// </summary>
    public abstract class Extractor : IDisposable
    {
        #region Properties
        /// <summary>
        /// The backing stream to extract the data from.
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// The number of bytes at the beginning of the stream which should be ignored.
        /// </summary>
        public long StartOffset { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to extract an archive contained in a stream.
        /// </summary>
        /// <param name="archive">The stream containing the archive data.</param>
        /// <param name="startOffset">The number of bytes at the beginning of the stream which should be ignored.</param>
        protected Extractor(Stream archive, long startOffset)
        {
            Stream = archive;
            StartOffset = startOffset;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting a sub-directory from an archive stream.
        /// </summary>
        /// <param name="mimeType">The MIME type of archive format of the stream; must not be <see langword="null"/>.</param>
        /// <param name="stream">TThe stream containing the archive data.</param>
        /// <param name="startOffset">The number of bytes at the beginning of the stream which should be ignored.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        public static Extractor CreateExtractor(string mimeType, Stream stream, long startOffset)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            // Select the correct extractor based on the MIME type
            Extractor extractor;
            switch (mimeType)
            {
                case "application/zip": extractor = new ZipExtractor(stream, startOffset); break;
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
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        /// <exception cref="IOException">Thrown if the file couldn't be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="mimeType"/> doesn't belong to a known and supported archive type.</exception>
        public static Extractor CreateExtractor(string mimeType, string path, long startOffset)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Try to guess missing MIME type
            if (string.IsNullOrEmpty(mimeType)) mimeType = GuessMimeType(path);

            return CreateExtractor(mimeType, File.OpenRead(path), startOffset);
        }
        #endregion

        //--------------------//

        #region Static helpers
        /// <summary>
        /// Tries to guess the MIME type of an archive file by looking at its file ending.
        /// </summary>
        /// <param name="fileName">The file name to analyze.</param>
        /// <returns>The MIME type if it could be guessed; <see langword="null"/> otherwise.</returns>
        /// <remarks>The file's content is not analyzed.</remarks>
        public static string GuessMimeType(string fileName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            #endregion

            if (fileName.EndsWith(".zip")) return "application/zip";
            if (fileName.EndsWith(".cab")) return "application/vnd.ms-cab-compressed";
            if (fileName.EndsWith(".tar")) return "application/x-tar";
            if (fileName.EndsWith(".tar.gz") || fileName.EndsWith(".tgz")) return "application/x-compressed-tar";
            if (fileName.EndsWith(".tar.bz2") || fileName.EndsWith(".tbz2") || fileName.EndsWith(".tbz")) return "application/x-bzip-compressed-tar";
            if (fileName.EndsWith(".tar.lzma")) return "application/x-lzma-compressed-tar";
            if (fileName.EndsWith(".deb")) return "application/x-deb";
            if (fileName.EndsWith(".rpm")) return "application/x-rpm";
            if (fileName.EndsWith(".dmg")) return "application/x-apple-diskimage";
            return null;
        }
        #endregion

        #region Content
        /// <summary>
        /// Returns a list of all contained files and directories that would be extracted by <see cref="Extract"/>.
        /// </summary>
        /// <returns>A list of files and directories using native path separators.</returns>
        public abstract IEnumerable<string> ListContent();

        /// <summary>
        /// Returns a list of all contained directories that would be extracted by <see cref="Extract"/>.
        /// </summary>
        /// <returns>A list of directories using native path seperators.</returns>
        public abstract IEnumerable<string> ListDirectories();
        #endregion

        #region Extraction
        /// <summary>
        /// Extracts the content of a sub-directory inside an archive to a directory on the disk.
        /// </summary>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <param name="subDir">The sub-directory in the archive to be extracted; <see langword="null"/> for entire archive.</param>
        /// <exception cref="IOException">Thrown if the archive is not usable, e.g. if it contains references to '..'.</exception>
        public abstract void Extract(string target, string subDir);
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
