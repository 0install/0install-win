/*
 * Copyright 2010-2013 Bastian Eicher
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
    /// Represents an application's listing in the Windows Games Explorer.
    /// </summary>
    [XmlType("games-explorer", Namespace = XmlNamespace)]
    public sealed class GamesExplorer : Capability, IEquatable<GamesExplorer>
    {
        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsMachineWideOnly { get { return false; } }

        // ToDo

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs { get { return new[] {"game:" + ID}; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "GamesExplorer". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("GamesExplorer");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            return new GamesExplorer {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(GamesExplorer other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GamesExplorer && Equals((GamesExplorer)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
