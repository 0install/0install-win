/*
 * Copyright 2010-2016 Bastian Eicher
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

using System;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Tar;
using JetBrains.Annotations;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a Ruby Gem archive.
    /// </summary>
    public class RubyGemExtractor : TarGzExtractor
    {
        /// <summary>
        /// Prepares to extract a Ruby Gem archive.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="targetPath">The path to the directory to extract into.</param>
        /// <exception cref="IOException">The archive is damaged.</exception>
        internal RubyGemExtractor([NotNull] Stream stream, [NotNull] string targetPath)
            : base(GetPartialStream(stream), targetPath)
        {}

        /// <summary>
        /// Adds a layer around a stream that isolates the <c>data.tar.gz</c> file from a TAR stream.
        /// </summary>
        /// <param name="stream">The TAR stream.</param>
        /// <returns>A stream representing the <c>data.tar.gz</c> data.</returns>
        /// <exception cref="IOException">The compressed stream contains invalid data.</exception>
        private static Stream GetPartialStream(Stream stream)
        {
            try
            {
                var tar = new TarInputStream(stream);
                while (true)
                {
                    var entry = tar.GetNextEntry();
                    if (entry == null) throw new IOException(Resources.RubyGemInvalid);
                    if (entry.Name == "data.tar.gz") return tar;
                }
            }
                #region Error handling
            catch (SharpZipBaseException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            catch (InvalidDataException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion
        }
    }
}
