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
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.FrontendCommands
{
    /// <summary>
    /// Updates all applications in the <see cref="AppList"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class UpdateApps : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "update-all";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "update-apps";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionUpdateApps; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 0; } }
        #endregion

        #region State
        private bool _clean;

        /// <inheritdoc/>
        public UpdateApps([NotNull] ICommandHandler handler) : base(handler)
        {
            Options.Add("c|clean", () => Resources.OptionClean, _ => _clean = true);
        }
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            var selectedImplementations = SolveAll(GetTargets()).ToList();
            DownloadUncachedImplementations(selectedImplementations);
            SelfUpdateCheck();

            Handler.CancellationToken.ThrowIfCancellationRequested();
            if (_clean) Clean(selectedImplementations.Select(impl => impl.ManifestDigest));

            return 0;
        }

        #region Helpers
        /// <summary>
        /// Returns a list of <see cref="Requirements"/> that describe the applications to be updated.
        /// </summary>
        private IEnumerable<Requirements> GetTargets()
        {
            // Target every application in the AppList...
            return from entry in AppList.Entries
                // ... unless excluded from auto-update
                where entry.AutoUpdate
                // ... or excluded by a hostname filter
                where entry.Hostname == null || Regex.IsMatch(Environment.MachineName, entry.Hostname)
                // Use custom app restrictions if any
                select entry.Requirements ?? new Requirements(entry.InterfaceUri);
        }

        private IEnumerable<ImplementationSelection> SolveAll(IEnumerable<Requirements> targets)
        {
            FeedManager.Refresh = true;

            // Run solver for each app
            var implementations = new List<ImplementationSelection>();
            foreach (var requirements in targets)
                implementations.AddRange(Solver.Solve(requirements).Implementations);

            // Deduplicate selections
            return implementations.Distinct(ManifestDigestPartialEqualityComparer<ImplementationSelection>.Instance);
        }

        private void DownloadUncachedImplementations(IEnumerable<ImplementationSelection> selectedImplementations)
        {
            var selections = new Selections(selectedImplementations);
            var uncachedImplementations = SelectionsManager.GetUncachedSelections(selections).ToList();

            // Do not waste time on Fetcher subsystem if nothing is missing from cache
            if (uncachedImplementations.Count == 0) return;

            // Only show implementations in the UI that need to be downloaded
            Handler.ShowSelections(new Selections(uncachedImplementations), FeedCache);

            try
            {
                var toDownload = SelectionsManager.GetImplementations(uncachedImplementations);
                Fetcher.Fetch(toDownload);
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

        private void Clean(IEnumerable<ManifestDigest> digestsToKeep)
        {
            var toDelete = Store.ListAll().Except(digestsToKeep, ManifestDigestPartialEqualityComparer.Instance).ToList();
            Handler.RunTask(new ForEachTask<ManifestDigest>(Resources.RemovingOutdated, toDelete, x => Store.Remove(x)));
        }
        #endregion
    }
}
