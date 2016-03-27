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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Represents a <see cref="Key"/>-<see cref="Domain"/> pair in a <see cref="TrustDB"/> for display in a UI.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for string sorting in UI lists")]
    public class TrustNode : INamed<TrustNode>, IEquatable<TrustNode>
    {
        /// <summary>
        /// Creates a new <see cref="Key"/>-<see cref="Domain"/> pair.
        /// </summary>
        /// <param name="fingerprint">The <see cref="Key.Fingerprint"/>.</param>
        /// <param name="domain">The domain the fingerprint is valid for.</param>
        public TrustNode([NotNull] string fingerprint, Domain domain)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException("fingerprint");
            #endregion

            Fingerprint = fingerprint;
            Domain = domain;
        }

        /// <summary>
        /// The UI path name of this node. Uses a backslash as the separator in hierarchical names.
        /// </summary>
        [Browsable(false)]
        [NotNull]
        public string Name { get { return Fingerprint + "\\" + Domain.Value; } set { throw new NotSupportedException(); } }

        /// <summary>
        /// The <see cref="Key.Fingerprint"/>.
        /// </summary>
        [NotNull]
        public string Fingerprint { get; private set; }

        /// <summary>
        /// The domain the fingerprint is valid for.
        /// </summary>
        public Domain Domain { get; private set; }

        #region Equality
        /// <inheritdoc/>
        public bool Equals(TrustNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Fingerprint == other.Fingerprint && Domain == other.Domain;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TrustNode)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Fingerprint.GetHashCode() * 397) ^ Domain.GetHashCode();
            }
        }
        #endregion

        #region Comparison
        /// <inheritdoc/>
        int IComparable<TrustNode>.CompareTo(TrustNode other)
        {
            #region Sanity checks
            if (other == null) throw new ArgumentNullException("other");
            #endregion

            int fingerprintCompare = string.CompareOrdinal(Fingerprint, other.Fingerprint);
            return (fingerprintCompare == 0) ? string.CompareOrdinal(Domain.Value, other.Domain.Value) : fingerprintCompare;
        }
        #endregion
    }
}
