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
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Shared logic for keeping state during a single <see cref="ISolver.Solve"/> run.
    /// </summary>
    public abstract class SolverRun
    {
        #region Depdendencies
        protected CancellationToken CancellationToken;
        protected readonly Requirements TopLevelRequirements;
        private readonly Config _config;
        private readonly IStore _store;

        /// <summary>
        /// Creates a new solver run.
        /// </summary>
        /// <param name="requirements">The top-level requirements the solver should try to meet.</param>
        /// <param name="cancellationToken">Used to signal when the user wishes to cancel the solver run.</param>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="store">Used to check which <see cref="Implementation"/>s are already cached.</param>
        protected SolverRun(Requirements requirements, CancellationToken cancellationToken, Config config, IFeedManager feedManager, IStore store)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (config == null) throw new ArgumentNullException("config");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            CancellationToken = cancellationToken;
            _config = config;
            _store = store;

            TopLevelRequirements = requirements;
            Selections.InterfaceID = requirements.InterfaceID;
            Selections.Command = requirements.Command;

            _comparer = new TransparentCache<string, SelectionCandidateComparer>(id => new SelectionCandidateComparer(config, _interfacePreferences[id].StabilityPolicy, store));
            _feeds = new TransparentCache<string, Feed>(feedManager.GetFeed);
        }
        #endregion

        #region Caches
        /// <summary>Maps interface IDs to <see cref="InterfacePreferences"/>.</summary>
        private readonly TransparentCache<string, InterfacePreferences> _interfacePreferences = new TransparentCache<string, InterfacePreferences>(InterfacePreferences.LoadForSafe);

        /// <summary>Maps interface IDs to <see cref="SelectionCandidateComparer"/>s.</summary>
        private readonly TransparentCache<string, SelectionCandidateComparer> _comparer;

        /// <summary>Maps feed IDs to <see cref="Feed"/>s. Transparent caching ensures individual feeds do not change during solver run.</summary>
        private readonly TransparentCache<string, Feed> _feeds;

        /// <summary>
        /// Retrieves the original <see cref="Implementation"/> an <see cref="ImplementationSelection"/> was based ofF.
        /// </summary>
        protected Implementation GetOriginalImplementation(ImplementationSelection implemenationSelection)
        {
            #region Sanity checks
            if (implemenationSelection == null) throw new ArgumentNullException("implemenationSelection");
            #endregion

            return _feeds[implemenationSelection.FromFeed ?? implemenationSelection.InterfaceID][implemenationSelection.ID];
        }
        #endregion

        #region Properties
        private readonly Selections _selections = new Selections();

        /// <summary>
        /// The implementations selected by the solver run.
        /// </summary>
        public Selections Selections { get { return _selections; } }
        #endregion

        /// <summary>
        /// Try to satisfy the <see cref="TopLevelRequirements"/>. If successful the result can be retrieved from <see cref="Selections"/>.
        /// </summary>
        /// <returns><see langword="true"/> if a solution was found; <see langword="false"/> otherwise.</returns>
        public abstract bool TryToSolve();

        #region Candidates
        /// <summary>
        /// Gets all <see cref="SelectionCandidate"/>s for a specific set of <see cref="Requirements"/> sorted from best to worst.
        /// </summary>
        public IList<SelectionCandidate> GetSortedCandidates(Requirements requirements)
        {
            var candidates = GetFeeds(requirements)
                .SelectMany(x => GetCandidates(x.Key, x.Value, requirements))
                .ToList();
            candidates.Sort(_comparer[requirements.InterfaceID]);
            return candidates;
        }

        private IDictionary<string, Feed> GetFeeds(Requirements requirements)
        {
            var dictionary = new Dictionary<string, Feed>();

            AddFeed(dictionary, requirements.InterfaceID, requirements);
            foreach (var reference in _interfacePreferences[requirements.InterfaceID].Feeds)
                AddFeed(dictionary, reference.Source, requirements);

            return dictionary;
        }

        private void AddFeed(IDictionary<string, Feed> dictionary, string feedID, Requirements requirements)
        {
            if (dictionary.ContainsKey(feedID)) return;

            var feed = _feeds[feedID];
            dictionary.Add(feedID, feed);

            foreach (var reference in feed.Feeds
                .Where(reference => reference.Architecture.IsCompatible(requirements.Architecture) &&
                                    reference.Languages.ContainsAny(requirements.Languages)))
                AddFeed(dictionary, reference.Source, requirements);
        }

        private IEnumerable<SelectionCandidate> GetCandidates(string feedID, Feed feed, Requirements requirements)
        {
            var feedPreferences = FeedPreferences.LoadForSafe(feedID);

            if (UnixUtils.IsUnix && feed.Elements.OfType<PackageImplementation>().Any())
                Log.Warn("Linux native package managers not supported yet!");
            // TODO: Windows <package-implementation>s

            return
                from implementation in feed.Elements.OfType<Implementation>()
                let offlineUncached = (_config.NetworkUse == NetworkLevel.Offline && !_store.Contains(implementation.ManifestDigest))
                select new SelectionCandidate(feedID, feedPreferences, implementation, requirements, offlineUncached);
        }
        #endregion
    }
}
