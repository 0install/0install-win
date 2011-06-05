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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's listing in the Windows Games Explorer.
    /// </summary>
    [XmlType("games-explorer", Namespace = XmlNamespace)]
    public class GamesExplorer : Capability, IEquatable<GamesExplorer>
    {
        #region Properties
        /// <inheritdoc/>
        public override bool MachineWideOnly { get { return true; } }

        // ToDo
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
        public override Capability CloneCapability()
        {
            return new GamesExplorer {ID = ID};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(GamesExplorer other)
        {
            if (other == null) return false;

            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(GamesExplorer) && Equals((GamesExplorer)obj);
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
