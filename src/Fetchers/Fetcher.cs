/*
 * Copyright 2010 Bastian Eicher, Roland Leopold Walkling
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
using System.Net;
using Common.Net;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Fetchers.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;
using System.Diagnostics;

namespace ZeroInstall.Fetchers
{
    internal class RetrievalMethodRanker : IComparer<RetrievalMethod>
    {
        #region Declaration of Priorities
        private enum Category
        {
            Null,
            Format,
            Simplicity
        }

        private enum Valuation
        {
            Recipe = 1 * Category.Simplicity,
            Archive = 0 * Category.Simplicity,
            ZipFormat = 0 * Category.Format,
            GzFormat = 1 * Category.Format,
            UnknownFormat = 2 * Category.Format
        }
        #endregion

        private abstract class Ranking : IComparable<Ranking>
        {
            protected abstract int Value { get; }

            public int CompareTo(Ranking other)
            {
                #region Sanity checks
                if (other == null) throw new ArgumentNullException("other");
                #endregion

                return Value - other.Value;
            }
        }

        #region Archive Ranking
        private class ArchiveRanking : Ranking
        {
            private readonly Archive _subject;

            public ArchiveRanking(Archive subject)
            {
                _subject = subject;
            }

            protected override int Value
            {
                get
                {
                    int result = 0;
                    result += (int)Valuation.Archive;

                    if (_subject.MimeType == "application/zip")
                        result += (int)Valuation.ZipFormat;
                    else
                        result += (int)Valuation.UnknownFormat;

                    return result;
                }
            }
        }
        #endregion

        #region Recipe Ranking
        private class RecipeRanking : Ranking
        {
            public readonly Recipe Subject;

            public RecipeRanking(Recipe subject)
            {
                Subject = subject;
            }

            protected override int Value
            {
                get
                {
                    int result = 0;
                    result += (int)Valuation.Archive;

                    return result;
                }
            }
        }
        #endregion

        #region Dispatching Creation of Ranking objects
        private static Ranking Rank(RetrievalMethod subject)
        {
            #region Sanity checks
            if (subject == null) throw new ArgumentNullException("subject");
            #endregion

            Ranking result = null;

            if (subject.GetType() == typeof(Archive))
            {
                result = new ArchiveRanking((Archive)subject);
            }
            else if (subject.GetType() == typeof(Recipe))
            {
                result = new RecipeRanking((Recipe)subject);
            }
            else
            {
                Debug.Fail("subject (RetrievalMethod) has unknown type");
            }
            return result;
        }
        #endregion

        public int Compare(RetrievalMethod x, RetrievalMethod y)
        {
            var rankingX = Rank(x);
            var rankingY = Rank(y);
            return rankingX.CompareTo(rankingY);
        }
    }

    /// <summary>
    /// A Method Object that encapsulates fetching a single <see cref="Model.Implementation"/>
    /// </summary>
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
            TryRetrievalMethods(handler);
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
            // ToDo: Catch more exceptions
            catch (WebException ex)
            {
                Problems.Add(ex);
            }
        }

        private void SetCompletedIfThereWereNoProblems()
        {
            if (Problems.Count == 0)
            {
                Completed = true;
            }
        }

        private void PerformRetrievalMethodDispatchingOnType(RetrievalMethod method, ITaskHandler handler)
        {
            var archive = method as Archive;
            var recipe = method as Recipe;
            if (archive != null)
            {
                PerformArchiveStep(archive, handler);
            }
            else if (recipe != null)
            {
                if (recipe.ContainsUnknownSteps) throw new FetcherException("Recipe contains unknown steps.");
                PerformRecipe(recipe, handler);
            }
        }

        private void PerformArchiveStep(Archive archive, ITaskHandler handler)
        {
            var tempArchiveInfo = DownloadAndPrepareArchive(archive, handler);

            try
            {
                _fetcherInstance.Store.AddArchive(tempArchiveInfo, _digest, handler);
            }
            catch (ImplementationAlreadyInStoreException) {}
            finally { File.Delete(tempArchiveInfo.Path); }
        }

        private ArchiveFileInfo DownloadAndPrepareArchive(Archive archive, ITaskHandler handler)
        {
            string tempArchive = FileUtils.GetTempFile("0install-fetcher");

            DownloadArchive(archive, tempArchive, handler);

            return new ArchiveFileInfo
            {
                Path = tempArchive,
                MimeType = archive.MimeType,
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
                _fetcherInstance.Store.AddMultipleArchives(archives, _digest, handler);
            }
            catch (ImplementationAlreadyInStoreException) {}
            finally { foreach (var archive in archives) File.Delete(archive.Path); }
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
                handler.RunTask(downloadFile, _digest);
            }
            catch
            {
                File.Delete(destination);
                throw;
            }
        }
    }

    /// <summary>
    /// Downloads <see cref="Implementation"/>s, extracts them and adds them to the <see cref="Store"/>.
    /// </summary>
    public class Fetcher : MarshalByRefObject, IFetcher
    {
        #region Properties
        /// <inheritdoc/>
        public IStore Store { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download fetcher.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Model.Implementation"/>s in.</param>
        public Fetcher(IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            Store = store;
        }
        #endregion
        
        //--------------------//

        protected virtual ImplementationFetch CreateFetch(Implementation implementation)
        {
            return new ImplementationFetch(this, implementation);
        }

        /// <inheritdoc/
        public void Run(FetchRequest fetchRequest)
        {
            #region Sanity checks
            if (fetchRequest == null) throw new ArgumentNullException("fetchRequest");
            #endregion

            foreach (var implementation in fetchRequest.Implementations)
            {
                var fetchProcess = CreateFetch(implementation);
                fetchProcess.Execute(fetchRequest.Handler);
                if (!fetchProcess.Completed) throw fetchProcess.Problems.Last;
            }
        }
    }
}
