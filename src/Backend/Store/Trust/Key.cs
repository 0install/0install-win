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
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// A known OpenPGP key, trusted to sign feeds from a certain set of domains.
    /// </summary>
    [XmlType("key", Namespace = TrustDB.XmlNamespace)]
    public sealed class Key : ICloneable<Key>, IEquatable<Key>
    {
        /// <summary>
        /// The cryptographic fingerprint of this key.
        /// </summary>
        [XmlAttribute("fingerprint")]
        public string Fingerprint { get; set; }

        // Note: Can not use ICollection<T> interface with XML Serialization
        /// <summary>
        /// A list of <see cref="Domain"/>s this key is valid for.
        /// </summary>
        [XmlElement("domain"), NotNull]
        public DomainSet Domains { get; } = new DomainSet();

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            return Fingerprint + " " + Domains;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Key"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Key"/>.</returns>
        public Key Clone()
        {
            var key = new Key {Fingerprint = Fingerprint};
            key.Domains.AddRange(Domains.CloneElements());
            return key;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Key other)
        {
            if (other == null) return false;
            return Fingerprint == other.Fingerprint && Domains.SetEquals(other.Domains);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is Key && Equals((Key)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Fingerprint ?? "").GetHashCode();
                result = (result * 397) ^ Domains.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
