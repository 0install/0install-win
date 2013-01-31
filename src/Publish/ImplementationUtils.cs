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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Common.Collections;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Helper methods for manipulating <see cref="Implementation"/>s.
    /// </summary>
    public static class ImplementationUtils
    {
        /// <summary>
        /// Creates a new <see cref="Implementation"/> by completing a <see cref="RetrievalMethod"/> and calculating the resulting <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="retrievalMethod">The <see cref="RetrievalMethod"/> to use.</param>
        /// <param name="store">Adds the downloaded archive to the default <see cref="IStore"/> when set to <see langword="true"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>A newly created <see cref="Implementation"/> containing one <see cref="Archive"/>.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if there was a problem extracting the archive.</exception>
        /// <exception cref="WebException">Thrown if there was a problem downloading the archive.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to temporary files was not permitted.</exception>
        /// <exception cref="NotSupportedException">Thrown if the archive's MIME type could not be determined.</exception>
        public static Implementation Build(RetrievalMethod retrievalMethod, bool store, ITaskHandler handler)
        {
            #region Sanity checks
            if (retrievalMethod == null) throw new ArgumentNullException("retrievalMethod");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            TemporaryDirectory implementationDir = null;
            new PerTypeDispatcher<RetrievalMethod>(false)
            {
                (Archive archive) => implementationDir = DownloadAndExtractArchive(archive, handler),
                (Recipe recipe) => implementationDir = ApplyRecipe(recipe, handler)
            }.Dispatch(retrievalMethod);

            try
            {
                var digest = GenerateDigest(implementationDir.Path, store, handler);
                return new Implementation {ID = "sha1new=" + digest.Sha1New, ManifestDigest = digest, RetrievalMethods = {retrievalMethod}};
            }
            finally
            {
                implementationDir.Dispose();
            }
        }

        /// <summary>
        /// Adds missing data (by downloading and infering) to an <see cref="Implementation"/>.
        /// </summary>
        /// <param name="implementation">The <see cref="Implementation"/> to add data to.</param>
        /// <param name="store"><see langword="true"/> to store the directory as an implementation in the default <see cref="IStore"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        public static void AddMissing(Implementation implementation, bool store, ITaskHandler handler)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Convert sha256 to sha256new
            if (!string.IsNullOrEmpty(implementation.ManifestDigest.Sha256) && string.IsNullOrEmpty(implementation.ManifestDigest.Sha256New))
            {
                implementation.ManifestDigest = new ManifestDigest(
                    implementation.ManifestDigest.Sha1,
                    implementation.ManifestDigest.Sha1New,
                    implementation.ManifestDigest.Sha256,
                    implementation.ManifestDigest.Sha256.Base16Decode().Base32Encode());
            }

            new PerTypeDispatcher<RetrievalMethod>(true)
            {
                (Archive archive) =>
                {
                    // Download archives if digest or archive information is missing
                    if (implementation.ManifestDigest == default(ManifestDigest) || archive.Size == 0)
                    {
                        ManifestDigest digest;
                        using (var tempDir = DownloadAndExtractArchive(archive, handler))
                            digest = GenerateDigest(tempDir.Path, store, handler);

                        if (implementation.ManifestDigest == default(ManifestDigest))
                        { // No existing digest, set from archive
                            implementation.ManifestDigest = digest;
                        }
                        else if (digest != implementation.ManifestDigest)
                        { // Archive does not match existing digest
                            throw new DigestMismatchException(implementation.ManifestDigest.ToString(), null, digest.ToString(), null);
                        }
                    }
                },
                (Recipe recipe) =>
                {
                    // Download recipes if digest is missing
                    if (implementation.ManifestDigest == default(ManifestDigest))
                    {
                        using (var tempDir = ApplyRecipe(recipe, handler))
                            implementation.ManifestDigest = GenerateDigest(tempDir.Path, store, handler);
                    }
                }
            }.Dispatch(implementation.RetrievalMethods);

            if (string.IsNullOrEmpty(implementation.ID)) implementation.ID = "sha1new=" + implementation.ManifestDigest.Sha1New;
        }

        /// <summary>
        /// Downloads and extracts an <see cref="Archive"/> and adds missing properties.
        /// </summary>
        /// <param name="archive">The <see cref="Archive"/> to be downloaded.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>A temporary directory containing the contents of the archive.</returns>
        public static TemporaryDirectory DownloadAndExtractArchive(Archive archive, ITaskHandler handler)
        {
            #region Sanity checks
            if (archive == null) throw new ArgumentNullException("archive");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            using (var tempFile = DownloadArchive(archive, handler))
            {
                var extractionDir = new TemporaryDirectory("0publish");
                try
                {
                    using (var extractor = Extractor.CreateExtractor(archive.MimeType, tempFile.Path, 0, extractionDir.Path))
                    {
                        extractor.SubDir = archive.Extract;
                        handler.RunTask(extractor, null); // Defer task to handler
                    }
                    return extractionDir;
                }
                    #region Error handling
                catch
                {
                    extractionDir.Dispose();
                    throw;
                }
                #endregion
            }
        }

        /// <summary>
        /// Downloads an <see cref="Archive"/> and adds missing properties.
        /// </summary>
        /// <param name="archive">The <see cref="Archive"/> to be downloaded.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>A temporary file containing the archive.</returns>
        public static TemporaryFile DownloadArchive(Archive archive, ITaskHandler handler)
        {
            #region Sanity checks
            if (archive == null) throw new ArgumentNullException("archive");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Guess missing MIME type
            if (String.IsNullOrEmpty(archive.MimeType)) archive.MimeType = ArchiveUtils.GuessMimeType(archive.Location.ToString());

            // Download the arhive
            var archiveFile = new TemporaryFile("0publish");
            handler.RunTask(new DownloadFile(archive.Location, archiveFile.Path), null); // Defer task to handler

            // Set downloaded file size
            archive.Size = new FileInfo(archiveFile.Path).Length;

            return archiveFile;
        }

        /// <summary>
        /// Applies a <see cref="Recipe"/> and adds missing properties.
        /// </summary>
        /// <param name="recipe">The <see cref="Recipe"/> to be applied.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>A temporary directory containing the result of the recipe.</returns>
        public static TemporaryDirectory ApplyRecipe(Recipe recipe, ITaskHandler handler)
        {
            #region Sanity checks
            if (recipe == null) throw new ArgumentNullException("recipe");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var downloadedArchives = new List<ArchiveFileInfo>();
            try
            {
                // Download all archives required by the recipe
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (var archive in recipe.Steps.OfType<Archive>())
                    downloadedArchives.Add(new ArchiveFileInfo {Path = DownloadArchive(archive, handler).Path, SubDir = archive.Extract, MimeType = archive.MimeType});
                // ReSharper restore LoopCanBeConvertedToQuery

                // Apply the recipe
                return RecipeUtils.ApplyRecipe(recipe, downloadedArchives, handler, null);
            }
            finally
            {
                // Clean up temporary archive files
                foreach (var archive in downloadedArchives) File.Delete(archive.Path);
            }
        }

        /// <summary>
        /// Generates the <see cref="ManifestDigest"/> for a directory.
        /// </summary>
        /// <param name="path">The path of the directory to generate the digest for.</param>
        /// <param name="store"><see langword="true"/> to store the directory as an implementation in the default <see cref="IStore"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>The newly generated digest.</returns>
        private static ManifestDigest GenerateDigest(string path, bool store, ITaskHandler handler)
        {
            var digest = Manifest.CreateDigest(path, handler);
            if (store)
            {
                try
                {
                    StoreProvider.CreateDefault().AddDirectory(path, digest, handler);
                }
                catch (ImplementationAlreadyInStoreException)
                {}
            }
            return digest;
        }
    }
}
