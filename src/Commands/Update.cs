/*
 * Copyright 2010-2012 Bastian Eicher
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
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Check for updates to the program and download them if found.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Update : Download
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "update";
        #endregion

        #region Variables
        private Selections _oldSelections;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionUpdate; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionUpdate; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Update(Policy policy) : base(policy)
        {
            //Options.Remove("o|offline");
            //Options.Remove("r|refresh");
            //Options.Remove("xml");
            //Options.Remove("show");
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + StringUtils.ConcatenateEscapeArgument(AdditionalArgs), "");
            if (SelectionsDocument) throw new NotSupportedException(Resources.NoSelectionsDocumentUpdate);

            Policy.Handler.ShowProgressUI();

            try
            {
                // Run solver with refresh forced off to get the old values
                var noRefreshPolicy = Policy.Clone();
                noRefreshPolicy.FeedManager.Refresh = false;
                _oldSelections = Policy.Solver.Solve(Requirements, noRefreshPolicy, out StaleFeeds);

                // Rerun solver in refresh mode to get the new values
                Policy.FeedManager.Refresh = true;
                Solve();
            }
                #region Error handling
            catch (SolverException ex)
            {
                Policy.Handler.Output(Resources.UpdateProblem, ex.Message);
                return 1;
            }
            #endregion

            SelectionsUI();

            DownloadUncachedImplementations();

            ShowUpdateOutput();
            return 0;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Shows a list of changes found by the update process.
        /// </summary>
        private void ShowUpdateOutput()
        {
            bool changes = false;

            var builder = new StringBuilder();
            foreach (var oldImplementation in _oldSelections.Implementations)
            {
                string interfaceID = oldImplementation.InterfaceID;
                try
                {
                    var newImplementation = Selections[interfaceID];
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

            foreach (var newImplementation in Selections.Implementations)
            {
                string interfaceID = newImplementation.InterfaceID;
                if (!_oldSelections.ContainsImplementation(interfaceID))
                { // Implementation added
                    builder.AppendLine(interfaceID + ": new -> " + newImplementation.Version);
                    changes = true;
                }
            }

            if (changes)
            {
                // Display the list of changes
                string message = (builder.Length == 0 ? "" : builder.ToString(0, builder.Length - Environment.NewLine.Length)); // Remove trailing line-break
                Policy.Handler.Output(Resources.ChangesFound, message);
            }
            else
            {
                // Show a "nothing changed" message (but not in batch mode, since it is too unimportant)
                if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.ChangesFound, Resources.NoUpdatesFound);
            }
        }
        #endregion
    }
}
