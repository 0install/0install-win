/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// An application's ability to handle a certain URL protocol such as HTTP.
    /// </summary>
    [Description("An application's ability to handle a certain URL protocol such as HTTP.")]
    [Serializable, XmlRoot("url-protocol", Namespace = CapabilityList.XmlNamespace), XmlType("url-protocol", Namespace = CapabilityList.XmlNamespace)]
    public sealed class UrlProtocol : VerbCapability, IEquatable<UrlProtocol>
    {
        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsMachineWideOnly { get { return false; } }

        private readonly List<KnownProtocolPrefix> _knownPrefixes = new List<KnownProtocolPrefix>();

        /// <summary>
        /// A well-known protocol prefix such as "http". Should be empty and set in <see cref="Capability.ID"/> instead if it is a custom protocol.
        /// </summary>
        [Browsable(false)]
        [XmlElement("known-prefix"), NotNull]
        public List<KnownProtocolPrefix> KnownPrefixes { get { return _knownPrefixes; } }

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs { get { return new[] {"progid:" + ID}; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "ID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return ID;
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            var capability = new UrlProtocol {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID, ExplicitOnly = ExplicitOnly};
            capability.Icons.AddRange(Icons);
            capability.Descriptions.AddRange(Descriptions.CloneElements());
            capability.Verbs.AddRange(Verbs.CloneElements());
            capability.KnownPrefixes.AddRange(KnownPrefixes);
            return capability;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(UrlProtocol other)
        {
            if (other == null) return false;
            return base.Equals(other) && KnownPrefixes.SequencedEquals(other.KnownPrefixes);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is UrlProtocol && Equals((UrlProtocol)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ KnownPrefixes.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
