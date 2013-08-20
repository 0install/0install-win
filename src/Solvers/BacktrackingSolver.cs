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
            else throw new SolverException(":(");
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
            private readonly IHandler _handler;
            private readonly List<Restriction> _restrictions = new List<Restriction>();

            public BacktrackingRun(Config config, IFeedManager feedManager, IStore store, IHandler handler)
                : base(config, feedManager, store)
            {
                _handler = handler;
            }

            public bool TryToSolve(Requirements requirements)
            {
                _handler.CancellationToken.ThrowIfCancellationRequested();

                var candidates = GetSortedCandidates(requirements);
                var filteredCandidates = FilterCandidates(candidates, requirements);

                if (Selections.ContainsImplementation(requirements.InterfaceID))
                    return filteredCandidates.Select(c => c.Implementation.ID).Contains(Selections[requirements.InterfaceID].ID);

                foreach (var candidate in filteredCandidates)
                {
                    Selections.Implementations.Add(candidate.ToSelection(candidates, requirements));
                    _restrictions.AddRange(candidate.Implementation.Restrictions);

                    if (candidate.Implementation.Dependencies.All(dependency => TryToSolve(dependency.ToRequirements(requirements))) &&
                        candidate.Implementation.Commands.Where(command => command.Runner != null).All(command => TryToSolve(command.Runner.ToRequirements(requirements))))
                        return true;
                    else
                    {
                        Selections.Implementations.RemoveAt(Selections.Implementations.Count - 1);
                        _restrictions.RemoveRange(_restrictions.Count - candidate.Implementation.Restrictions.Count - 1, candidate.Implementation.Restrictions.Count);
                    }
                }
                return false;
            }

            private IEnumerable<SelectionCandidate> FilterCandidates(IEnumerable<SelectionCandidate> candidates, Requirements requirements)
            {
                // TODO: Prevent x86 AMD64 mixing
                return from candidate in candidates
                    where candidate.IsSuitable
                    // Candidate does no conflict with existing restrictions
                    where !_restrictions.Any(restriction =>
                        restriction.Interface == requirements.InterfaceID &&
                        !restriction.EffectiveVersions.Match(candidate.Version))
                    // Candidate restrictions do not conflict with existing selections
                    where candidate.Implementation.Restrictions.All(restriction =>
                        !Selections.ContainsImplementation(restriction.Interface) ||
                        restriction.EffectiveVersions.Match(Selections[restriction.Interface].Version))
                    select candidate;
            }
        }
    }
}
