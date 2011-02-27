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
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "update";

        private Selections _newSelections;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionUpdate; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Update(IHandler handler, Policy policy, ISolver solver) : base(handler, policy, solver)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        protected override void ExecuteHelper()
        {
            // Run solver in normal mode to get the old values
            base.ExecuteHelper();

            // Rerun solver in refresh mode to get the new values
            if (!PreSelected)
            {
                Policy.FeedManager.Refresh = true;
                _newSelections = Solver.Solve(Requirements, Policy, Handler, out StaleFeeds);
            }
        }

        /// <inheritdoc/>
        public override int Execute()
        {
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs, "");
            ExecuteHelper();

            Handler.Output(Resources.ChangesFound, GetUpdateInformation());
            Policy.Fetcher.RunSync(new FetchRequest(_newSelections.ListUncachedImplementations(Policy)), Handler);
            return 0;
        }

        /// <summary>
        /// Returns a list of changes found by the update process.
        /// </summary>
        private string GetUpdateInformation()
        {
            bool changes = false;

            var builder = new StringBuilder();
            foreach (var oldImplementation in Selections.Implementations)
            {
                string interfaceID = oldImplementation.InterfaceID;
                try
                {
                    var newImplementation = _newSelections.GetImplementation(interfaceID);
                    if (oldImplementation.Version != newImplementation.Version)
                    { // Implementation updated
                        builder.AppendLine(interfaceID + ": " + oldImplementation.Version + " -> " + newImplementation.Version);
                        changes = true;
                    }
                }
                catch (KeyNotFoundException)
                { // Implementation removed
                    builder.AppendLine(Resources.NoLongerUsed + interfaceID);
                }
            }

            foreach (var newImplementation in _newSelections.Implementations)
            {
                string interfaceID = newImplementation.InterfaceID;
                if (!Selections.ContainsImplementation(interfaceID))
                { // Implementation added
                    builder.AppendLine(interfaceID + ": new -> " + newImplementation.Version);
                    changes = true;
                }
            }
            if (!changes) builder.AppendLine(Resources.NoUpdatesFound);

            return (builder.Length == 0 ? "" : builder.ToString(0, builder.Length - Environment.NewLine.Length)); // Remove trailing line-break
        }
        #endregion
    }
}
