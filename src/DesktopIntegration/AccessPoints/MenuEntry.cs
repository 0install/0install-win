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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Common.Tasks;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Creates an entry for an application in the user's application menu (i.e. Windows start menu, GNOME application menu, etc.).
    /// </summary>
    [XmlType("menu-entry", Namespace = AppList.XmlNamespace)]
    public class MenuEntry : IconAccessPoint, IEquatable<MenuEntry>
    {
        #region Properties
        /// <summary>
        /// The category or folder in the menu to add the entry to; <see langword="null"/> for top-level entry.
        /// </summary>
        [Description("The category or folder in the menu to add the entry to; null for top-level entry.")]
        [XmlAttribute("category")]
        public string Category { get; set; }
        #endregion

        //--------------------//

        #region Collision
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            return new[] { "menu:" + Category + @"\" + Name };
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, InterfaceFeed target, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // ToDo: Implement
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool systemWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            // ToDo: Implement
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "MenuEntry". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("MenuEntry");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint CloneAccessPoint()
        {
            return new MenuEntry {Command = Command, Name = Name, Category = Category};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(MenuEntry other)
        {
            if (other == null) return false;

            return base.Equals(other) &&
                other.Category == Category;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(MenuEntry) && Equals((MenuEntry)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Category ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
