/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

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
