/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Ranks <see cref="SelectionCandidate"/>s.
    /// </summary>
    public class SelectionCandidateComparer : IComparer<SelectionCandidate>
    {
        private readonly NetworkLevel _networkUse;
        private readonly Predicate<Implementation> _isCached;
        private readonly Stability _stabilityPolicy;
        private readonly LanguageSet _languages;

        /// <summary>
        /// Creates a new <see cref="SelectionCandidate"/> ranker.
        /// </summary>
        /// <param name="config">Used to retrieve global configuration.</param>
        /// <param name="isCached">Used to determine which implementations are already cached in the <see cref="IStore"/>.</param>
        /// <param name="stabilityPolicy">Implementations at this stability level or higher are preferred. Lower levels are used only if there is no other choice.</param>
        /// <param name="languages">The preferred languages for the implementation.</param>
        public SelectionCandidateComparer([NotNull] Config config, [NotNull] Predicate<Implementation> isCached, Stability stabilityPolicy, [NotNull] LanguageSet languages)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _networkUse = config.NetworkUse;
            _stabilityPolicy = (stabilityPolicy == Stability.Unset)
                ? (config.HelpWithTesting ? Stability.Testing : Stability.Stable)
                : stabilityPolicy;

            _isCached = isCached ?? throw new ArgumentNullException(nameof(isCached));
            _languages = languages ?? throw new ArgumentNullException(nameof(languages));
        }

        /// <inheritdoc/>
        public int Compare(SelectionCandidate x, SelectionCandidate y)
        {
            if (ReferenceEquals(
                x ?? throw new ArgumentNullException(nameof(x)),
                y ?? throw new ArgumentNullException(nameof(y)))) return 0;

            // Preferred implementations come first
            if (x.EffectiveStability == Stability.Preferred && y.EffectiveStability != Stability.Preferred) return -1;
            if (x.EffectiveStability != Stability.Preferred && y.EffectiveStability == Stability.Preferred) return 1;

            // Strongly prefer languages we understand
            int xLanguageRank = GetLanguageRank(x.Implementation.Languages);
            int yLanguageRank = GetLanguageRank(y.Implementation.Languages);
            if (xLanguageRank > yLanguageRank) return -1;
            if (xLanguageRank < yLanguageRank) return 1;

            // Cached implementations come next if we have limited network access
            if (_networkUse != NetworkLevel.Full)
            {
                bool xCached = _isCached(x.Implementation);
                bool yCached = _isCached(y.Implementation);
                if (xCached && !yCached) return -1;
                if (!xCached && yCached) return 1;
            }

            // TODO: Packages that require admin access to install come last

            // Implementations at or above the selected stability level come before all others (smaller enum value = more stable)
            if (x.EffectiveStability <= _stabilityPolicy && y.EffectiveStability > _stabilityPolicy) return -1;
            if (x.EffectiveStability > _stabilityPolicy && y.EffectiveStability <= _stabilityPolicy) return 1;

            // Newer versions come before older ones
            if (x.Version > y.Version) return -1;
            if (x.Version < y.Version) return 1;

            // More specific CPU types come first (checking whether the CPU type is compatible at all is done elsewhere)
            if (x.Implementation.Architecture.Cpu > y.Implementation.Architecture.Cpu) return -1;
            if (x.Implementation.Architecture.Cpu < y.Implementation.Architecture.Cpu) return 1;

            // Slightly prefer languages specialised to our country
            int xCountryRank = GetCountryRank(x.Implementation.Languages);
            int yCountryRank = GetCountryRank(y.Implementation.Languages);
            if (xCountryRank > yCountryRank) return -1;
            if (xCountryRank < yCountryRank) return 1;

            // Slightly prefer cached versions
            if (_networkUse == NetworkLevel.Full)
            {
                bool xCached = _isCached(x.Implementation);
                bool yCached = _isCached(y.Implementation);
                if (xCached && !yCached) return -1;
                if (!xCached && yCached) return 1;
            }

            // Order by ID so the order is not random
            return string.CompareOrdinal(x.Implementation.ID, y.Implementation.ID);
        }

        private static readonly LanguageSet _englishSet = new LanguageSet {"en", "en_US"};

        private int GetLanguageRank(LanguageSet languages)
        {
            if (languages.Count == 0) return 1;
            else if (languages.ContainsAny(_languages, ignoreCountry: true)) return 2;
            else if (languages.ContainsAny(_englishSet)) return 0; // Prefer English over other languages we do not understand
            else return -1;
        }

        private int GetCountryRank(LanguageSet languages)
        {
            if (languages.Count == 0) return 0;
            else if (languages.ContainsAny(_languages)) return 1;
            else return -1;
        }
    }
}
