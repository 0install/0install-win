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

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// A specific domain with feeds a <see cref="Key"/> is trusted to sign.
    /// </summary>
    [XmlType("domain", Namespace = TrustDB.XmlNamespace)]
    public struct Domain : ICloneable, IEquatable<Domain>
    {
        /// <summary>
        /// A valid domain name (not a full <see cref="Uri"/>!).
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }

        /// <summary>
        /// Creates a new domain entry.
        /// </summary>
        /// <param name="value">A valid domain name (not a full <see cref="Uri"/>!).</param>
        public Domain(string value) : this()
        {
            Value = value;
        }

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            return Value;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Domain"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Domain"/>.</returns>
        public Domain Clone()
        {
            return new Domain(Value);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Domain other)
        {
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public static bool operator ==(Domain left, Domain right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(Domain left, Domain right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj is Domain && Equals((Domain)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Value ?? "").GetHashCode();
        }
        #endregion
    }
}
