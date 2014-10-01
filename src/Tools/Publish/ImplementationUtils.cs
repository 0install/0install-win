/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using NanoByte.Common.Undo;
using NanoByte.Common.Utils;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Helper methods for manipulating <see cref="Implementation"/>s.
    /// </summary>
    public static class ImplementationUtils
    {
        #region Build
        /// <summary>
        /// Creates a new <see cref="Implementation"/> by completing a <see cref="RetrievalMethod"/> and calculating the resulting <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="retrievalMethod">The <see cref="RetrievalMethod"/> to use.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="keepDownloads">Adds the downloaded archive to the default <see cref="IStore"/> when set to <see langword="true"/>.</param>
        /// <returns>A newly created <see cref="Implementation"/> containing one <see cref="Archive"/>.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">There was a problem extracting the archive.</exception>
        /// <exception cref="WebException">There was a problem downloading the archive.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to temporary files was not permitted.</exception>
        /// <exception cref="NotSupportedException">The archive's MIME type could not be determined.</exception>
        public static Implementation Build(RetrievalMethod retrievalMethod, ITaskHandler handler, bool keepDownloads = false)
        {
            #region Sanity checks
            if (retrievalMethod == null) throw new ArgumentNullException("retrievalMethod");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var implementationDir = retrievalMethod.DownloadAndApply(handler);
            try
            {
                var digest = GenerateDigest(implementationDir, handler, keepDownloads);
                return new Implementation {ID = "sha1new=" + digest.Sha1New, ManifestDigest = digest, RetrievalMethods = {retrievalMethod}};
            }
            finally
            {
                implementationDir.Dispose();
            }
        }
        #endregion

        #region Add missing
        /// <summary>
        /// Adds missing data (by downloading and infering) to an <see cref="Implementation"/>.
        /// </summary>
        /// <param name="implementation">The <see cref="Implementation"/> to add data to.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        /// <param name="keepDownloads"><see langword="true"/> to store the directory as an implementation in the default <see cref="IStore"/>.</param>
        /// <exception cref="OperationCanceledException">The user canceled the operation.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">There is a problem access a temporary file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a temporary file is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An existing digest does not match the newly calculated one.</exception>
        public static void AddMissing(this Implementation implementation, ITaskHandler handler, ICommandExecutor executor = null, bool keepDownloads = false)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (executor == null) executor = new SimpleCommandExecutor();

            ConvertSha256ToSha256New(implementation, executor);

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var retrievalMethod in implementation.RetrievalMethods)
            {
                if (implementation.IsManifestDigestMissing() || retrievalMethod.IsDownloadSizeMissing())
                {
                    using (var tempDir = retrievalMethod.DownloadAndApply(handler, executor))
                        implementation.UpdateDigest(tempDir, handler, executor, keepDownloads);
                }
            }

            if (string.IsNullOrEmpty(implementation.ID)) implementation.ID = "sha1new=" + implementation.ManifestDigest.Sha1New;
        }

        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength", Justification = "We are explicitly looking for empty strings as opposed to null strings.")]
        private static bool IsManifestDigestMissing(this Implementation implementation)
        {
            return implementation.ManifestDigest == default(ManifestDigest) ||
                // Empty strings are used in 0template to indicate that the user wishes this value to be calculated
                implementation.ManifestDigest.Sha1New == "" ||
                implementation.ManifestDigest.Sha256 == "" ||
                implementation.ManifestDigest.Sha256New == "";
        }

        private static bool IsDownloadSizeMissing(this RetrievalMethod retrievalMethod)
        {
            var downloadRetrievalMethod = retrievalMethod as DownloadRetrievalMethod;
            return downloadRetrievalMethod != null && downloadRetrievalMethod.Size == 0;
        }

        private static void ConvertSha256ToSha256New(Implementation implementation, ICommandExecutor executor)
        {
            if (string.IsNullOrEmpty(implementation.ManifestDigest.Sha256) || !string.IsNullOrEmpty(implementation.ManifestDigest.Sha256New)) return;

            var digest = new ManifestDigest(
                implementation.ManifestDigest.Sha1,
                implementation.ManifestDigest.Sha1New,
                implementation.ManifestDigest.Sha256,
                implementation.ManifestDigest.Sha256.Base16Decode().Base32Encode());

            executor.Execute(new SetValueCommand<ManifestDigest>(() => implementation.ManifestDigest, value => implementation.ManifestDigest = value, digest));
        }
        #endregion

        #region Digest helpers
        /// <summary>
        /// Updates the <see cref="ManifestDigest"/> in an <see cref="Implementation"/>.
        /// </summary>
        /// <param name="implementation">The <see cref="Implementation"/> to update.</param>
        /// <param name="path">The path of the directory to generate the digest for.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        /// <param name="keepDownloads"><see langword="true"/> to store the directory as an implementation in the default <see cref="IStore"/>.</param>
        /// <exception cref="OperationCanceledException">The user canceled the operation.</exception>
        /// <exception cref="IOException">There is a problem access a temporary file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a temporary file is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An existing digest does not match the newly calculated one.</exception>
        private static void UpdateDigest(this Implementation implementation, string path, ITaskHandler handler, ICommandExecutor executor, bool keepDownloads = false)
        {
            var digest = GenerateDigest(path, handler, keepDownloads);

            if (implementation.ManifestDigest == default(ManifestDigest))
                executor.Execute(new SetValueCommand<ManifestDigest>(() => implementation.ManifestDigest, value => implementation.ManifestDigest = value, digest));
            else if (!digest.PartialEquals(implementation.ManifestDigest))
                throw new DigestMismatchException(implementation.ManifestDigest.ToString(), digest.ToString());
        }

        /// <summary>
        /// Generates the <see cref="ManifestDigest"/> for a directory.
        /// </summary>
        /// <param name="path">The path of the directory to generate the digest for.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="keepDownloads"><see langword="true"/> to store the directory as an implementation in the default <see cref="IStore"/>.</param>
        /// <returns>The newly generated digest.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the operation.</exception>
        /// <exception cref="IOException">There is a problem access a temporary file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a temporary file is not permitted.</exception>
        public static ManifestDigest GenerateDigest(string path, ITaskHandler handler, bool keepDownloads = false)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var digest = new ManifestDigest();

            // Generate manifest for each available format...
            foreach (var format in ManifestFormat.Recommended)
                // ... and add the resulting digest to the return value
            {
                var generator = new ManifestGenerator(path, format);
                handler.RunTask(generator);
                ManifestDigest.ParseID(generator.Result.CalculateDigest(), ref digest);
            }

            if (digest.PartialEquals(ManifestDigest.Empty))
                Log.Warn(Resources.EmptyImplementation);

            if (keepDownloads)
            {
                try
                {
                    StoreFactory.CreateDefault().AddDirectory(path, digest, handler);
                }
                catch (ImplementationAlreadyInStoreException)
                {}
            }

            return digest;
        }
        #endregion
    }
}
