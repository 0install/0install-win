/*
 * Copyright 2010-2011 Bastian Eicher, Roland Leopold Walkling
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
using Common.Net;
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
        private ITask _currentTask;

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
            TryRetrievalMethods(handler);
        }

        public void Cancel()
        {
            // ToDo: Handle canceling when task hasn't been started yet
            if (_currentTask != null && _currentTask.CanCancel) _currentTask.Cancel();
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
                PerformRetrievalMethodDispatchingOnType(method, handler);
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

        private void PerformRetrievalMethodDispatchingOnType(RetrievalMethod method, ITaskHandler handler)
        {
            var archive = method as Archive;
            var recipe = method as Recipe;
            if (archive != null)
                PerformArchiveStep(archive, handler);
            else if (recipe != null)
            {
                if (recipe.ContainsUnknownSteps) throw new NotSupportedException("Recipe contains unknown steps.");
                PerformRecipe(recipe, handler);
            }
        }

        private void PerformArchiveStep(Archive archive, ITaskHandler handler)
        {
            var tempArchiveInfo = DownloadAndPrepareArchive(archive, handler);

            try
            {
                _fetcherInstance.Store.AddArchive(tempArchiveInfo, _digest, new HookTaskHandler(handler, task => _currentTask = task));
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
            var archives = new List<ArchiveFileInfo>();
            foreach (var currentStep in recipe.Steps)
            {
                var currentArchive = currentStep as Archive;
                if (currentArchive == null) throw new InvalidOperationException(Resources.UnknownRecipeStepType);

                archives.Add(DownloadAndPrepareArchive(currentArchive, handler));
            }
            try
            {
                _fetcherInstance.Store.AddMultipleArchives(archives, _digest, new HookTaskHandler(handler, task => _currentTask = task));
            }
            catch (ImplementationAlreadyInStoreException)
            {}
            finally
            {
                foreach (var archive in archives) File.Delete(archive.Path);
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
                // Defer task to handler
                _currentTask = downloadFile;
                handler.RunTask(downloadFile, _digest);
            }
            catch
            {
                File.Delete(destination);
                throw;
            }
        }
    }
}
