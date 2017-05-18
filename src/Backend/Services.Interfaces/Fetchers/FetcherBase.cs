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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Implementations.Build;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Fetchers
{
    /// <summary>
    /// Base class for <see cref="IFetcher"/> implementations using template methods.
    /// </summary>
    public abstract class FetcherBase : IFetcher
    {
        #region Dependencies
        private readonly IStore _store;

        /// <summary>A callback object used when the the user needs to be informed about progress.</summary>
        [NotNull]
        protected readonly ITaskHandler Handler;

        /// <summary>
        /// Creates a new download fetcher.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Implementation"/>s in.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        protected FetcherBase([NotNull] IStore store, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            _store = store;
            Handler = handler;
        }
        #endregion

        /// <inheritdoc/>
        public abstract void Fetch(IEnumerable<Implementation> implementations);

        /// <summary>
        /// Downloads a single <see cref="Implementation"/> to the <see cref="IStore"/>.
        /// </summary>
        /// <param name="implementation">The implementation to download.</param>
        /// <param name="tag">The <see cref="ITask.Tag"/> to set for the download process.</param>
        /// <returns>A fully qualified path to the directory containing the implementation; <c>null</c> if the requested implementation could not be found in the store or is a package implementation.</returns>
        /// <exception cref="OperationCanceledException">A download or IO task was canceled from another thread.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        [CanBeNull]
        protected abstract string Fetch([NotNull] Implementation implementation, object tag);

        /// <summary>
        /// Determines the local path of an implementation.
        /// </summary>
        /// <param name="implementation">The implementation to be located.</param>
        /// <returns>A fully qualified path to the directory containing the implementation; <c>null</c> if the requested implementation could not be found in the store or is a package implementation.</returns>
        [CanBeNull]
        protected string GetPathSafe([NotNull] ImplementationBase implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            #endregion

            if (implementation.ID.StartsWith(ExternalImplementation.PackagePrefix)) return null;

            _store.Flush();
            return _store.GetPath(implementation.ManifestDigest);
        }

        /// <summary>
        /// Executes the best possible <see cref="RetrievalMethod"/> for an <see cref="Implementation"/>.
        /// </summary>
        /// <param name="implementation">The implementation to be retrieved.</param>
        /// <remarks>Make sure <see cref="Implementation.RetrievalMethods"/> is not empty before calling this!</remarks>
        /// <exception cref="OperationCanceledException">A download or IO task was canceled from another thread.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        protected void Retrieve([NotNull] Implementation implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            #endregion

            implementation.RetrievalMethods
                .OrderBy(x => x, RetrievalMethodRanker.Instance)
                .TryAny(retrievalMethod => Retrieve(retrievalMethod, implementation.ManifestDigest));
        }

        /// <summary>
        /// Executes a specific <see cref="RetrievalMethod"/>.
        /// </summary>
        /// <param name="retrievalMethod">The retrieval method to execute.</param>
        /// <param name="manifestDigest">The digest the result of the retrieval method should produce.</param>
        /// <exception cref="OperationCanceledException">A download or IO task was canceled from another thread.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        private void Retrieve([NotNull] RetrievalMethod retrievalMethod, ManifestDigest manifestDigest)
        {
            var externalRetrievalMethod = retrievalMethod as ExternalRetrievalMethod;
            if (externalRetrievalMethod != null)
            {
                RunNative(externalRetrievalMethod);
                return;
            }

            // Treat single steps as a Recipes for easier handling
            var recipe = retrievalMethod as Recipe ?? new Recipe {Steps = {(IRecipeStep)retrievalMethod}};
            try
            {
                Cook(recipe, manifestDigest);
            }
                #region Error handling
            catch (ImplementationAlreadyInStoreException)
            {}
            catch (DigestMismatchException)
            {
                Log.Error("Damaged download: " + retrievalMethod);
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Handles the execution of <see cref="ExternalRetrievalMethod.Install"/>.
        /// </summary>
        private void RunNative([NotNull] ExternalRetrievalMethod externalRetrievalMethod)
        {
            if (!string.IsNullOrEmpty(externalRetrievalMethod.ConfirmationQuestion))
            {
                if (!Handler.Ask(externalRetrievalMethod.ConfirmationQuestion,
                    defaultAnswer: false, alternateMessage: externalRetrievalMethod.ConfirmationQuestion)) throw new OperationCanceledException();
            }

            externalRetrievalMethod.Install();
        }

        /// <summary>
        /// Executes a <see cref="Recipe"/>.
        /// </summary>
        /// <param name="recipe">The recipe to execute.</param>
        /// <param name="manifestDigest">The digest the result of the recipe should produce.</param>
        /// <exception cref="OperationCanceledException">A download or IO task was canceled from another thread.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">There is already an <see cref="Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        private void Cook([NotNull] Recipe recipe, ManifestDigest manifestDigest)
        {
            Handler.CancellationToken.ThrowIfCancellationRequested();

            // Fail fast on unsupported Archive types
            foreach (var archive in recipe.Steps.OfType<Archive>())
                ArchiveExtractor.VerifySupport(archive.MimeType);

            // Download any other Implementations required by the Recipe
            Fetch(recipe.Steps.OfType<CopyFromStep>().Select(x => x.Implementation));

            var downloadedFiles = new List<TemporaryFile>();
            try
            {
                // Download any files or archives required by the recipe
                foreach (var downloadStep in recipe.Steps.OfType<DownloadRetrievalMethod>())
                    downloadedFiles.Add(Download(downloadStep, tag: manifestDigest));

                // More efficient special-case handling for Archive-only Recipes
                if (recipe.Steps.All(step => step is Archive))
                    ApplyArchives(recipe.Steps.Cast<Archive>().ToList(), downloadedFiles, manifestDigest);
                else
                    ApplyRecipe(recipe, downloadedFiles, manifestDigest);
            }
            finally
            {
                foreach (var downloadedFile in downloadedFiles) downloadedFile.Dispose();
            }
        }

        /// <summary>
        /// Downloads a <see cref="DownloadRetrievalMethod"/> to a temporary file.
        /// </summary>
        /// <param name="retrievalMethod">The file to download.</param>
        /// <param name="tag">The <see cref="ITask.Tag"/> to set for the download process.</param>
        /// <returns>The downloaded temporary file.</returns>
        /// <exception cref="OperationCanceledException">A download was canceled from another thread.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to <see cref="IStore"/> is not permitted.</exception>
        protected virtual TemporaryFile Download([NotNull] DownloadRetrievalMethod retrievalMethod, [CanBeNull] object tag = null)
        {
            #region Sanity checks
            if (retrievalMethod == null) throw new ArgumentNullException(nameof(retrievalMethod));
            #endregion

            retrievalMethod.Validate();

            var tempFile = new TemporaryFile("0install-fetcher");
            try
            {
                Handler.RunTask(new DownloadFile(retrievalMethod.Href, tempFile, retrievalMethod.DownloadSize) {Tag = tag});
                return tempFile;
            }
                #region Error handling
            catch
            {
                tempFile.Dispose();
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Extracts <see cref="Archive"/>s to the <see cref="_store"/>.
        /// </summary>
        /// <param name="archives">The archives to extract over each other in order.</param>
        /// <param name="files">The downloaded archive files, indexes matching those in <paramref name="archives"/>.</param>
        /// <param name="manifestDigest">The digest the extracted archives should produce.</param>
        /// <exception cref="OperationCanceledException">An IO task was canceled from another thread.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">There is already an <see cref="Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        private void ApplyArchives([NotNull, ItemNotNull] IList<Archive> archives, [NotNull, ItemNotNull] IList<TemporaryFile> files, ManifestDigest manifestDigest)
        {
            var archiveFileInfos = new ArchiveFileInfo[archives.Count];
            for (int i = 0; i < archiveFileInfos.Length; i++)
            {
                archiveFileInfos[i] = new ArchiveFileInfo
                {
                    Path = files[i].Path,
                    Extract = archives[i].Extract,
                    Destination = archives[i].Destination,
                    MimeType = archives[i].MimeType,
                    StartOffset = archives[i].StartOffset,
                    OriginalSource = archives[i].Href
                };
            }

            _store.AddArchives(archiveFileInfos, manifestDigest, Handler);
        }

        /// <summary>
        /// Applies a <see cref="Recipe"/> and sends the result to the <see cref="_store"/>.
        /// </summary>
        /// <param name="recipe">The recipe to apply.</param>
        /// <param name="files">The files downloaded for the <paramref name="recipe"/> steps, order matching the <see cref="DownloadRetrievalMethod"/> steps in <see cref="Recipe.Steps"/>.</param>
        /// <param name="manifestDigest">The digest the result of the recipe should produce.</param>
        /// <exception cref="OperationCanceledException">An IO task was canceled from another thread.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">There is already an <see cref="Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        private void ApplyRecipe([NotNull] Recipe recipe, [NotNull, ItemNotNull] IEnumerable<TemporaryFile> files, ManifestDigest manifestDigest)
        {
            using (var recipeDir = recipe.Apply(files, Handler, manifestDigest))
                _store.AddDirectory(recipeDir, manifestDigest, Handler);
        }
    }
}
