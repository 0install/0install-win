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

using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using JetBrains.Annotations;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a GZip-compressed TAR archive.
    /// </summary>
    public class TarGzExtractor : TarExtractor
    {
        private readonly Stream _stream;

        /// <summary>
        /// Prepares to extract a TAR archive contained in a GZip-compressed stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">The archive is damaged.</exception>
        internal TarGzExtractor([NotNull] Stream stream, [NotNull] string target)
            : base(GetDecompressionStream(stream), target)
        {
            _stream = stream;
            UnitsTotal = stream.Length;
        }

        /// <summary>
        /// Adds a GZip-extraction layer around a stream.
        /// </summary>
        /// <param name="stream">The stream containing the GZip-compressed data.</param>
        /// <returns>A stream representing the uncompressed data.</returns>
        /// <exception cref="IOException">The compressed stream contains invalid data.</exception>
        private static Stream GetDecompressionStream(Stream stream)
        {
            try
            {
                return new GZipInputStream(stream);
            }
                #region Error handling
            catch (GZipException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion
        }

        /// <inheritdoc/>
        protected override void UpdateProgress()
        {
            // Use original stream instead of decompressed stream to track progress
            UnitsProcessed = _stream.Position;
        }
    }
}
