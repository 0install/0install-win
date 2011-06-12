/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's ability to handle a certain URL protocol.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [XmlType("url-protocol", Namespace = XmlNamespace)]
    public class UrlProtocol : VerbCapability, IEquatable<UrlProtocol>
    {
        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool GlobalOnly { get { return false; } }

        /// <summary>
        /// The protocol prefix such as "http". Should only be set when different from <see cref="Capability.ID"/>.
        /// </summary>
        [Description("The protocol prefix such as \"http\". Should only be set when different from ID.")]
        [XmlAttribute("prefix")]
        public string Prefix { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "UrlProtocol: Description (ID)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("UrlProtocol : {0} ({1})", Description, ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability CloneCapability()
        {
            var capability = new UrlProtocol {ID = ID, Description = Description, Prefix = Prefix};
            capability.Icons.AddAll(Icons);
            capability.Verbs.AddAll(Verbs);
            return capability;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(UrlProtocol other)
        {
            if (other == null) return false;

            return base.Equals(other) && other.Prefix == Prefix;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(UrlProtocol) && Equals((UrlProtocol)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Prefix ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
