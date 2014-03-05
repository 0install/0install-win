/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Net;
using System.Text;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Check for updates to the program and download them if found.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Update : Download
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "update";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionUpdate; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionUpdate; } }

        /// <inheritdoc/>
        public Update(ICommandHandler handler) : base(handler)
        {
            //Options.Remove("o|offline");
            //Options.Remove("r|refresh");
            //Options.Remove("xml");
            //Options.Remove("show");
        }
        #endregion

        #region State
        private Selections _oldSelections;
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            if (SelectionsDocument) throw new NotSupportedException(Resources.NoSelectionsDocumentUpdate);

            Handler.ShowProgressUI();

            try
            {
                OldSolve();
                RefreshSolve();
            }
                #region Error handling
            catch (WebException)
            {
                if (Handler.Batch) return 1;
                else throw;
            }
            catch (SolverException)
            {
                if (Handler.Batch) return 1;
                else throw;
            }
            #endregion

            ShowSelections();

            DownloadUncachedImplementations();

            ShowOutput();
            return 0;
        }

        #region Helpers
        /// <summary>
        /// Run solver with refresh forced off to get the old values
        /// </summary>
        private void OldSolve()
        {
            FeedManager.Refresh = false;
            _oldSelections = Solver.Solve(Requirements);
        }

        /// <summary>
        /// Shows a list of changes found by the update process.
        /// </summary>
        private void ShowOutput()
        {
            string output = GetOutputMessage();
            if (string.IsNullOrEmpty(output)) Handler.OutputLow(Resources.ChangesFound, Resources.NoUpdatesFound);
            else Handler.Output(Resources.ChangesFound, output);
        }

        private string GetOutputMessage()
        {
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
                }
            }

            // Detect replaced feeds
            Feed feed = FeedCache.GetFeed(Requirements.InterfaceID);
            if (feed.ReplacedBy != null)
                builder.AppendLine(string.Format(Resources.FeedReplaced, Requirements.InterfaceID, feed.ReplacedBy.Target));

            return (builder.Length == 0)
                ? ""
                : builder.ToString(0, builder.Length - Environment.NewLine.Length);
        }
        #endregion
    }
}
