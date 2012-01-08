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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's ability to act as a COM server.
    /// </summary>
    [XmlType("com-server", Namespace = XmlNamespace)]
    public sealed class ComServer : Capability, IEquatable<ComServer>
    {
        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsSystemWideOnly { get { return false; } }

        // ToDo

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs { get { return new[] {"classes:" + ID}; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "ComServer". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ComServer");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            return new ComServer {ID = ID};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ComServer other)
        {
            if (other == null) return false;

            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ComServer) && Equals((ComServer)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
