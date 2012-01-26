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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's ability to handle a certain URL protocol (e.g. HTTP).
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [XmlType("url-protocol", Namespace = XmlNamespace)]
    public sealed class UrlProtocol : VerbCapability, IEquatable<UrlProtocol>
    {
        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsSystemWideOnly { get { return false; } }

        // Preserve order
        private readonly C5.LinkedList<KnownProtocolPrefix> _knownPrefixes = new C5.LinkedList<KnownProtocolPrefix>();

        /// <summary>
        /// A well-known protocol prefix such as "http". Should be empty and set in <see cref="Capability.ID"/> instead if it is a custom protocol.
        /// </summary>
        [Description("The protocoles prefix such as \"http\". Should be empty and set in ID instead if it is a custom protocol.")]
        [XmlElement("known-prefix")]
        public C5.LinkedList<KnownProtocolPrefix> KnownPrefixes { get { return _knownPrefixes; } }

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs { get { return new[] {"progid:" + ID}; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "UrlProtocol: ID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("UrlProtocol : {0}", ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            var capability = new UrlProtocol {ID = ID};
            capability.Icons.AddAll(Icons);
            foreach (var description in Descriptions) capability.Descriptions.Add(description.Clone());
            foreach (var verb in Verbs) capability.Verbs.Add(verb.Clone());
            capability.KnownPrefixes.AddAll(KnownPrefixes);
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
                int result = base.GetHashCode();
                result = (result * 397) ^ KnownPrefixes.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
