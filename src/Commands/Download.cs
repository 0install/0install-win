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
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Selection"/>, except that it also downloads the selected versions if they are not already cached.
    /// </summary>
    [CLSCompliant(false)]
    public class Download : Selection
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "download"; } }

        /// <inheritdoc/>
        public override string Description { get { return "This behaves similarly to '0install select', except that it also downloads the selected versions if they are not already cached. Unlike 'select', it does not print the selected versions by default. Returns an exit status of zero if it selected a suitable set of versions and they are now all downloaded and in the cache; returns a status of 1 otherwise."; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Download(IHandler handler) : base(handler)
        {
            // ToDo: Add --show
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        protected override void ExecuteHelper()
        {
            base.ExecuteHelper();

            if (Policy.Preferences.NetworkLevel == NetworkLevel.Offline) return;

            // ToDo: Output if --show is set

            Policy.Fetcher.RunSync(new FetchRequest(Selections.ListUncachedImplementations(Policy)), Handler);
        }
        #endregion
    }
}
