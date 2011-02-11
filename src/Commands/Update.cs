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
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Check for updates to the program and download them if found.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Update : Selection
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "update"; } }

        /// <inheritdoc/>
        public override string Description { get { return Resources.DescriptionUpdate; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Update(IHandler handler) : base(handler)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        protected override void ExecuteHelper()
        {
            if (Policy.Preferences.NetworkLevel == NetworkLevel.Offline) return;
            Policy.FeedManager.Refresh = true;

            // ToDo: Store information about changes

            Policy.Fetcher.RunSync(new FetchRequest(Selections.ListUncachedImplementations(Policy)), Handler);

            base.ExecuteHelper();
        }

        /// <inheritdoc/>
        public override int Execute()
        {
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments, Name);
            ExecuteHelper();

            // ToDo: Output information about changes
            return 0;
        }
        #endregion
    }
}
