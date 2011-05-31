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

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// Integrates an application into a file manager's context menu.
    /// </summary>
    /// <seealso cref="ZeroInstall.Model.Capabilities.ContextMenu"/>
    [XmlType("context-menu", Namespace = XmlNamespace)]
    public class ContextMenu : AccessPoint, IEquatable<ContextMenu>
    {
        #region Properties
        // ToDo
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "ContextMenu". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ContextMenu");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint CloneAccessPoint()
        {
            return new ContextMenu();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ContextMenu other)
        {
            if (other == null) return false;

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ContextMenu) && Equals((ContextMenu)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return 0;
            }
        }
        #endregion
    }
}
