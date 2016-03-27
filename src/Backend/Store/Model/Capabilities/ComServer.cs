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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// An application's ability to act as a COM server.
    /// </summary>
    [Description("An application's ability to act as a COM server.")]
    [Serializable, XmlRoot("com-server", Namespace = CapabilityList.XmlNamespace), XmlType("com-server", Namespace = CapabilityList.XmlNamespace)]
    public sealed class ComServer : Capability, IEquatable<ComServer>
    {
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsMachineWideOnly { get { return false; } }

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs { get { return new[] {"classes:" + ID}; } }

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "-". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "-";
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            return new ComServer {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ComServer other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ComServer && Equals((ComServer)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
