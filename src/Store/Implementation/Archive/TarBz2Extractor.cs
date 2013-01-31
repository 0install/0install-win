/*
 * Copyright 2010-2013 Bastian Eicher
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

using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation.Archive
{
    /// <summary>
    /// Provides methods for extracting a BZip2-compressed TAR archive (optionally as a background task).
    /// </summary>
    public class TarBz2Extractor : TarExtractor
    {
        #region Constructor
        /// <summary>
        /// Prepares to extract a TAR archive contained in a BZip2-compressed stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public TarBz2Extractor(Stream stream, string target)
            : base(GetDecompressionStream(stream), target)
        {}

        /// <summary>
        /// Adds a BZip2-extraction layer around a stream.
        /// </summary>
        /// <param name="stream">The stream containing the BZip2-compressed data.</param>
        /// <returns>A stream representing the uncompressed data.</returns>
        /// <exception cref="IOException">Thrown if the compressed stream contains invalid data.</exception>
        private static Stream GetDecompressionStream(Stream stream)
        {
            try
            {
                return new BZip2InputStream(stream);
            }
            catch (BZip2Exception ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
        }
        #endregion
    }
}
