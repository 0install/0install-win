﻿/*
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
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Ranks <see cref="SelectionCandidate"/>s.
    /// </summary>
    public class SelectionCandidateComparer : IComparer<SelectionCandidate>
    {
        private readonly NetworkLevel _networkUse;
        private readonly Stability _stabilityPolicy;
        private readonly IStore _store;

        /// <summary>
        /// Creates a new <see cref="SelectionCandidate"/> ranker.
        /// </summary>
        /// <param name="config">Used to retrieve global configuration.</param>
        /// <param name="stabilityPolicy">Implementations at this stability level or higher are preferred. Lower levels are used only if there is no other choice. Must not be <see cref="Stability.Unset"/>!</param>
        /// <param name="store">Used to determine which implementations are already cached.</param>
        public SelectionCandidateComparer(Config config, Stability stabilityPolicy, IStore store)
        {
            #region Sanity check
            if (config == null) throw new ArgumentNullException("config");
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _networkUse = config.NetworkUse;
            _stabilityPolicy = (stabilityPolicy == Stability.Unset)
                ? (config.HelpWithTesting ? Stability.Testing : Stability.Stable)
                : stabilityPolicy;
            _store = store;
        }

        /// <inheritdoc/>
        public int Compare(SelectionCandidate x, SelectionCandidate y)
        {
            #region Sanity checks
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            #endregion

            // Preferred implementations come first
            if (x.EffectiveStability == Stability.Preferred && y.EffectiveStability != Stability.Preferred) return -1;
            if (x.EffectiveStability != Stability.Preferred && y.EffectiveStability == Stability.Preferred) return 1;

            // TODO: Strongly prefer languages we understand

            // Cached implementations come next if we have limited network access
            if (_networkUse != NetworkLevel.Full)
            {
                bool xCached = _store.Contains(x.Implementation.ManifestDigest);
                bool yCached = _store.Contains(x.Implementation.ManifestDigest);
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

            // TODO: Slightly prefer languages specialised to our country

            // Slightly prefer cached versions
            if (_networkUse == NetworkLevel.Full)
            {
                bool xCached = _store.Contains(x.Implementation.ManifestDigest);
                bool yCached = _store.Contains(x.Implementation.ManifestDigest);
                if (xCached && !yCached) return -1;
                if (!xCached && yCached) return 1;
            }

            // Order by ID so the order is not random
            return string.CompareOrdinal(x.Implementation.ID, y.Implementation.ID);
        }
    }
}
