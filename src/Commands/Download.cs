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
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Selection"/>, except that it also downloads the selected versions if they are not already cached.
    /// </summary>
    [CLSCompliant(false)]
    public class Download : Selection
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "download";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionDownload; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionDownload; } }

        /// <inheritdoc/>
        public override int GuiDelay { get { return Handler.Batch ? 1000 : 0; } }

        /// <inheritdoc/>
        public Download(IBackendHandler handler) : base(handler)
        {
            Options.Add("show", () => Resources.OptionShow, _ => _show = true);
        }
        #endregion

        #region State
        /// <summary>Indicates the user wants the implementation locations on the disk.</summary>
        private bool _show;

        /// <summary><see cref="Implementation"/>s referenced in <see cref="Selection.Selections"/> that are not available in the <see cref="IStore"/>.</summary>
        protected ICollection<Implementation> UncachedImplementations;
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            Handler.ShowProgressUI();

            Solve();
            if (FeedManager.Stale || UncachedImplementations.Count != 0) RefreshSolve();
            ShowSelections();

            DownloadUncachedImplementations();

            Handler.CancellationToken.ThrowIfCancellationRequested();
            return ShowOutput();
        }

        #region Helpers
        /// <inheritdoc/>
        protected override void Solve()
        {
            base.Solve();

            try
            {
                UncachedImplementations = SelectionsManager.GetUncachedImplementations(Selections);
            }
                #region Error handling
            catch (InvalidDataException ex)
            {
                // Wrap exception to add context
                throw new SolverException(ex.Message, ex);
            }
            #endregion
        }

        /// <summary>
        /// Downloads any <see cref="Model.Implementation"/>s in <see cref="Selection"/> that are missing from <see cref="IStore"/>.
        /// </summary>
        /// <remarks>Makes sure <see cref="ISolver"/> ran with up-to-date feeds before downloading any implementations.</remarks>
        protected void DownloadUncachedImplementations()
        {
            // Do not waste time on Fetcher subsystem if nothing is missing from cache
            if (UncachedImplementations.Count == 0) return;

            try
            {
                Fetcher.Fetch(UncachedImplementations);
            }
                #region Error handling
            catch
            {
                // Suppress any left-over errors if the user canceled anyway
                Handler.CancellationToken.ThrowIfCancellationRequested();
                throw;
            }
            #endregion
        }

        private int ShowOutput()
        {
            if (_show || ShowXml) Handler.Output(Resources.SelectedImplementations, GetSelectionsOutput());
            else Handler.OutputLow(Resources.DownloadComplete, Resources.AllComponentsDownloaded);

            return 0;
        }
        #endregion
    }
}
