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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NanoByte.Common.Collections;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Helper functions for <see cref="ISolver"/> implementations.
    /// </summary>
    public static class SolverUtils
    {
        /// <summary>
        /// Returns a list of <see cref="Requirements"/> that substitute blanks with appropriate default values.
        /// Multiple different <see cref="Requirements"/> represent equally valid but mutually exclusive choices such as 32-bit vs 64-bit processes.
        /// </summary>
        /// <param name="requirements">The baseline <see cref="Requirements"/> to extend.</param>
        /// <returns>1 or more alternative <see cref="Requirements"/> ordered from most to least optimal.</returns>
        public static IEnumerable<Requirements> GetEffective(this Requirements requirements)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            #endregion

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
        public static ImplementationSelection ToSelection(this SelectionCandidate candidate, IEnumerable<SelectionCandidate> allCandidates, Requirements requirements)
        {
            #region Sanity checks
            if (candidate == null) throw new ArgumentNullException("candidate");
            if (allCandidates == null) throw new ArgumentNullException("allCandidates");
            if (requirements == null) throw new ArgumentNullException("requirements");
            #endregion

            var implementation = candidate.Implementation;
            var selection = new ImplementationSelection(allCandidates)
            {
                ID = implementation.ID,
                LocalPath = implementation.LocalPath,
                ManifestDigest = implementation.ManifestDigest,
                Architecture = implementation.Architecture,
                Version = implementation.Version,
                Released = implementation.Released,
                Stability = candidate.EffectiveStability,
                License = implementation.License,
                InterfaceUri = requirements.InterfaceUri
            };
            if (candidate.FeedUri != requirements.InterfaceUri) selection.FromFeed = candidate.FeedUri;

            var externalImplementation = implementation as ExternalImplementation;
            if (externalImplementation != null) selection.QuickTestFile = externalImplementation.QuickTestFile;

            selection.Bindings.AddRange(implementation.Bindings.CloneElements());
            selection.AddDependencies(requirements, from: candidate.Implementation);
            selection.AddCommand(requirements, from: candidate.Implementation);

            return selection;
        }

        /// <summary>
        /// Transfers <see cref="Dependency"/>s from one <see cref="IDependencyContainer"/> to another.
        /// </summary>
        /// <param name="target">The <see cref="IDependencyContainer"/> to add the <see cref="Dependency"/>s to.</param>
        /// <param name="requirements">The requirements which restrict which <see cref="Dependency"/>s are applicable.</param>
        /// <param name="from">The <see cref="IDependencyContainer"/> to get the <see cref="Dependency"/>s to.</param>
        public static void AddDependencies(this IDependencyContainer target, Requirements requirements, IDependencyContainer from)
        {
            #region Sanity checks
            if (target == null) throw new ArgumentNullException("target");
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (from == null) throw new ArgumentNullException("from");
            #endregion

            target.Dependencies.AddRange(from.Dependencies.Where(x => x.OS.IsCompatible(requirements.Architecture)).CloneElements());
            target.Restrictions.AddRange(from.Restrictions.Where(x => x.OS.IsCompatible(requirements.Architecture)).CloneElements());
        }

        /// <summary>
        /// Adds a <see cref="Command"/> specified in an <see cref="Implementation"/> to a <see cref="ImplementationSelection"/>.
        /// </summary>
        /// <param name="selection">The <see cref="ImplementationSelection"/> to add the <see cref="Command"/> to.</param>
        /// <param name="requirements">The requirements specifying which <see cref="Requirements.Command"/> to extract.</param>
        /// <param name="from">The <see cref="Implementation"/> to get the <see cref="Command"/> from.</param>
        /// <returns>The <see cref="Command"/> that was added to <paramref name="selection"/>; <see langword="null"/> if none.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This method explicitly transfers information from an Implementation to an ImplementationSelection.")]
        public static Command AddCommand(this ImplementationSelection selection, Requirements requirements, Implementation @from)
        {
            #region Sanity checks
            if (selection == null) throw new ArgumentNullException("selection");
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (from == null) throw new ArgumentNullException("from");
            #endregion

            var command = from[requirements.Command];
            if (command == null) return null;

            var newCommand = new Command {Name = command.Name, Path = command.Path};
            newCommand.Arguments.AddRange(command.Arguments.CloneElements());
            newCommand.Bindings.AddRange(command.Bindings.CloneElements());
            if (command.WorkingDir != null) newCommand.WorkingDir = command.WorkingDir.Clone();
            if (command.Runner != null) newCommand.Runner = command.Runner.CloneRunner();
            newCommand.AddDependencies(requirements, from: command);

            selection.Commands.Add(newCommand);
            return newCommand;
        }

        /// <summary>
        /// Creates <see cref="Requirements"/> for solving a <see cref="Dependency"/> or <see cref="Restriction"/>.
        /// </summary>
        /// <param name="dependency">The dependency or restriction to solve.</param>
        /// <param name="topLevelRequirements">The top-level requirements specifying <see cref="Architecture"/> and custom restrictions.</param>
        public static Requirements ToRequirements(this Restriction dependency, Requirements topLevelRequirements)
        {
            #region Sanity checks
            if (dependency == null) throw new ArgumentNullException("dependency");
            if (topLevelRequirements == null) throw new ArgumentNullException("topLevelRequirements");
            #endregion

            var requirements = new Requirements(dependency.InterfaceUri, "", topLevelRequirements.Architecture);
            requirements.Distributions.AddRange(dependency.Distributions);
            requirements.CopyVersionRestrictions(from: dependency);
            requirements.CopyVersionRestrictions(from: topLevelRequirements);
            return requirements;
        }

        /// <summary>
        /// Creates <see cref="Requirements"/> for solving a <see cref="Runner"/> dependency.
        /// </summary>
        /// <param name="runner">The dependency to solve.</param>
        /// <param name="topLevelRequirements">The top-level requirements specifying <see cref="Architecture"/> and custom restrictions.</param>
        public static Requirements ToRequirements(this Runner runner, Requirements topLevelRequirements)
        {
            #region Sanity checks
            if (runner == null) throw new ArgumentNullException("runner");
            if (topLevelRequirements == null) throw new ArgumentNullException("topLevelRequirements");
            #endregion

            var requirements = new Requirements(runner.InterfaceUri, runner.Command ?? Command.NameRun, topLevelRequirements.Architecture);
            requirements.CopyVersionRestrictions(from: runner);
            requirements.CopyVersionRestrictions(from: topLevelRequirements);
            return requirements;
        }

        private static void CopyVersionRestrictions(this Requirements requirements, Restriction from)
        {
            if (from.Versions != null) requirements.ExtraRestrictions.Add(from.InterfaceUri, from.Versions);
        }

        private static void CopyVersionRestrictions(this Requirements requirements, Requirements from)
        {
            requirements.ExtraRestrictions.AddRange(from.ExtraRestrictions);
        }

        /// <summary>
        /// Creates <see cref="Requirements"/> for all <see cref="ExecutableInBinding"/>s and the specific <see cref="Command"/>s they reference.
        /// </summary>
        /// <param name="bindingContainer">The binding container that may contain <see cref="ExecutableInBinding"/>s.</param>
        /// <param name="interfaceUri">The interface URI the bindings refer to. For <see cref="Element"/>s and <see cref="Command"/>s this is the URI of the containing feed. For <see cref="Dependency"/>s it is the URI of the target feed.</param>
        public static IEnumerable<Requirements> ToBindingRequirements(this IBindingContainer bindingContainer, FeedUri interfaceUri)
        {
            #region Sanity checks
            if (bindingContainer == null) throw new ArgumentNullException("bindingContainer");
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            #endregion

            return bindingContainer.Bindings.OfType<ExecutableInBinding>()
                .Select(x => new Requirements(interfaceUri, x.Command ?? Command.NameRun));
        }

        /// <summary>
        /// Checks wether a set of selection candidates contains an implementation with a specific ID.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This method only operate on Selections.")]
        public static bool Contains(this IEnumerable<SelectionCandidate> candidates, ImplementationSelection implementation)
        {
            #region Sanity checks
            if (candidates == null) throw new ArgumentNullException("candidates");
            if (implementation == null) throw new ArgumentNullException("implementation");
            #endregion

            return candidates.Select(x => x.Implementation.ID).Contains(implementation.ID);
        }

        /// <summary>
        /// Removes all <see cref="Restriction"/>s from <see cref="Selections"/>.
        /// </summary>
        public static void PurgeRestrictions(this Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            foreach (var implementation in selections.Implementations)
            {
                implementation.Restrictions.Clear();

                foreach (var command in implementation.Commands)
                    command.Restrictions.Clear();
            }
        }
    }
}
