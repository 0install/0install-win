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
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// SUMMARY
    /// </summary>
    internal static class SolverUtils
    {
        public static ImplementationSelection ToSelection(this SelectionCandidate candidate, IEnumerable<SelectionCandidate> allCandidates, Requirements requirements)
        {
            var implementation = candidate.Implementation;
            var selection = new ImplementationSelection(allCandidates)
            {
                ID = implementation.ID,
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
            selection.Commands.Add(candidate.Implementation[requirements.Command]);

            return selection;
        }

        public static Requirements ToRequirements(this Dependency dependency, Requirements baseRequirements)
        {
            var requirements = new Requirements
            {
                InterfaceID = dependency.Interface,
                Versions = dependency.EffectiveVersions,
                Architecture = baseRequirements.Architecture
            };
            requirements.VersionsFor.AddAll(baseRequirements.VersionsFor);
            requirements.Normalize();
            return requirements;
        }

        public static Requirements ToRequirements(this Runner runner, Requirements baseRequirements)
        {
            var requirements = new Requirements
            {
                InterfaceID = runner.Interface,
                Command = runner.Command,
                Versions = runner.EffectiveVersions,
                Architecture = baseRequirements.Architecture
            };
            requirements.VersionsFor.AddAll(baseRequirements.VersionsFor);
            requirements.Normalize();
            return requirements;
        }
    }
}
