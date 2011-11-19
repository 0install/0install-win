/*
 * Copyright 2010-2011 Bastian Eicher
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
using Common;
using Common.Collections;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Selection"/>, except that it also downloads the selected versions if they are not already cached.
    /// </summary>
    [CLSCompliant(false)]
    public class Download : Selection
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "download";
        #endregion

        #region Variables
        /// <summary>Indicates the user wants the implementation locations on the disk.</summary>
        private bool _show;

        /// <summary><see cref="Implementation"/>s referenced in <see cref="Selection.Selections"/> that are not available in the <see cref="Fetcher.Store"/>.</summary>
        protected IEnumerable<Implementation> UncachedImplementations;

        /// <summary>Synchronization handle to prevent race conditions with <see cref="IFetcher"/> canceling.</summary>
        private readonly object _fetcherCancelLock = new object();

        /// <summary>The <see cref="FetchRequest"/> currently being executed.</summary>
        private FetchRequest _currentFetchRequest;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionDownload; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionDownload; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Download(Policy policy) : base(policy)
        {
            Options.Add("show", Resources.OptionShow, unused => _show = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs, "");
            #endregion

            Policy.Handler.ShowProgressUI(Cancel);

            Solve();

            // If any of the feeds are getting old or any implementations need to be downloaded rerun solver in refresh mode (unless it was already in that mode to begin with)
            if ((StaleFeeds || !EnumerableUtils.IsEmpty(UncachedImplementations)) && !Policy.FeedManager.Refresh && Policy.Config.NetworkUse != NetworkLevel.Offline)
            {
                Policy.FeedManager.Refresh = true;
                Solve();
            }
            SelectionsUI();

            DownloadUncachedImplementations();

            if (_show) Policy.Handler.Output(Resources.SelectedImplementations, GetSelectionsOutput());
            else
            {
                // Show a "download complete" message (but not in batch mode, since it is too unimportant)
                if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.DownloadComplete, Resources.AllComponentsDownloaded);
            }
            return 0;
        }
        #endregion

        #region Helpers
        /// <inheritdoc/>
        protected override Selections Solve()
        {
            var result = base.Solve();

            try
            {
                UncachedImplementations = Selections.ListUncachedImplementations(Policy.Fetcher.Store, Policy.FeedManager.Cache);
            }
                #region Error handling
            catch (InvalidDataException ex)
            {
                // Wrap exception to add context
                throw new SolverException(ex.Message, ex);
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Downloads any <see cref="Model.Implementation"/>s in <see cref="Selection"/> that are missing from <see cref="Fetcher.Store"/>.
        /// </summary>
        /// <remarks>Makes sure <see cref="ISolver"/> ran with up-to-date feeds before downloading any implementations.</remarks>
        protected void DownloadUncachedImplementations()
        {
            // Make sure cancellation doesn't fall within a blind spot between check and Fetcher start
            lock (_fetcherCancelLock)
            {
                if (Canceled) throw new UserCancelException();
                _currentFetchRequest = new FetchRequest(UncachedImplementations, Policy.Handler);
                Policy.Fetcher.Start(_currentFetchRequest);
            }

            try
            {
                Policy.Fetcher.Join(_currentFetchRequest);
            }
            catch
            {
                // Suppress any left-over errors if the user canceled anyway
                if (Canceled) throw new UserCancelException();
                throw;
            }
            lock (_fetcherCancelLock)
                _currentFetchRequest = null;

            if (Canceled) throw new UserCancelException(); // ToDo: Remove once fetcher has a cancelable synchronous run method
        }
        #endregion

        #region Cancel
        /// <inheritdoc/>
        public override void Cancel()
        {
            base.Cancel();

            lock (_fetcherCancelLock)
            {
                if (_currentFetchRequest != null) Policy.Fetcher.Cancel(_currentFetchRequest);
            }
        }
        #endregion
    }
}
