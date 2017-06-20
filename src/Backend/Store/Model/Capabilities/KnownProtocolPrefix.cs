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
using System.Xml.Serialization;
using NanoByte.Common;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// Names a well-known protocol prefix. Used for protocols that are shared accross many applications (e.g. HTTP, FTP) but not for application-specific protocols.
    /// </summary>
    /// <seealso cref="UrlProtocol.KnownPrefixes"/>
    [Description("Names a well-known protocol prefix. Used for protocols that are shared accross many applications (e.g. HTTP, FTP) but not for application-specific protocols.")]
    [Serializable, XmlRoot("known-prefix", Namespace = CapabilityList.XmlNamespace), XmlType("known-prefix", Namespace = CapabilityList.XmlNamespace)]
    public class KnownProtocolPrefix : XmlUnknown, ICloneable<KnownProtocolPrefix>, IEquatable<KnownProtocolPrefix>
    {
        /// <summary>
        /// The value of the prefix (e.g. "http").
        /// </summary>
        [Description("The value of the prefix (e.g. \"http\").")]
        [XmlAttribute("value")]
        public string Value { get; set; }

        #region Conversion
        /// <summary>
        /// Returns the prefix in the form "Value". Not safe for parsing!
        /// </summary>
        public override string ToString() => Value;
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="KnownProtocolPrefix"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="KnownProtocolPrefix"/>.</returns>
        public KnownProtocolPrefix Clone() => new KnownProtocolPrefix {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Value = Value};
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(KnownProtocolPrefix other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Value == Value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is KnownProtocolPrefix && Equals((KnownProtocolPrefix)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Value?.GetHashCode() ?? 0;
                return result;
            }
        }
        #endregion
    }
}
