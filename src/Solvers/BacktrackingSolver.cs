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
using ZeroInstall.Model.Selection;
using ZeroInstall.Solvers.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Uses basic backtracking to solve <see cref="Requirements"/>.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public sealed class BacktrackingSolver : ISolver
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
        public BacktrackingSolver(Config config, IFeedManager feedManager, IStore store, IHandler handler)
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

        #region Public interface
        /// <inheritdoc/>
        public Selections Solve(Requirements requirements, out bool staleFeeds)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (string.IsNullOrEmpty(requirements.InterfaceID)) throw new ArgumentException(Resources.MissingInterfaceID, "requirements");
            #endregion

            requirements.Normalize();

            var run = new BacktrackingRun(_config, _feedManager, _store, _handler);
            if (run.TryToSolve(requirements))
            {
                staleFeeds = run.StaleFeeds;

                run.Selections.InterfaceID = requirements.InterfaceID;
                run.Selections.Command = requirements.Command;
                return run.Selections;
            }
            else throw new SolverException("No solution found");
        }

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements)
        {
            bool temp;
            return Solve(requirements, out temp);
        }
        #endregion

        private class BacktrackingRun : SolverRun
        {
            public BacktrackingRun(Config config, IFeedManager feedManager, IStore store, IHandler handler)
                : base(config, feedManager, store, handler)
            {}

            /// <summary>
            /// Try to satisfy a set of <paramref name="requirements"/>, respecting any existing <see cref="SolverRun.Selections"/>.
            /// </summary>
            public bool TryToSolve(Requirements requirements)
            {
                Handler.CancellationToken.ThrowIfCancellationRequested();

                var allCandidates = GetSortedCandidates(requirements);
                var suitableCandidates = FilterSuitableCandidates(allCandidates, requirements.InterfaceID);

                // Stop if specific implementation already selected elsewhere in tree
                if (Selections.ContainsImplementation(requirements.InterfaceID))
                    return suitableCandidates.Contains(Selections[requirements.InterfaceID]);

                return TryToSelectCandidate(suitableCandidates, requirements, allCandidates);
            }

            /// <summary>
            /// A running list of <see cref="Restriction"/>s from all <see cref="SelectionCandidate"/>s added to <see cref="Selections"/> so far.
            /// </summary>
            private readonly List<Restriction> _restrictions = new List<Restriction>();

            private IEnumerable<SelectionCandidate> FilterSuitableCandidates(IEnumerable<SelectionCandidate> candidates, string interfaceID)
            {
                // TODO: Prevent x86 AMD64 mixing
                return candidates.Where(candidate =>
                    candidate.IsSuitable &&
                    !ConflictsWithExistingRestrictions(interfaceID, candidate) &&
                    !ConflictsWithExistingSelections(candidate));
            }

            private bool ConflictsWithExistingRestrictions(string interfaceID, SelectionCandidate candidate)
            {
                return _restrictions.Any(restriction =>
                    restriction.Interface == interfaceID && !restriction.EffectiveVersions.Match(candidate.Version));
            }

            private bool ConflictsWithExistingSelections(SelectionCandidate candidate)
            {
                return candidate.Implementation.Restrictions.Any(restriction =>
                    Selections.ContainsImplementation(restriction.Interface) && !restriction.EffectiveVersions.Match(Selections[restriction.Interface].Version));
            }

            private bool TryToSelectCandidate(IEnumerable<SelectionCandidate> candidates, Requirements requirements, IList<SelectionCandidate> allCandidates)
            {
                foreach (var candidate in candidates)
                {
                    AddToSelections(candidate, requirements, allCandidates);
                    if (TryToSolveCommand(candidate.Implementation, requirements) &&
                        TryToSolveDependencies(candidate.Implementation.Dependencies, requirements))
                        return true;
                    else RemoveFromSelections(candidate);
                }
                return false;
            }

            private bool TryToSolveDependencies(IEnumerable<Dependency> dependencies, Requirements requirements)
            {
                return dependencies.All(dependency => TryToSolve(dependency.ToRequirements(requirements)));
            }

            private bool TryToSolveCommand(Implementation implementation, Requirements requirements)
            {
                if (string.IsNullOrEmpty(requirements.Command)) return true;
                var command = implementation[requirements.Command];
                
                if (command.Runner != null && !TryToSolve(command.Runner.ToRequirements(requirements))) return false;
                return TryToSolveDependencies(command.Dependencies, requirements);
            }

            private void AddToSelections(SelectionCandidate candidate, Requirements requirements, IEnumerable<SelectionCandidate> allCandidates)
            {
                Selections.Implementations.Add(candidate.ToSelection(allCandidates, requirements));
                _restrictions.AddRange(candidate.Implementation.Restrictions);
            }

            private void RemoveFromSelections(SelectionCandidate candidate)
            {
                Selections.Implementations.RemoveLast();
                _restrictions.RemoveLast(candidate.Implementation.Restrictions.Count);
            }
        }
    }
}
