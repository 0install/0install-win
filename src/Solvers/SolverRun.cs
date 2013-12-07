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
using Common.Collections;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Solvers.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Shared logic for keeping state during a single <see cref="ISolver.Solve"/> run.
    /// </summary>
    internal abstract class SolverRun
    {
        #region Depdendencies
        private readonly Config _config;
        private readonly IStore _store;

        /// <summary>
        /// A callback object used when the the user needs to be asked questions or informed about download and IO tasks.
        /// </summary>
        protected readonly IHandler Handler;

        /// <summary>
        /// Creates a new solver run.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="store">Used to check which <see cref="Implementation"/>s are already cached.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        protected SolverRun(Config config, IFeedManager feedManager, IStore store, IHandler handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _config = config;
            _store = store;
            Handler = handler;

            _comparer = new TransparentCache<string, SelectionCandidateComparer>(id => new SelectionCandidateComparer(config, _interfacePreferences[id].StabilityPolicy, store));
            _feeds = new TransparentCache<string, Feed>(feedManager.GetFeed);
        }
        #endregion

        #region Caches
        /// <summary>Maps interface IDs to <see cref="InterfacePreferences"/>.</summary>
        private readonly TransparentCache<string, InterfacePreferences> _interfacePreferences = new TransparentCache<string, InterfacePreferences>(InterfacePreferences.LoadForSafe);

        /// <summary>Maps interface IDs to <see cref="SelectionCandidateComparer"/>s.</summary>
        private readonly TransparentCache<string, SelectionCandidateComparer> _comparer;

        /// <summary>Maps feed IDs to <see cref="FeedPreferences"/>. Transparent caching ensures individual preferences do not change during solver run.</summary>
        private readonly TransparentCache<string, FeedPreferences> _feedPreferences = new TransparentCache<string, FeedPreferences>(FeedPreferences.LoadForSafe);

        /// <summary>Maps feed IDs to <see cref="Feed"/>s. Transparent caching ensures individual feeds do not change during solver run.</summary>
        private readonly TransparentCache<string, Feed> _feeds;
        #endregion

        #region Properties
        private readonly Selections _selections = new Selections();

        public Selections Selections { get { return _selections; } }
        #endregion

        #region Candidates
        /// <summary>
        /// Gets all <see cref="SelectionCandidate"/>s for a specific set of <see cref="Requirements"/> sorted from best to worst.
        /// </summary>
        public IList<SelectionCandidate> GetSortedCandidates(Requirements requirements)
        {
            var mainCandidates = GetCandidates(requirements.InterfaceID, requirements);
            var additionalCandidates = GetCandidates(_interfacePreferences[requirements.InterfaceID].Feeds, requirements);

            var candidates = mainCandidates.Concat(additionalCandidates).ToList();
            candidates.Sort(_comparer[requirements.InterfaceID]);
            return candidates;
        }

        /// <summary>
        /// Gets all <see cref="SelectionCandidate"/>s for a specific set of <see cref="Requirements"/> from a single feed.
        /// </summary>
        private IEnumerable<SelectionCandidate> GetCandidates(string feedID, Requirements requirements)
        {
            var feed = _feeds[feedID];

            // TODO: Add support for PackageImplementations
            var mainCandidates = feed.Elements.OfType<Implementation>().Select(implementation => GetCandidate(feedID, requirements, implementation));
            var additionalCandidates = GetCandidates(feed.Feeds, requirements);
            return mainCandidates.Concat(additionalCandidates);
        }

        /// <summary>
        /// Gets a <see cref="SelectionCandidate"/> for a specific <see cref="Implementation"/>.
        /// </summary>
        private SelectionCandidate GetCandidate(string feedID, Requirements requirements, Implementation implementation)
        {
            var feedPreferences = _feedPreferences[feedID];

            // TODO: Check it is a valid implementation (has version number, manifest digest, etc.)
            var candidate = new SelectionCandidate(feedID, feedPreferences, implementation, requirements);
            if (candidate.IsSuitable && NotSuitableBecauseOffline(candidate))
            {
                candidate.IsSuitable = false;
                candidate.Notes = Resources.SelectionCandidateNoteNotCached;
            }
            return candidate;
        }

        private bool NotSuitableBecauseOffline(SelectionCandidate candidate)
        {
            return _config.EffectiveNetworkUse == NetworkLevel.Offline && !_store.Contains(candidate.Implementation.ManifestDigest);
        }

        /// <summary>
        /// Gets all <see cref="SelectionCandidate"/>s for a specific set of <see cref="Requirements"/> from a set of feeds.
        /// </summary>
        private IEnumerable<SelectionCandidate> GetCandidates(IEnumerable<FeedReference> feedReferences, Requirements requirements)
        {
            return feedReferences
                .Where(feedReference =>
                    feedReference.Architecture.IsCompatible(requirements.Architecture) &&
                    feedReference.Languages.ContainsAny(requirements.Languages))
                .SelectMany(feedReference => GetCandidates(feedReference.Source, requirements));
        }
        #endregion
    }
}
