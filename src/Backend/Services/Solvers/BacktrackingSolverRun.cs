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

using System.Collections.Generic;
using System.Linq;
using NanoByte.Common.Collections;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Holds state during a single <see cref="BacktrackingSolver.Solve"/> run.
    /// </summary>
    internal sealed class BacktrackingSolverRun : SolverRun
    {
        /// <inheritdoc/>
        public BacktrackingSolverRun(Requirements requirements, CancellationToken cancellationToken, Config config, IFeedManager feedManager, IStore store) : base(requirements, cancellationToken, config, feedManager, store)
        {}

        /// <inheritdoc/>
        public override bool TryToSolve()
        {
            return TryToSolve(TopLevelRequirements);
        }

        /// <summary>
        /// Try to satisfy a set of <paramref name="requirements"/>, respecting any existing <see cref="SolverRun.Selections"/>.
        /// </summary>
        private bool TryToSolve(Requirements requirements)
        {
            CancellationToken.ThrowIfCancellationRequested();

            var allCandidates = GetSortedCandidates(requirements);
            var suitableCandidates = FilterSuitableCandidates(allCandidates, requirements.InterfaceID);

            var existingSelection = Selections.GetImplementation(requirements.InterfaceID);
            return (existingSelection == null)
                ? TryToSelectCandidate(suitableCandidates, requirements, allCandidates)
                : TryToUseExistingCandidate(requirements, suitableCandidates, existingSelection);
        }

        /// <summary>
        /// A running list of <see cref="Restriction"/>s from all <see cref="SelectionCandidate"/>s added to <see cref="Selections"/> so far.
        /// </summary>
        private readonly List<Restriction> _restrictions = new List<Restriction>();

        private IEnumerable<SelectionCandidate> FilterSuitableCandidates(IEnumerable<SelectionCandidate> candidates, string interfaceID)
        {
            return candidates.Where(candidate =>
                candidate.IsSuitable &&
                !ConflictsWithExistingRestrictions(candidate, interfaceID) &&
                !ConflictsWithExistingSelections(candidate));
        }

        private bool ConflictsWithExistingRestrictions(SelectionCandidate candidate, string interfaceID)
        {
            return _restrictions.Any(restriction =>
                restriction.InterfaceID == interfaceID && !restriction.Versions.Match(candidate.Version));
        }

        private bool ConflictsWithExistingSelections(SelectionCandidate candidate)
        {
            return candidate.Implementation.Restrictions.Any(restriction =>
            {
                var implemenation = Selections.GetImplementation(restriction.InterfaceID);
                return implemenation != null && !restriction.Versions.Match(implemenation.Version);
            });
        }

        private bool TryToUseExistingCandidate(Requirements requirements, IEnumerable<SelectionCandidate> suitableCandidates, ImplementationSelection selection)
        {
            if (!suitableCandidates.Contains(selection)) return false;
            if (selection.ContainsCommand(requirements.Command)) return true;

            var command = selection.AddCommand(requirements, from: GetOriginalImplementation(selection));
            return TryToSolveCommand(command, requirements);
        }

        private bool TryToSelectCandidate(IEnumerable<SelectionCandidate> candidates, Requirements requirements, IList<SelectionCandidate> allCandidates)
        {
            foreach (var candidate in candidates)
            {
                var selection = AddToSelections(candidate, requirements, allCandidates);

                var command = selection[requirements.Command];
                if (TryToSolveDependencies(selection) && TryToSolveCommand(command, requirements) && TryToSolveBindingRequirements(selection)) return true;
                else RemoveLastFromSelections();
            }
            return false;
        }

        private bool TryToSolveBindingRequirements(IInterfaceIDBindingContainer selection)
        {
            return selection.ToBindingRequirements(selection.InterfaceID).All(TryToSolve);
        }

        private bool TryToSolveDependencies(IDependencyContainer dependencyContainer)
        {
            var dependencies = dependencyContainer.Dependencies;

            var essentialDependencies = new List<Dependency>();
            var recommendedDependencies = new List<Dependency>();
            dependencies.Bucketize(x => x.Importance)
                .Add(Importance.Essential, essentialDependencies)
                .Add(Importance.Recommended, recommendedDependencies)
                .Run();

            foreach (var dependency in essentialDependencies)
                if (!TryToSolveDependency(dependency)) return false;
            foreach (var dependency in recommendedDependencies)
                if (!TryToSolveDependency(dependency)) dependencies.Remove(dependency);
            return true;
        }

        private bool TryToSolveDependency(Dependency dependency)
        {
            return TryToSolve(dependency.ToRequirements(TopLevelRequirements)) && TryToSolveBindingRequirements(dependency);
        }

        private bool TryToSolveCommand(Command command, Requirements requirements)
        {
            if (command == null) return true;

            if (command.Bindings.OfType<ExecutableInBinding>().Any()) throw new SolverException("<executable-in-*> not supported in <command>");

            if (command.Runner != null)
                if (!TryToSolve(command.Runner.ToRequirements(TopLevelRequirements))) return false;

            return command.ToBindingRequirements(requirements.InterfaceID).All(TryToSolve) && TryToSolveDependencies(command);
        }

        private ImplementationSelection AddToSelections(SelectionCandidate candidate, Requirements requirements, IEnumerable<SelectionCandidate> allCandidates)
        {
            var selection = candidate.ToSelection(allCandidates, requirements);
            Selections.Implementations.Add(selection);
            _restrictions.AddRange(selection.Restrictions);
            return selection;
        }

        private void RemoveLastFromSelections()
        {
            _restrictions.RemoveLast(Selections.Implementations.Last().Restrictions.Count);
            Selections.Implementations.RemoveLast();
        }
    }
}
