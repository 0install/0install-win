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
using ZeroInstall.Injector;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Solvers.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Solves simple tree-like dependencies in requirements (no support for loops, diamonds, etc.).
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public sealed class SimpleSolver : ISolver
    {
        #region Dependencies
        private readonly Config _config;
        private readonly IFeedManager _feedManager;
        private readonly IStore _store;
        private readonly IHandler _handler;

        /// <summary>
        /// Creates a new simple solver.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="store">Used to check which <see cref="Implementation"/>s are already cached.</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public SimpleSolver(Config config, IFeedManager feedManager, IStore store, IHandler handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (store == null) throw new ArgumentNullException("store");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _config = config;
            _store = store;
            _feedManager = feedManager;
            _handler = handler;
        }
        #endregion

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements, out bool staleFeeds)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (string.IsNullOrEmpty(requirements.InterfaceID)) throw new ArgumentException(Resources.MissingInterfaceID, "requirements");
            #endregion

            // Fill in default architecture values
            requirements = requirements.Clone();
            requirements.Architecture = new Architecture(
                (requirements.Architecture.OS == OS.All) ? Architecture.CurrentSystem.OS : requirements.Architecture.OS,
                (requirements.Architecture.Cpu == Cpu.All) ? Architecture.CurrentSystem.Cpu : requirements.Architecture.Cpu);

            var process = new SolveProcess(_config, _store, _handler, new SolverFeedCache(_feedManager));
            return process.Run(requirements, out staleFeeds);
        }

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements)
        {
            bool temp;
            return Solve(requirements, out temp);
        }

        private class SolveProcess
        {
            #region Dependencies
            private readonly Config _config;
            private readonly IStore _store;
            private readonly IHandler _handler;
            private readonly SolverFeedCache _feedCache;

            public SolveProcess(Config config, IStore store, IHandler handler, SolverFeedCache feedCache)
            {
                _config = config;
                _store = store;
                _handler = handler;
                _feedCache = feedCache;
            }
            #endregion

            private Selections _selections;

            public Selections Run(Requirements requirements, out bool staleFeeds)
            {
                var mainRequirements = requirements.Clone();

                // Default to 'run' or 'compile' command if not specified
                if (requirements.CommandName == null)
                    requirements.CommandName = (requirements.Architecture.Cpu == Cpu.Source ? Command.NameCompile : Command.NameRun);

                _selections = new Selections {InterfaceID = requirements.InterfaceID, CommandName = requirements.CommandName};
                AddSelection(mainRequirements);

                staleFeeds = _feedCache.StaleFeeds;
                return _selections;
            }

            private void AddSelection(Requirements requirements)
            {
                _handler.CancellationToken.ThrowIfCancellationRequested();

                var candidates = new List<SelectionCandidate>();

                var interfacePreferences = InterfacePreferences.LoadForSafe(requirements.InterfaceID);

                var feeds = new Dictionary<string, FeedPreferences>();
                var feedPreferences = FeedPreferences.LoadForSafe(requirements.InterfaceID);
                feeds.Add(requirements.InterfaceID, feedPreferences);

                AddCandidatesFromFeed(candidates, requirements.InterfaceID, feedPreferences, requirements);
                //LoadAdditionalFeeds(interfacePreferences.Feeds, ...);

                SortCandidates(candidates, interfacePreferences);

                var bestCandidate = candidates.Find(candidate => candidate.IsSuitable);
                if (bestCandidate == null) throw new SolverException("No fitting candidate!");
                var implementation = bestCandidate.Implementation;

                var selection = new ImplementationSelection(feeds, candidates)
                {
                    InterfaceID = requirements.InterfaceID,
                    ID = implementation.ID,
                    ManifestDigest = implementation.ManifestDigest,
                    Architecture = implementation.Architecture,
                    Version = implementation.Version,
                    Released = implementation.Released,
                    License = implementation.License,
                };
                selection.Dependencies.AddAll(implementation.Dependencies);
                selection.Bindings.AddAll(implementation.Bindings);
                if (bestCandidate.FeedID != requirements.InterfaceID) selection.FromFeed = bestCandidate.FeedID;
                if (!string.IsNullOrEmpty(requirements.CommandName))
                {
                    var command = implementation[requirements.CommandName];
                    if (command.Runner != null) throw new SolverException("Unable to handle <runner>s!");
                    selection.Commands.Add(command);
                }
                _selections.Implementations.Add(selection);

                // ToDo: Detect cyclic or cross-references
                foreach (var dependency in implementation.Dependencies.
                                                          Where(dependency => string.IsNullOrEmpty(dependency.Use) || (dependency.Use == "testing" && requirements.CommandName == "test")))
                {
                    AddSelection(new Requirements
                    {
                        InterfaceID = dependency.Interface,
                        Architecture = requirements.Architecture,
                        Versions = dependency.EffectiveVersions
                    });
                }
            }

            private void AddCandidatesFromFeed(ICollection<SelectionCandidate> candidates, string feedID, FeedPreferences feedPreferences, Requirements requirements)
            {
                var feed = _feedCache.GetFeed(feedID);

                // ToDo: Add support for PackageImplementations
                foreach (var implementation in feed.Elements.OfType<Implementation>())
                {
                    // ToDo: Check it is a valid implementation (has version number, manifest digest, etc.)

                    // Only list implementations that provide the requested command
                    if (!string.IsNullOrEmpty(requirements.CommandName))
                        if (implementation.Commands.All(command => command.Name != requirements.CommandName)) continue;

                    var candidate = new SelectionCandidate(feedID, implementation, feedPreferences[implementation.ID], requirements);

                    // Exclude non-cached implementations when in offline-mode
                    if (candidate.IsSuitable && _config.EffectiveNetworkUse == NetworkLevel.Offline && !_store.Contains(implementation.ManifestDigest))
                    {
                        candidate.IsSuitable = false;
                        candidate.Notes = Resources.SelectionCandidateNoteNotCached;
                    }

                    candidates.Add(candidate);
                }

                //LoadAdditionalFeeds(feed.Feeds, ...);
            }

            private void SortCandidates(List<SelectionCandidate> candidates, InterfacePreferences interfacePreferences)
            {
                var stabilityPolicy = interfacePreferences.StabilityPolicy;
                if (stabilityPolicy == Stability.Unset) stabilityPolicy = _config.HelpWithTesting ? Stability.Testing : Stability.Stable;

                candidates.Sort((x, y) =>
                {
                    // ToDo: Languages we understand come first

                    // Preferred implementations come first
                    if (x.EffectiveStability == Stability.Preferred && y.EffectiveStability != Stability.Preferred) return -1;
                    if (x.EffectiveStability != Stability.Preferred && y.EffectiveStability == Stability.Preferred) return 1;

                    // Prefer available implementations next if we have limited network access
                    if (_config.EffectiveNetworkUse != NetworkLevel.Full)
                    {
                        bool xCached = _store.Contains(x.Implementation.ManifestDigest);
                        bool yCached = _store.Contains(x.Implementation.ManifestDigest);

                        if (xCached && !yCached) return -1;
                        if (!xCached && yCached) return 1;
                    }

                    // ToDo: Packages that require admin access to install come last

                    // Implementations at or above the selected stability level come before all others (smaller enum value = more stable)
                    if (x.EffectiveStability <= stabilityPolicy && y.EffectiveStability > stabilityPolicy) return -1;
                    if (x.EffectiveStability > stabilityPolicy && y.EffectiveStability <= stabilityPolicy) return 1;

                    // Newer versions come before older ones
                    if (x.Version > y.Version) return -1;
                    if (x.Version < y.Version) return 1;

                    // ToDo: Get best architecture

                    // ToDo: Slightly prefer languages specialised to our country

                    // Slightly prefer cached versions
                    if (_config.EffectiveNetworkUse == NetworkLevel.Full)
                    {
                        bool xCached = _store.Contains(x.Implementation.ManifestDigest);
                        bool yCached = _store.Contains(x.Implementation.ManifestDigest);

                        if (xCached && !yCached) return -1;
                        if (!xCached && yCached) return 1;
                    }

                    // Order by ID so the order isn't random
                    return string.CompareOrdinal(x.Implementation.ID, y.Implementation.ID);
                });
            }
        }
    }
}
