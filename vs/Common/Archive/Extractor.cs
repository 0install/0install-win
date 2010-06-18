using System;
using System.IO;

namespace Common.Archive
{
    /// <summary>
    /// Provides methods for extracting archives.
    /// </summary>
    public abstract class Extractor : IDisposable
    {
        #region Properties
        /// <summary>
        /// The backing stream to extract the data from.
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// The sub-directory within the archive to extract; may be <see langword="null"/>.
        /// </summary>
        public string SubDir { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to extract an archive contained in a stream.
        /// </summary>
        /// <param name="archive">The stream containing the archive data.</param>
        /// <param name="subDir">The sub-directory within the archive to extract; may be <see langword="null"/>.</param>
        protected Extractor(Stream archive, string subDir)
        {
            Stream = archive;
            SubDir = subDir ?? "";
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting a sub-directory from an archive stream with a specific MIME type.
        /// </summary>
        /// <param name="stream">TThe stream containing the archive data.</param>
        /// <param name="mimeType">The MIME type of archive format of the file.</param>
        /// <param name="subDir">The sub-directory in the archive to be extracted.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        public static Extractor CreateExtractor(Stream stream, string mimeType, string subDir)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            switch (mimeType)
            {
                case "application/zip": return new ZipExtractor(stream, subDir);
                default: throw new NotSupportedException("Unknown MIME type");
            }
        }

        #region Variantions
        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting a sub-directory from an archive file with a specific MIME type.
        /// </summary>
        /// <param name="path">The file to be extracted.</param>
        /// <param name="mimeType">The MIME type of archive format of the file.</param>
        /// <param name="subDir">The sub-directory in the archive to be extracted.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        public static Extractor CreateExtractor(string path, string mimeType, string subDir)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            return CreateExtractor(File.OpenRead(path), mimeType, subDir);
        }

        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting a complete archive file with a specific MIME type.
        /// </summary>
        /// <param name="path">The file to be extracted.</param>
        /// <param name="mimeType">The MIME type of archive format of the file.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        public static Extractor CreateExtractor(string path, string mimeType)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            return CreateExtractor(path, mimeType, "");
        }

        /// <summary>
        /// Creates a new <see cref="Extractor"/> for extracting a complete archive file.
        /// </summary>
        /// <param name="path">The file to be extracted.</param>
        /// <returns>The newly created <see cref="Extractor"/>.</returns>
        public static Extractor CreateExtractor(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return CreateExtractor(path, GuessMimeType(path));
        }
        #endregion

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

        #region Extraction
        /// <summary>
        /// Extracts the content of an archive to a directory on the disk.
        /// </summary>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is not usable, e.g. if it contains references to '..'.</exception>
        public abstract void Extract(string target);
        #endregion

        //--------------------//

        #region Dispose

        /// <summary>
        /// Disposes the underlying file stream.
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
