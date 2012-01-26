/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Names a well-known protocol prefix. Use this for protocols that are shared accross many applications (e.g. HTTP, FTP) but not for application-specific protocols.
    /// </summary>
    [XmlType("known-prefix", Namespace = Capability.XmlNamespace)]
    public struct KnownProtocolPrefix : IEquatable<KnownProtocolPrefix>
    {
        #region Properties
        /// <summary>
        /// The value of the prefix (e.g. "http").
        /// </summary>
        [Description("The value of the prefix (e.g. \"http\").")]
        [XmlAttribute("value")]
        public string Value { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the prefix in the form "Value". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Value;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(KnownProtocolPrefix other)
        {
            return other.Value == Value;
        }

        /// <inheritdoc/>
        public static bool operator ==(KnownProtocolPrefix left, KnownProtocolPrefix right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(KnownProtocolPrefix left, KnownProtocolPrefix right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is KnownProtocolPrefix && Equals((KnownProtocolPrefix)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Value ?? "").GetHashCode();
        }
        #endregion
    }
}
