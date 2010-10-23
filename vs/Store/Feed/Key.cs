/*
 * Copyright 2010 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// An entry in the <see cref="Trust"/> database.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("key", Namespace = "http://zero-install.sourceforge.net/2007/injector/trust")]
    public sealed class Key : ICloneable, IEquatable<Key>
    {
        #region Properties
        /// <summary>
        /// The GPG cryptographic fingerprint of this key.
        /// </summary>
        [XmlAttribute("fingerprint")]
        public string Fingerprint { get; set; }

        // Order is preserved, duplicate entries are not allowed
        private readonly C5.HashedLinkedList<Domain> _domains = new C5.HashedLinkedList<Domain>();
        /// <summary>
        /// A list of <see cref="Domain"/>s this key is valid for.
        /// </summary>
        [XmlElement("domain")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedLinkedList<Domain> Domains { get { return _domains; } }
        #endregion

        //--------------------//

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
        public Key CloneKey()
        {
            var key = new Key {Fingerprint = Fingerprint};
            foreach (var domain in Domains) key.Domains.Add(domain.CloneDomain());

            return key;
        }

        public object Clone()
        {
            return CloneKey();
        }
        #endregion

        #region Equality
        public bool Equals(Key other)
        {
            if (ReferenceEquals(null, other)) return false;

            return Fingerprint == other.Fingerprint && Domains.UnsequencedEquals(other.Domains);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Key) && Equals((Key)obj);
        }

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