/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Net;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.CliCommands
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
        #endregion

        #region State
        private Selections _oldSelections;

        /// <inheritdoc/>
        public Update([NotNull] ICommandHandler handler) : base(handler)
        {
            //Options.Remove("o|offline");
            //Options.Remove("r|refresh");
            //Options.Remove("xml");
            //Options.Remove("show");
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            if (SelectionsDocument) throw new NotSupportedException(Resources.NoSelectionsDocumentUpdate);

            try
            {
                OldSolve();
                Log.Info("Running Refresh Solve to find updates");
                RefreshSolve();
            }
                #region Error handling
            catch (WebException ex)
            {
                if (Handler.Background)
                {
                    Log.Info("Suppressed network-related error message due to background mode");
                    Log.Info(ex);
                    return ExitCode.WebError;
                }
                else throw;
            }
            catch (SolverException ex)
            {
                if (Handler.Background)
                {
                    Log.Info("Suppressed Solver-related error message due to background mode");
                    Log.Info(ex);
                    return ExitCode.SolverError;
                }
                else throw;
            }
            #endregion

            DownloadUncachedImplementations();
            SelfUpdateCheck();

            return ShowOutput();
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
        private ExitCode ShowOutput()
        {
            var builder = new StringBuilder();
            foreach (var oldImplementation in _oldSelections.Implementations)
            {
                var interfaceUri = oldImplementation.InterfaceUri;

                var newImplementation = Selections.GetImplementation(interfaceUri);
                if (newImplementation == null)
                { // Implementation removed
                    builder.AppendLine(Resources.NoLongerUsed + interfaceUri);
                }
                else if (oldImplementation.Version != newImplementation.Version)
                { // Implementation updated
                    builder.AppendLine(interfaceUri + ": " + oldImplementation.Version + " -> " + newImplementation.Version);
                }
            }
            foreach (var newImplementation in Selections.Implementations)
            {
                var interfaceUri = newImplementation.InterfaceUri;
                if (!_oldSelections.ContainsImplementation(interfaceUri))
                { // Implementation added
                    builder.AppendLine(interfaceUri + ": new -> " + newImplementation.Version);
                }
            }

            // Detect replaced feeds
            Feed feed = FeedCache.GetFeed(Requirements.InterfaceUri);
            if (feed.ReplacedBy != null)
                builder.AppendLine(string.Format(Resources.FeedReplaced, Requirements.InterfaceUri, feed.ReplacedBy.Target));

            if (builder.Length == 0)
            {
                Handler.OutputLow(Resources.NoUpdatesFound, Resources.NoUpdatesFound);
                return ExitCode.NoChanges;
            }
            else
            {
                Handler.Output(Resources.ChangesFound, builder.ToString(0, builder.Length - Environment.NewLine.Length));
                return ExitCode.OK;
            }
        }
        #endregion
    }
}
