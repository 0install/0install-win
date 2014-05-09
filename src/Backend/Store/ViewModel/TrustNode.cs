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
using System.ComponentModel;
using NanoByte.Common;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Represents a <see cref="Key"/>-<see cref="Domain"/> pair in a <see cref="TrustDB"/> for display in a UI.
    /// </summary>
    public class TrustNode : Node, INamed<TrustNode>
    {
        /// <summary>
        /// The <see cref="Key.Fingerprint"/>.
        /// </summary>
        public string Fingerprint { get; private set; }

        /// <summary>
        /// The domain the fingerprint is valid for.
        /// </summary>
        public Domain Domain { get; private set; }

        /// <inheritdoc/>
        [Browsable(false)]
        public override string Name { get { return Fingerprint + '\\' + Domain.Value; } set { throw new NotSupportedException(); } }

        /// <summary>
        /// Creates a new <see cref="Key"/>-<see cref="Domain"/> pair.
        /// </summary>
        /// <param name="fingerprint">The <see cref="Key.Fingerprint"/>.</param>
        /// <param name="domain">The domain the fingerprint is valid for.</param>
        public TrustNode(string fingerprint, Domain domain)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException("fingerprint");
            #endregion

            Fingerprint = fingerprint;
            Domain = domain;
        }

        int IComparable<TrustNode>.CompareTo(TrustNode other)
        {
            #region Sanity checks
            if (other == null) throw new ArgumentNullException("other");
            #endregion

            int fingerprintCompare = string.CompareOrdinal(Fingerprint, other.Fingerprint);
            return (fingerprintCompare == 0) ? string.CompareOrdinal(Domain.Value, other.Domain.Value) : fingerprintCompare;
        }
    }
}