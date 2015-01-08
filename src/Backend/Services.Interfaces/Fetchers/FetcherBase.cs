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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
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
        protected readonly ITaskHandler Handler;

        /// <summary>
        /// Creates a new download fetcher.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="ZeroInstall.Store.Model.Implementation"/>s in.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        protected FetcherBase(IStore store, ITaskHandler handler)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _store = store;
            Handler = handler;
        }
        #endregion

        /// <inheritdoc/>
        public abstract void Fetch(IEnumerable<Implementation> implementations);

        /// <summary>
        /// Determines whether an <see cref="Implementation"/> is already cached.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This method only operates on original Implementations (not Selections).")]
        protected bool IsCached(Implementation implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            #endregion

            if (implementation.ID.StartsWith(ExternalImplementation.PackagePrefix)) return false;

            _store.Flush();
            return _store.Contains(implementation.ManifestDigest);
        }

        /// <summary>
        /// Executes the best possible <see cref="RetrievalMethod"/> for an <see cref="Implementation"/>.
        /// </summary>
        /// <param name="implementation">The implementation to be retrieved.</param>
        /// <remarks>Make sure <see cref="Implementation.RetrievalMethods"/> is not empty before calling this!</remarks>
        protected void Retrieve(Implementation implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            #endregion

            implementation.RetrievalMethods
                .OrderBy(x => x, RetrievalMethodRanker.Instance)
                .Try(retrievalMethod => Retrieve(retrievalMethod, implementation.ManifestDigest));
        }

        /// <summary>
        /// Executes a specific <see cref="RetrievalMethod"/>.
        /// </summary>
        /// <param name="retrievalMethod">The retrieval method to execute.</param>
        /// <param name="manifestDigest">The digest the result of the retrieval method should produce.</param>
        private void Retrieve(RetrievalMethod retrievalMethod, ManifestDigest manifestDigest)
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
            catch (ImplementationAlreadyInStoreException)
            {}
        }

        /// <summary>
        /// Handles the execution of <see cref="ExternalRetrievalMethod.Install"/>.
        /// </summary>
        private void RunNative(ExternalRetrievalMethod externalRetrievalMethod)
        {
            if (!string.IsNullOrEmpty(externalRetrievalMethod.ConfirmationQuestion))
                if (!Handler.AskQuestion(externalRetrievalMethod.ConfirmationQuestion)) throw new OperationCanceledException();

            externalRetrievalMethod.Install();
        }

        /// <summary>
        /// Executes a <see cref="Recipe"/>.
        /// </summary>
        /// <param name="recipe">The recipe to execute.</param>
        /// <param name="manifestDigest">The digest the result of the recipe should produce.</param>
        private void Cook(Recipe recipe, ManifestDigest manifestDigest)
        {
            Handler.CancellationToken.ThrowIfCancellationRequested();

            // Fail fast on unsupported archive type
            foreach (var archive in recipe.Steps.OfType<Archive>()) Extractor.VerifySupport(archive.MimeType);

            var downloadedFiles = new List<TemporaryFile>();
            try
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var downloadStep in recipe.Steps.OfType<DownloadRetrievalMethod>())
                    downloadedFiles.Add(Download(downloadStep, tag: manifestDigest));

                // More efficient special-case handling for Archive-only cases
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
        protected virtual TemporaryFile Download(DownloadRetrievalMethod retrievalMethod, object tag = null)
        {
            #region Sanity checks
            if (retrievalMethod == null) throw new ArgumentNullException("retrievalMethod");
            #endregion

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
        private void ApplyArchives(IList<Archive> archives, IList<TemporaryFile> files, ManifestDigest manifestDigest)
        {
            var archiveFileInfos = new ArchiveFileInfo[archives.Count];
            for (int i = 0; i < archiveFileInfos.Length; i++)
            {
                archiveFileInfos[i] = new ArchiveFileInfo
                {
                    Path = files[i].Path,
                    SubDir = archives[i].Extract,
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
        private void ApplyRecipe(Recipe recipe, IEnumerable<TemporaryFile> files, ManifestDigest manifestDigest)
        {
            using (var recipeDir = recipe.Apply(files, Handler, manifestDigest))
                _store.AddDirectory(recipeDir, manifestDigest, Handler);
        }
    }
}
