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

using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Deployment.Compression.Cab;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a MS Cabinet archive.
    /// </summary>
    public class CabExtractor : MicrosoftExtractor
    {
        /// <summary>
        /// Prepares to extract a MS Cabinet archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">The archive is damaged.</exception>
        internal CabExtractor([NotNull] Stream stream, [NotNull] string target)
            : base(target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            CabStream = stream;

            try
            {
                UnitsTotal = CabEngine.GetFileInfo(this, _ => true).Sum(x => x.Length);
            }
                #region Error handling
            catch (CabException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion
        }

        /// <inheritdoc/>
        protected override void Execute()
        {
            State = TaskState.Data;

            try
            {
                if (!Directory.Exists(EffectiveTargetDir)) Directory.CreateDirectory(EffectiveTargetDir);
                CabEngine.Unpack(this, _ => true);
            }
                #region Error handling
            catch (CabException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion

            State = TaskState.Complete;
        }
    }
}
