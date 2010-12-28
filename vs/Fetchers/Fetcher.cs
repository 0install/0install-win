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
using Common;
using Common.Net;
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
            private readonly Recipe _subject;

            public RecipeRanking(Recipe subject)
            {
                _subject = subject;
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
    /// A Method Object that encapsulates fetching a single <see cref="Implementation"/>
    /// </summary>
    public class ImplementationFetch
    {
        private Fetcher FetcherInstance;
        private readonly List<RetrievalMethod> RetrievalMethods;
        private ManifestDigest Digest;
        internal bool Completed = false;
        internal List<Exception> Problems = new List<Exception>();

        public ImplementationFetch(Fetcher fetcher, Implementation implementation)
        {
            FetcherInstance = fetcher;
            RetrievalMethods = new List<RetrievalMethod>(implementation.RetrievalMethods.Count);
            RetrievalMethods.AddRange(implementation.RetrievalMethods);
            RetrievalMethods.Sort(new RetrievalMethodRanker());
            Digest = implementation.ManifestDigest;
        }

        public void Execute()
        {
            TryRetrievalMethods();
        }

        private void TryRetrievalMethods()
        {
            foreach (var method in RetrievalMethods)
            {
                Problems.Clear();
                TryOneRetrievalMethodAndNoteProblems(method);
                SetCompletedIfThereWereNoProblems();
                if (Completed) break;
            }
        }

        private void TryOneRetrievalMethodAndNoteProblems(RetrievalMethod method)
        {
            try
            {
                PerformRetrievalMethodDispatchingOnType(method);
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

        private void PerformRetrievalMethodDispatchingOnType(RetrievalMethod method)
        {
            var archive = method as Archive;
            var recipe = method as Recipe;
            if (archive != null)
            {
                PerformArchiveStep(archive);
            }
            else if (recipe != null)
            {
                PerformRecipe(recipe);
            }
        }

        private void PerformArchiveStep(Archive archive)
        {
            var tempArchiveInfo = DownloadAndPrepareArchive(archive);

            try
            {
                FetcherInstance.Store.AddArchive(tempArchiveInfo,
                    Digest,
                    FetcherInstance.Handler);
            }
            catch (ImplementationAlreadyInStoreException) {}
            finally { File.Delete(tempArchiveInfo.Path); }
        }

        private ArchiveFileInfo DownloadAndPrepareArchive(Archive archive)
        {
            string tempArchive = FileUtils.GetTempFile("0install-fetcher");

            DownloadArchive(archive, tempArchive);

            return new ArchiveFileInfo
            {
                Path = tempArchive,
                MimeType = archive.MimeType,
                SubDir = archive.Extract,
                StartOffset = archive.StartOffset
            };
        }
        private void PerformRecipe(Recipe recipe)
        {
            var archives = new List<ArchiveFileInfo>();
            foreach (var currentStep in recipe.Steps)
            {
                var currentArchive = currentStep as Archive;
                if (currentArchive == null) throw new InvalidOperationException(Resources.UnknownRecipeStepType);

                archives.Add(DownloadAndPrepareArchive(currentArchive));
            }
            try
            {
                FetcherInstance.Store.AddMultipleArchives(archives, Digest, FetcherInstance.Handler);
            }
            catch (ImplementationAlreadyInStoreException) {}
            finally { foreach (var archive in archives) File.Delete(archive.Path); }
        }

        protected virtual void DownloadArchive(Archive archive, string destination)
        {
            #region Sanity checks
            if (archive == null) throw new ArgumentNullException("archive");
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            #endregion

            if (archive.StartOffset != 0)
            {
                // This is the only way to tell DownloadFile not to download a
                // certain part of the remote archive.
                // TODO: Make this behavior explicit
                PadIgnoredPartOfFile(archive, destination);
            }

            var downloadFile = new DownloadFile(archive.Location, destination, archive.Size + archive.StartOffset);

            try
            {
                // Run task locally or defer to handler
                if (FetcherInstance.Handler == null) downloadFile.RunSync();
                else FetcherInstance.Handler.RunDownloadTask(downloadFile);

                RejectLocalFileOfWrongSize(archive, destination);
            }
            catch (FetcherException)
            {
                File.Delete(destination);
                throw;
            }
        }

        private static void PadIgnoredPartOfFile(Archive archive, string destination)
        {
            #region Sanity checks
            if (archive == null) throw new ArgumentNullException("archive");
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            #endregion

            using (var tempArchiveStream = File.Create(destination))
            {
                tempArchiveStream.Seek(archive.StartOffset - 1, SeekOrigin.Begin);
                tempArchiveStream.WriteByte(0);
            }
        }

        /// <summary>
        /// Checks if the size of <paramref name="localFile"/> matches the <paramref name="archive"/>'s size and offset, otherwise throws a <see cref="FetcherException"/>.
        /// </summary>
        /// <exception cref="FetcherException">Thrown if the file has different size than stated in <paramref name="archive"/>.
        /// </exception>
        private static void RejectLocalFileOfWrongSize(Archive archive, string localFile)
        {
            #region Sanity checks
            if (archive == null) throw new ArgumentNullException("archive");
            if (string.IsNullOrEmpty(localFile)) throw new ArgumentNullException("localFile");
            #endregion

            if (new FileInfo(localFile).Length != archive.Size + archive.StartOffset)
                throw new FetcherException(string.Format(Resources.FileNotExpectedSize, archive.Location, archive.Size + archive.StartOffset, new FileInfo(localFile).Length));
        }
    }

    /// <summary>
    /// Manages one or more <see cref="FetchRequest"/>s and keeps clients informed of the progress. Files are downloaded and added to <see cref="Store"/> automatically.
    /// </summary>
    public class Fetcher : MarshalByRefObject
    {
        #region Properties
        /// <summary>
        /// The location to store the downloaded and unpacked <see cref="Implementation"/>s in.
        /// </summary>
        public IStore Store { get; private set; }

        /// <summary>
        /// A callback object used when the the user is to be informed about progress.
        /// </summary>
        public IFetchHandler Handler { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download fetcher with a custom target <see cref="IStore"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress; may be <see langword="null"/>.</param>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Implementation"/>s in.</param>
        public Fetcher(IFetchHandler handler, IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            Handler = handler;
            Store = store;
        }

        /// <summary>
        /// Creates a new download fetcher with the default <see cref="IStore"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress; may be <see langword="null"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public Fetcher(IFetchHandler handler) : this(handler, StoreProvider.Default)
        {}
        #endregion
        
        //--------------------//

        protected virtual ImplementationFetch CreateFetch(Implementation implementation)
        {
            return new ImplementationFetch(this, implementation);
        }

        /// <summary>
        /// Execute a complete request and block until it is done.
        /// </summary>
        /// <exception cref="UserCancelException">Thrown if a download, extraction or manifest task was cancelled from another thread.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to <see cref="Store"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown an <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="FetcherException"></exception>
        public void RunSync(FetchRequest fetchRequest)
        {
            #region Sanity checks
            if (fetchRequest == null) throw new ArgumentNullException("fetchRequest");
            #endregion

            foreach (var implementation in fetchRequest.Implementations)
            {
                var fetchProcess = CreateFetch(implementation);
                fetchProcess.Execute();
                if (!fetchProcess.Completed) throw new FetcherException("Request not completely fulfilled", fetchProcess.Problems);
            }
        }
    }
}
