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

using System.Collections.Generic;
using System.Linq;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Helper functions for <see cref="ISolver"/> implementations.
    /// </summary>
    internal static class SolverUtils
    {
        /// <summary>
        /// Returns a list of <see cref="Requirements"/> that substitute blanks with appropriate default values.
        /// Multiple different <see cref="Requirements"/> represent equally valid but mutually exclusive choices such as 32-bit vs 64-bit processes.
        /// </summary>
        /// <param name="requirements">The baseline <see cref="Requirements"/> to extend.</param>
        /// <returns>1 or more alternative <see cref="Requirements"/> ordered from most to least optimal.</returns>
        public static IEnumerable<Requirements> GetEffective(this Requirements requirements)
        {
            var effectiveRequirements = requirements.Clone();
            effectiveRequirements.Command = requirements.Command ?? (requirements.Architecture.Cpu == Cpu.Source ? Command.NameCompile : Command.NameRun);
            effectiveRequirements.Architecture = new Architecture(
                (effectiveRequirements.Architecture.OS == OS.All) ? Architecture.CurrentSystem.OS : effectiveRequirements.Architecture.OS,
                (effectiveRequirements.Architecture.Cpu == Cpu.All) ? Architecture.CurrentSystem.Cpu : effectiveRequirements.Architecture.Cpu);

            if (effectiveRequirements.Architecture.Cpu == Cpu.X64)
            { // x86-on-X64 compatability
                var x86Requirements = effectiveRequirements.Clone();
                x86Requirements.Architecture = new Architecture(x86Requirements.Architecture.OS, Cpu.I686);
                return new[] {effectiveRequirements, x86Requirements};
            }
            else return new[] {effectiveRequirements};
        }

        /// <summary>
        /// Turns a <see cref="SelectionCandidate"/> into a <see cref="ImplementationSelection"/>.
        /// </summary>
        /// <param name="candidate">The selected candidate.</param>
        /// <param name="allCandidates">All candidates that were considered for selection (including the selected one). These are used to present the user with possible alternatives.</param>
        /// <param name="requirements">The requirements the selected candidate was chosen for.</param>
        /// <returns></returns>
        public static ImplementationSelection ToSelection(this SelectionCandidate candidate, IEnumerable<SelectionCandidate> allCandidates, Requirements requirements)
        {
            var implementation = candidate.Implementation;
            var selection = new ImplementationSelection(allCandidates)
            {
                ID = implementation.ID,
                LocalPath = implementation.LocalPath,
                ManifestDigest = implementation.ManifestDigest,
                Architecture = implementation.Architecture,
                Version = implementation.Version,
                Released = implementation.Released,
                License = implementation.License,
                InterfaceID = requirements.InterfaceID,
            };
            if (candidate.FeedID != requirements.InterfaceID) selection.FromFeed = candidate.FeedID;

            selection.Dependencies.AddAll(implementation.Dependencies);
            selection.Bindings.AddAll(implementation.Bindings);

            var command = candidate.Implementation.GetCommand(requirements.Command);
            if (command != null) selection.Commands.Add(command);

            return selection;
        }

        /// <summary>
        /// Creates <see cref="Requirements"/> for solving a <see cref="Dependency"/>.
        /// </summary>
        /// <param name="dependency">The dependency to solve.</param>
        /// <param name="topLevelRequirements">The top-level requirements specifying <see cref="Architecture"/> and custom restrictions.</param>
        public static Requirements ToRequirements(this Dependency dependency, Requirements topLevelRequirements)
        {
            var requirements = new Requirements
            {
                InterfaceID = dependency.Interface,
                Command = "",
                Versions = dependency.EffectiveVersions,
                Architecture = topLevelRequirements.Architecture
            };
            requirements.VersionsFor.AddAll(topLevelRequirements.VersionsFor);
            return requirements;
        }

        /// <summary>
        /// Creates <see cref="Requirements"/> for solving a <see cref="Runner"/> dependency.
        /// </summary>
        /// <param name="runner">The dependency to solve.</param>
        /// <param name="topLevelRequirements">The top-level requirements specifying <see cref="Architecture"/> and custom restrictions.</param>
        public static Requirements ToRequirements(this Runner runner, Requirements topLevelRequirements)
        {
            var requirements = new Requirements
            {
                InterfaceID = runner.Interface,
                Command = runner.Command ?? Command.NameRun,
                Versions = runner.EffectiveVersions,
                Architecture = topLevelRequirements.Architecture
            };
            requirements.VersionsFor.AddAll(topLevelRequirements.VersionsFor);
            return requirements;
        }

        /// <summary>
        /// Checks wether a set of selection candidates contains an implementation with a specific ID.
        /// </summary>
        public static bool Contains(this IEnumerable<SelectionCandidate> candidates, string implementationID)
        {
            return candidates.Select(c => c.Implementation.ID).Contains(implementationID);
        }
    }
}
