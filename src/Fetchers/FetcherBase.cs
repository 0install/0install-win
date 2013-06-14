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
using System.Linq;
using Common.Collections;
using Common.Storage;
using Common.Tasks;
using ZeroInstall.Fetchers.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// An abstract base class for <see cref="IFetcher"/> implementations.
    /// </summary>
    public abstract class FetcherBase : MarshalByRefObject, IFetcher
    {
        #region Dependencies
        protected readonly IStore Store;
        protected readonly ITaskHandler Handler;

        /// <summary>
        /// Creates a new download fetcher.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Model.Implementation"/>s in.</param>
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

            implementation.RetrievalMethods.OrderBy(x => x, RetrievalMethodRanker.Instance).
                           Try(retrievalMethod => ApplyRetrievalMethod(retrievalMethod, implementation.ManifestDigest));
        }

        private void ApplyRetrievalMethod(RetrievalMethod retrievalMethod, ManifestDigest manifestDigest)
        {
            Handler.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                new PerTypeDispatcher<RetrievalMethod>(false)
                {
                    (Archive archive) => ApplyArchive(archive, manifestDigest),
                    (SingleFile singleFile) => ApplySingleFile(singleFile, manifestDigest),
                    (Recipe recipe) => ApplyRecipe(recipe, manifestDigest)
                }.Dispatch(retrievalMethod);
            }
            catch (ImplementationAlreadyInStoreException)
            {}
        }

        private void ApplyArchive(Archive archive, ManifestDigest manifestDigest)
        {
            // Fail fast on unsupported archive type
            Extractor.VerifySupport(archive.MimeType);

            using (var downloadedFile = DownloadFile(archive, manifestDigest))
                AddArchives(new[] {downloadedFile}, new[] {archive}, manifestDigest);
        }

        private void ApplySingleFile(SingleFile singleFile, ManifestDigest manifestDigest)
        {
            using (var tempDir = new TemporaryDirectory("0install-fetcher"))
            {
                using (var downloadedFile = DownloadFile(singleFile, manifestDigest))
                    RecipeUtils.ApplySingleFile(singleFile, downloadedFile, tempDir);

                Store.AddDirectory(tempDir, manifestDigest, Handler);
            }
        }

        private void ApplyRecipe(Recipe recipe, ManifestDigest manifestDigest)
        {
            // Fail fast on unsupported archive type
            foreach (var archive in recipe.Steps.OfType<Archive>()) Extractor.VerifySupport(archive.MimeType);

            var downloadedFiles = new List<TemporaryFile>();
            try
            {
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (var downloadStep in recipe.Steps.OfType<DownloadRetrievalMethod>())
                    downloadedFiles.Add(DownloadFile(downloadStep, manifestDigest));
                // ReSharper restore LoopCanBeConvertedToQuery

                if (recipe.Steps.All(step => step is Archive))
                { // Optimized special case for archive-only recipes
                    AddArchives(downloadedFiles, recipe.Steps.Cast<Archive>(), manifestDigest);
                }
                else
                {
                    using (var recipeDir = RecipeUtils.ApplyRecipe(recipe, downloadedFiles, Handler, manifestDigest))
                        Store.AddDirectory(recipeDir, manifestDigest, Handler);
                }
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
                Handler.RunTask(new DownloadFile(retrievalMethod.Location, tempFile, retrievalMethod.DownloadSize), manifestDigest);
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

        private void AddArchives(IList<TemporaryFile> files, IEnumerable<Archive> archives, ManifestDigest manifestDigest)
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
    }
}
