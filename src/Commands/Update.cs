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
using System.Text;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Check for updates to the program and download them if found.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Update : Selection
    {
        #region Variables
        private Selections _oldSelections;
        #endregion

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
            // Run solver in offline mode to get the old values
            var offlinePolicy = Policy.CreateDefault();
            offlinePolicy.AdditionalStore = Policy.AdditionalStore;
            offlinePolicy.Preferences.NetworkLevel = NetworkLevel.Offline;
            _oldSelections = SolverProvider.Default.Solve(Requirements, offlinePolicy, Handler);

            // Run solver in online refresh mode to get the new values
            Policy.FeedManager.Refresh = true;
            base.ExecuteHelper();
        }

        /// <inheritdoc/>
        public override int Execute()
        {
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments, Name);
            ExecuteHelper();

            Handler.Inform(Resources.ChangesFound, GetUpdateInformation());

            Policy.Fetcher.RunSync(new FetchRequest(Selections.ListUncachedImplementations(Policy)), Handler);
            return 0;
        }

        /// <summary>
        /// Returns a list of changes found by the update process.
        /// </summary>
        private string GetUpdateInformation()
        {
            bool changes = false;

            var builder = new StringBuilder();
            // ToDo: Output information about changes
            foreach (var oldImplementation in _oldSelections.Implementations)
            {
                string interfaceID = oldImplementation.InterfaceID;
                try
                {
                    var newImplementation = Selections.GetImplementation(interfaceID);
                    if (oldImplementation.Version != newImplementation.Version)
                    { // Implementation updated
                        builder.AppendLine(interfaceID + ": " + oldImplementation.Version + " -> " + newImplementation.Version);
                        changes = true;
                    }
                }
                catch(KeyNotFoundException)
                { // Implementation removed
                    builder.AppendLine(Resources.NoLongerUsed + interfaceID);
                }
            }

            foreach (var newImplementation in Selections.Implementations)
            {
                string interfaceID = newImplementation.InterfaceID;
                if (!_oldSelections.ContainsImplementation(interfaceID))
                { // Implementation added
                    builder.AppendLine(interfaceID + ": new -> " + newImplementation.Version);
                    changes = true;
                }
            }
            if (!changes) builder.AppendLine(Resources.NoUpdatesFound);

            return (builder.Length == 0 ? "" : builder.ToString(0, builder.Length - 1)); // Remove trailing line-break
        }
        #endregion
    }
}
