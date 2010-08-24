/*
 * Copyright 2006-2010 Bastian Eicher, Roland Leopold Walkling
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
using ICSharpCode.SharpZipLib.GZip;

namespace Common.Archive
{
    /// <summary>
    /// Provides methods for extracting a GZip-compressed TAR archive (optionally as a background task).
    /// </summary>
    public class TarGzExtractor : TarExtractor
    {
        #region Constructor
        /// <summary>
        /// Prepares to extract a TAR archive contained in a GZip-compressed stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive's data.</param>
        /// <param name="startOffset">The number of bytes at the beginning of the stream which should be ignored.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public TarGzExtractor(Stream stream, long startOffset, string target)
            : base(GetDecompressionStream(stream), startOffset, target)
        {}

        /// <summary>
        /// Adds a BZip2-extraction layer above a stream.
        /// </summary>
        private static Stream GetDecompressionStream(Stream compressed)
        {
            try { return new GZipInputStream(compressed); }
            catch (GZipException ex)
            {
                // Make sure only standard exception types are thrown to the outside
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
        }
        #endregion
    }
}
