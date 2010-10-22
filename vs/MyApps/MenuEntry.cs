/*
 * Copyright 2010 Bastian Eicher
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

namespace ZeroInstall.MyApps
{
    /// <summary>
    /// Lists an application as an entry in the desktop environment's main menu (freedesktop.org, Windows start menu, etc.).
    /// </summary>
    [XmlType("menu-entry", Namespace = "http://0install.de/schema/my-apps/app-list")]
    public class MenuEntry : Integration, IEquatable<MenuEntry>
    {
        #region Properties
        /// <summary>
        /// The name of the menu entry.
        /// </summary>
        [Description("The name of the menu entry.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// A category as defined by the freedesktop.org menu specification.
        /// </summary>
        [Description("A category as defined by the freedesktop.org menu specification.")]
        [XmlAttribute("category")]
        public string Category { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the integration in the form "MenuEntry: Name (Category)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("MenuEntry: {0} ({1})", Name, Category);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="MenuEntry"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="MenuEntry"/>.</returns>
        public override Integration CloneIntegration()
        {
            return new MenuEntry {Name = Name, Category = Category};
        }
        #endregion

        #region Equality
        public bool Equals(MenuEntry other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Name == Name || other.Category == Category;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(MenuEntry) && Equals((MenuEntry)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Category != null ? Category.GetHashCode() : 0);
            }
        }
        #endregion
    }
}
