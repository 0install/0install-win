/*
 * Copyright 2011 Bastian Eicher
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

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Creates a desktop shortcut for launching the application.
    /// </summary>
    [XmlType("desktop-shortcut", Namespace = XmlNamespace)]
    public class DesktopShortcut : Integration, IEquatable<DesktopShortcut>
    {
        #region Properties
        /// <summary>
        /// The name of the desktop shortcut.
        /// </summary>
        [Description("The name of the desktop shortcut.")]
        [XmlAttribute("name")]
        public string Name { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the integration in the form "DesktopShortcut: Name". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "DesktopShortcut: " + Name;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="DesktopShortcut"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="DesktopShortcut"/>.</returns>
        public override Integration CloneIntegration()
        {
            return new DesktopShortcut {Name = Name};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(DesktopShortcut other)
        {
            if (other == null) return false;

            return other.Name == Name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(DesktopShortcut) && Equals((DesktopShortcut)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name != null ? Name.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
