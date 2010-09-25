/*
 * Copyright 2006-2010 Bastian Eicher
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

using System.IO;
using Common.Properties;
using SevenZip;

namespace Common.Archive
{
    /// <summary>
    /// Provides methods for extracting a LZMA-compressed TAR archive (optionally as a background task).
    /// </summary>
    public class TarLzmaExtractor : TarExtractor
    {
        #region Constructor
        /// <summary>
        /// Prepares to extract a TAR archive contained in a LZMA-compressed stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public TarLzmaExtractor(Stream stream, string target)
            : base(GetDecompressionStream(stream), target)
        {}

        /// <summary>
        /// Adds a LZMA-extraction layer around a stream.
        /// </summary>
        /// <param name="stream">The stream containing the LZMA-compressed data.</param>
        /// <returns>A stream representing the uncompressed data.</returns>
        /// <exception cref="IOException">Thrown if the compressed stream contains invalid data.</exception>
        private static Stream GetDecompressionStream(Stream stream)
        {
            try { return new LzmaInputStream(stream); }
            catch (InvalidParamException ex)
            {
                // Make sure only standard exception types are thrown to the outside
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
        }
        #endregion
    }
}
