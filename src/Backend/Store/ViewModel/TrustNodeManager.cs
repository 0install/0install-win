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
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Maps between <see cref="TrustDB"/> and <see cref="TrustNode"/>s.
    /// </summary>
    public static class TrustNodeManager
    {
        /// <summary>
        /// Creates <see cref="TrustNode"/> representations for all entries in a <see cref="TrustDB"/>.
        /// </summary>
        public static NamedCollection<TrustNode> ToNodes([NotNull] this TrustDB trustDB)
        {
            #region Sanity checks
            if (trustDB == null) throw new ArgumentNullException(nameof(trustDB));
            #endregion

            var nodes = new NamedCollection<TrustNode>();
            foreach (var key in trustDB.Keys)
            {
                foreach (var domain in key.Domains)
                    nodes.Add(new TrustNode(key.Fingerprint, domain));
            }
            return nodes;
        }

        /// <summary>
        /// Creates a <see cref="TrustDB"/> from <see cref="TrustNode"/>s.
        /// </summary>
        public static TrustDB ToTrustDB([NotNull] this IEnumerable<TrustNode> nodes)
        {
            #region Sanity checks
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            #endregion

            var trustDB = new TrustDB();
            foreach (var node in nodes)
                trustDB.TrustKey(node.Fingerprint, node.Domain);
            return trustDB;
        }
    }
}
