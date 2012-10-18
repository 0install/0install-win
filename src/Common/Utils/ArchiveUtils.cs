using System;

namespace Common.Utils
{
    /// <summary>
    /// Provides helper methods for dealing with archive files.
    /// </summary>
    public static class ArchiveUtils
    {
        /// <summary>
        /// Tries to guess the MIME type of an archive file by looking at its file extension.
        /// </summary>
        /// <param name="fileName">The file name to analyze.</param>
        /// <returns>The MIME type if it could be guessed; <see langword="null"/> otherwise.</returns>
        /// <remarks>The file's content is not analyzed.</remarks>
        public static string GuessMimeType(string fileName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            #endregion

            if (fileName.EndsWithIgnoreCase(".zip")) return "application/zip";
            if (fileName.EndsWithIgnoreCase(".cab")) return "application/vnd.ms-cab-compressed";
            if (fileName.EndsWithIgnoreCase(".tar")) return "application/x-tar";
            if (fileName.EndsWithIgnoreCase(".tar.gz") || fileName.EndsWithIgnoreCase(".tgz")) return "application/x-compressed-tar";
            if (fileName.EndsWithIgnoreCase(".tar.bz2") || fileName.EndsWithIgnoreCase(".tbz2") || fileName.EndsWithIgnoreCase(".tbz")) return "application/x-bzip-compressed-tar";
            if (fileName.EndsWithIgnoreCase(".tar.lzma")) return "application/x-lzma-compressed-tar";
            if (fileName.EndsWithIgnoreCase(".deb")) return "application/x-deb";
            if (fileName.EndsWithIgnoreCase(".rpm")) return "application/x-rpm";
            if (fileName.EndsWithIgnoreCase(".dmg")) return "application/x-apple-diskimage";
            if (fileName.EndsWithIgnoreCase(".gem")) return "application/x-ruby-gem";
            return null;
        }
    }
}
