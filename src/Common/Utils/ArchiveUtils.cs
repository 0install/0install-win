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

            if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) return "application/zip";
            if (fileName.EndsWith(".cab", StringComparison.OrdinalIgnoreCase)) return "application/vnd.ms-cab-compressed";
            if (fileName.EndsWith(".tar", StringComparison.OrdinalIgnoreCase)) return "application/x-tar";
            if (fileName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".tgz")) return "application/x-compressed-tar";
            if (fileName.EndsWith(".tar.bz2", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".tbz2") || fileName.EndsWith(".tbz")) return "application/x-bzip-compressed-tar";
            if (fileName.EndsWith(".tar.lzma", StringComparison.OrdinalIgnoreCase)) return "application/x-lzma-compressed-tar";
            if (fileName.EndsWith(".deb", StringComparison.OrdinalIgnoreCase)) return "application/x-deb";
            if (fileName.EndsWith(".rpm", StringComparison.OrdinalIgnoreCase)) return "application/x-rpm";
            if (fileName.EndsWith(".dmg", StringComparison.OrdinalIgnoreCase)) return "application/x-apple-diskimage";
            if (fileName.EndsWith(".gem", StringComparison.OrdinalIgnoreCase)) return "application/x-ruby-gem";
            return null;
        }
    }
}
