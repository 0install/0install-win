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
using System.Linq;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Fetchers
{
    /// <summary>
    /// An abstract base class for <see cref="IFetcher"/> implementations.
    /// </summary>
    public abstract class FetcherBase : IFetcher
    {
        #region Dependencies
        protected readonly IStore Store;
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

            Store = store;
            Handler = handler;
        }
        #endregion

        //--------------------//

        /// <inheritdoc/>
        public abstract void Fetch(IEnumerable<Implementation> implementations);

        protected void FetchImplementation(Implementation implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            #endregion

            if (!implementation.ManifestDigest.AvailableDigests.Any()) throw new NotSupportedException(string.Format(Resources.NoManifestDigest, implementation.ID));
            if (implementation.RetrievalMethods.Count == 0) throw new NotSupportedException(string.Format(Resources.NoRetrievalMethod, implementation.ID));

            // Try retrieveal methods sorted by preference until one works
            implementation.RetrievalMethods
                .OrderBy(x => x, RetrievalMethodRanker.Instance)
                .Try(retrievalMethod => Download(retrievalMethod, implementation.ManifestDigest));
        }

        private void Download(RetrievalMethod retrievalMethod, ManifestDigest manifestDigest)
        {
            try
            {
                // Treat everything as a Recipe for easier handling
                Download(
                    retrievalMethod as Recipe ?? new Recipe {Steps = {(IRecipeStep)retrievalMethod}},
                    manifestDigest);
            }
            catch (ImplementationAlreadyInStoreException)
            {}
        }

        private void Download(Recipe recipe, ManifestDigest manifestDigest)
        {
            Handler.CancellationToken.ThrowIfCancellationRequested();

            // Fail fast on unsupported archive type
            foreach (var archive in recipe.Steps.OfType<Archive>()) Extractor.VerifySupport(archive.MimeType);

            var downloadedFiles = new List<TemporaryFile>();
            try
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var downloadStep in recipe.Steps.OfType<DownloadRetrievalMethod>())
                    downloadedFiles.Add(DownloadFile(downloadStep, manifestDigest));

                // More efficient special-case handling for Archive-only handling
                if (recipe.Steps.All(step => step is Archive))
                    ApplyArchives(downloadedFiles, recipe.Steps.Cast<Archive>(), manifestDigest);
                else
                    ApplyRecipe(downloadedFiles, recipe, manifestDigest);
            }
            finally
            {
                foreach (var downloadedFile in downloadedFiles) downloadedFile.Dispose();
            }
        }

        private TemporaryFile DownloadFile(DownloadRetrievalMethod retrievalMethod, ManifestDigest manifestDigest)
        {
            var tempFile = new TemporaryFile("0install-fetcher");
            try
            {
                Handler.RunTask(new DownloadFile(retrievalMethod.Href, tempFile, retrievalMethod.DownloadSize) {Tag = manifestDigest});
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

        private void ApplyArchives(IList<TemporaryFile> files, IEnumerable<Archive> archives, ManifestDigest manifestDigest)
        {
            var archiveFileInfos = archives.Select((archive, i) => new ArchiveFileInfo
            {
                Path = (string)files[i],
                SubDir = archive.Extract,
                Destination = archive.Destination,
                MimeType = archive.MimeType,
                StartOffset = archive.StartOffset
            });

            Store.AddArchives(archiveFileInfos.ToList(), manifestDigest, Handler);
        }

        private void ApplyRecipe(IEnumerable<TemporaryFile> files, Recipe recipe, ManifestDigest manifestDigest)
        {
            using (var recipeDir = recipe.Apply(files, Handler, manifestDigest))
                Store.AddDirectory(recipeDir, manifestDigest, Handler);
        }
    }
}
