/*
 * Copyright 2010-2012 Bastian Eicher, Roland Leopold Walkling
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
using System.IO;
using System.Net;
using System.Threading;
using Common;
using Common.Collections;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Fetchers.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// A Method Object that encapsulates fetching a single <see cref="Model.Implementation"/>
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class ImplementationFetch
    {
        private readonly Fetcher _fetcherInstance;
        private readonly List<RetrievalMethod> _retrievalMethods;
        private readonly ManifestDigest _digest;
        internal bool Completed;
        internal readonly C5.IList<Exception> Problems = new C5.LinkedList<Exception>();

        public ImplementationFetch(Fetcher fetcher, Implementation implementation)
        {
            #region Sanity checks
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            if (implementation == null) throw new ArgumentNullException("implementation");
            #endregion

            _fetcherInstance = fetcher;
            _retrievalMethods = new List<RetrievalMethod>(implementation.RetrievalMethods.Count);
            _retrievalMethods.AddRange(implementation.RetrievalMethods);
            _retrievalMethods.Sort(new RetrievalMethodRanker());
            _digest = implementation.ManifestDigest;
        }

        public void Execute(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            using (var mutex = new Mutex(false, "0install-fetcher-" + _digest.BestDigest))
            {
                // Wait for the mutex and allow cancellation every 100 ms
                try
                {
                    while (!mutex.WaitOne(100, false))
                        handler.RunTask(new WaitTask(Resources.DownloadInAnotherWindow, mutex), _digest);
                }
                    #region Error handling
                catch (AbandonedMutexException ex)
                {
                    // Abandoned mutexes also get owned, but indicate something may have gone wrong elsewhere
                    Log.Warn(ex.Message);
                }
                #endregion

                try
                {
                    // Check if another process added the implementation in the meantime
                    _fetcherInstance.Store.ClearCaches();
                    if (_fetcherInstance.Store.Contains(_digest))
                    {
                        Completed = true;
                        return;
                    }

                    TryRetrievalMethods(handler);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        private void TryRetrievalMethods(ITaskHandler handler)
        {
            foreach (var method in _retrievalMethods)
            {
                Problems.Clear();
                TryOneRetrievalMethodAndNoteProblems(method, handler);
                SetCompletedIfThereWereNoProblems();
                if (Completed) break;
            }
        }

        private void TryOneRetrievalMethodAndNoteProblems(RetrievalMethod method, ITaskHandler handler)
        {
            try
            {
                new PerTypeDispatcher<RetrievalMethod>(false)
                {
                    (Archive archive) => PerformArchiveStep(archive, handler),
                    (Recipe recipe) =>
                    {
                        if (recipe.ContainsUnknownSteps) throw new NotSupportedException("Recipe contains unknown steps.");
                        PerformRecipe(recipe, handler);
                    },
                }.Dispatch(method);
            }
            catch (WebException ex)
            {
                Problems.Add(ex);
            }
        }

        private void SetCompletedIfThereWereNoProblems()
        {
            if (Problems.Count == 0) Completed = true;
        }

        private void PerformArchiveStep(Archive archive, ITaskHandler handler)
        {
            var tempArchiveInfo = DownloadAndPrepareArchive(archive, handler);

            try
            {
                _fetcherInstance.Store.AddArchives(new[] {tempArchiveInfo}, _digest, handler);
            }
            catch (ImplementationAlreadyInStoreException)
            {}
            finally
            {
                File.Delete(tempArchiveInfo.Path);
            }
        }

        private ArchiveFileInfo DownloadAndPrepareArchive(Archive archive, ITaskHandler handler)
        {
            string tempArchive = FileUtils.GetTempFile("0install-fetcher");

            DownloadArchive(archive, tempArchive, handler);

            return new ArchiveFileInfo
            {
                Path = tempArchive,
                MimeType = archive.MimeType.ToLowerInvariant(),
                SubDir = archive.Extract,
                StartOffset = archive.StartOffset
            };
        }

        private void PerformRecipe(Recipe recipe, ITaskHandler handler)
        {
            bool complexRecipe = false;
            var downloadedArchives = new List<ArchiveFileInfo>();
            foreach (var step in recipe.Steps)
            {
                var archive = step as Archive;
                if (archive == null) complexRecipe = true;
                else downloadedArchives.Add(DownloadAndPrepareArchive(archive, handler));
            }

            try
            {
                if (complexRecipe)
                { // Complex recipes require a temporary directory to build the final implementation before adding it to the store
                    using (var recipeDir = RecipeUtils.ApplyRecipe(recipe, downloadedArchives, handler, _digest))
                        _fetcherInstance.Store.AddDirectory(recipeDir.Path, _digest, handler);
                }
                else _fetcherInstance.Store.AddArchives(downloadedArchives, _digest, handler);
            }
            catch (ImplementationAlreadyInStoreException)
            {}
            finally
            {
                foreach (var archive in downloadedArchives) File.Delete(archive.Path);
            }
        }

        protected virtual void DownloadArchive(Archive archive, string destination, ITaskHandler handler)
        {
            #region Sanity checks
            if (archive == null) throw new ArgumentNullException("archive");
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var downloadFile = new DownloadFile(archive.Location, destination, archive.Size + archive.StartOffset);

            try
            {
                handler.RunTask(downloadFile, _digest); // Defer task to handler
            }
            catch
            {
                File.Delete(destination);
                throw;
            }
        }
    }
}
