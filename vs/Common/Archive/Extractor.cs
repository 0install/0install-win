using System;
using System.Collections.Generic;
using System.IO;
using Common.Helpers;
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
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
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
        /// <exception cref="IOException">Thrown if the archive is damaged or if the file couldn't be read.</exception>
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
        /// Returns a list of all files and directories contained in the archive.
        /// </summary>
        /// <returns>A list of files and directories using native path separators.</returns>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public abstract IEnumerable<string> ListContent();

        /// <summary>
        /// Returns a list of all directories contained in the archive.
        /// </summary>
        /// <returns>A list of directories using native path separators.</returns>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public abstract IEnumerable<string> ListDirectories();
        #endregion

        #region Extraction
        /// <summary>
        /// Extracts the content of a sub-directory inside an archive to a directory on the disk.
        /// </summary>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <param name="subDir">The sub-directory in the archive to be extracted; <see langword="null"/> for entire archive.</param>
        /// <param name="extractionProgress">Callback to track the progress of extracting files; may be <see langword="null"/>.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged or not usable.</exception>
        public abstract void Extract(string target, string subDir, ProgressCallback extractionProgress);
        #endregion

        //--------------------//

        #region Write entries
        /// <summary>
        /// Creates a directory in the file system and sets its last write time.
        /// </summary>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        /// <param name="dateTime">The last write time to set.</param>
        protected static void CreateDirectory(string target, string relativePath, DateTime dateTime)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            #endregion

            string directoryPath = CombinePath(target, relativePath);

            Directory.CreateDirectory(directoryPath);
            Directory.SetLastWriteTimeUtc(directoryPath, dateTime);
        }

        /// <summary>
        /// Writes a file to the file system and sets its last write time.
        /// </summary>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <param name="relativePath">A path relative to the archive's root.</param>
        /// <param name="dateTime">The last write time to set.</param>
        /// <param name="stream">The stream containing the file data to be written.</param>
        /// <param name="length">The length of the zip entries uncompressed data, needed because stream's Length property is always 0.</param>
        /// <param name="executable"><see langword="true"/> if the file's executable bit was set; <see langword="false"/> otherwise.</param>
        protected static void WriteFile(string target, string relativePath, DateTime dateTime, Stream stream, long length, bool executable)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            string filePath = CombinePath(target, relativePath);
            string directoryPath = Path.GetDirectoryName(filePath);

            bool alreadyExists = File.Exists(filePath);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            
            using (var fileStream = File.Create(filePath))
                if (length != 0) StreamHelper.Copy(stream, fileStream, 4096);

            if (executable) SetExecutableBit(target, relativePath);
            // If an executable file is overwritten by a non-executable file, remove the xbit flag
            else if (alreadyExists) RemoveExecutableBit(target, relativePath);

            Directory.SetLastWriteTimeUtc(filePath, dateTime);
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

            if (Path.IsPathRooted(target) && target.Contains("../")) throw new IOException(Resources.ArchiveInvalid);

            return Path.Combine(target, relativePath);
        }
        #endregion

        #region Executable flag
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativePath">The path to the directory to extract into.</param>
        /// <param name="path">A path relative to the archive's root.</param>
        private static void SetExecutableBit(string relativePath, string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                {
                    // ToDo: Set Unix octals
                    break;
                }

                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                default:
                {
                    // Non-Unixoid OSes (e.g. Windows) can't store the executable bit in the filesystem directly
                    // Remember in a text-file instead
                    string xbitFilePath = Path.Combine(relativePath, ".xbit");

                    // Default encoding (UTF8 without BOM)
                    using (var xbitWriter = File.AppendText(xbitFilePath))
                    {
                        xbitWriter.NewLine = "\n";
                        xbitWriter.WriteLine("/" + path);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativePath">The path to the directory to extract into.</param>
        /// <param name="path">A path relative to the archive's root.</param>
        private static void RemoveExecutableBit(string relativePath, string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            string xbitFilePath = Path.Combine(relativePath, ".xbit");
            if (!File.Exists(xbitFilePath)) return;

            string xbitFileContent = File.ReadAllText(xbitFilePath);
            xbitFileContent = xbitFileContent.Replace("/" + path + "\n", "");
            File.WriteAllText(xbitFilePath, xbitFileContent);
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
