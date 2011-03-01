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
using Common.Collections;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Selection"/>, except that it also downloads the selected versions if they are not already cached.
    /// </summary>
    [CLSCompliant(false)]
    public class Download : Selection
    {
        #region Variables
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "download";
        
        /// <summary>Indicates the user wants the implementation locations on the disk.</summary>
        private bool _show;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionDownload; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Download(Policy policy, ISolver solver) : base(policy, solver)
        {
            Options.Add("show", Resources.OptionShow, unused => _show = true);
        }
        #endregion

        //--------------------//

        #region Helpers
        /// <summary>
        /// Downloads any <see cref="Model.Implementation"/>s in <see cref="Selection"/> that are missing from <see cref="Policy.SearchStore"/>.
        /// </summary>
        /// <remarks>Makes sure <see cref="ISolver"/> ran with up-to-date feeds before downloading any implementations.</remarks>
        protected void DownloadUncachedImplementations()
        {
            var uncachedImplementations = Selections.ListUncachedImplementations(Policy);
            if (EnumUtils.IsEmpty(uncachedImplementations)) return;

            // If feeds weren't just refreshed anyway...
            if (!Policy.FeedManager.Refresh)
            {
                // ... rerun solver in refresh mode...
                Policy.FeedManager.Refresh = true;
                Solve();

                // ... and then get an updated download list
                uncachedImplementations = Selections.ListUncachedImplementations(Policy);
            }

            Policy.Fetcher.RunSync(new FetchRequest(uncachedImplementations), Policy.Handler);
        }
        #endregion

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs, "");
            #endregion

            Solve();

            // If any of the feeds are getting old rerun solver in refresh mode
            if (StaleFeeds && Policy.Preferences.NetworkLevel != NetworkLevel.Offline)
            {
                Policy.FeedManager.Refresh = true;
                Solve();
            }

            DownloadUncachedImplementations();

            if (_show) Policy.Handler.Output(Resources.SelectedImplementations, GetSelectionsOutput());
            else Policy.Handler.Output(Resources.DownloadComplete, Resources.AllComponentsDownloaded);
            return 0;
        }
        #endregion
    }
}
