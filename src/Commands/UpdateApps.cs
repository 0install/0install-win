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
using System.Linq;
using System.Text.RegularExpressions;
using Common;
using Common.Tasks;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Updates all applications in the application list.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class UpdateApps : IntegrationCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "update-apps";
        #endregion

        #region Variables
        private bool _clean;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionUpdateApps; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionUpdateApps; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public UpdateApps(Policy policy) : base(policy)
        {
            Options.Add("c|clean", Resources.OptionClean, unused => _clean = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count > 0) throw new OptionException(Resources.TooManyArguments, "");

            if (MachineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException();

            Policy.Handler.ShowProgressUI();
            var selectedImplementations = SolveAll(GetTargets()).ToList();

            DownloadUncachedImplementations(selectedImplementations);

            Policy.Handler.CancellationToken.ThrowIfCancellationRequested();
            if (_clean) Clean(selectedImplementations.Select(impl => impl.ManifestDigest));

            return 0;
        }
        #endregion

        #region Helpers
        private IEnumerable<Requirements> GetTargets()
        {
            // Target every application in the AppList...
            return from entry in AppList.Entries
                // ... unless excluded from auto-update
                where entry.AutoUpdate
                // ... or excluded by a hostname filter
                where entry.Hostname == null || Regex.IsMatch(Environment.MachineName, entry.Hostname)
                // Use custom app restrictions if any
                select entry.Requirements ?? new Requirements {InterfaceID = entry.InterfaceID};
        }

        private IEnumerable<ImplementationSelection> SolveAll(IEnumerable<Requirements> targets)
        {
            Policy.FeedManager.Refresh = true;

            // Run solver for each app
            var implementations = new List<ImplementationSelection>();
            Policy.Handler.RunTask(new ForEachTask<Requirements>(Resources.CheckingForUpdates, targets.ToList(), requirements =>
            {
                bool staleFeeds;
                implementations.AddRange(Policy.Solver.Solve(requirements, Policy, out staleFeeds).Implementations);
            }), null);

            // Deduplicate selections
            return implementations.Distinct(new ManifestDigestPartialEqualityComparer<ImplementationSelection>());
        }

        private void DownloadUncachedImplementations(IEnumerable<ImplementationSelection> selectedImplementations)
        {
            var selections = new Selections(selectedImplementations);
            var uncachedImplementations = selections.GetUncachedImplementations(Policy.Fetcher.Store).ToList();

            // Do not waste time on Fetcher subsystem if nothing is missing from cache
            if (uncachedImplementations.Count == 0) return;

            // Only show implementations in the UI that need to be downloaded
            Policy.Handler.ShowSelections(new Selections(uncachedImplementations), Policy.FeedManager.Cache);

            try
            {
                var toDownload = uncachedImplementations.Select(impl => impl.GetOriginalImplementation(Policy.FeedManager.Cache).CloneImplementation());
                Policy.Fetcher.FetchImplementations(toDownload, Policy.Handler);
            }
                #region Error handling
            catch
            {
                // Suppress any left-over errors if the user canceled anyway
                Policy.Handler.CancellationToken.ThrowIfCancellationRequested();
                throw;
            }
            #endregion
        }

        private void Clean(IEnumerable<ManifestDigest> digestsToKeep)
        {
            var toDelete = Policy.Fetcher.Store.ListAll().
                                  Except(digestsToKeep, new ManifestDigestPartialEqualityComparer()).ToList();
            Policy.Handler.RunTask(new ForEachTask<ManifestDigest>(Resources.RemovingOutdated, toDelete,
                Policy.Fetcher.Store.Remove), null);
        }
        #endregion
    }
}
