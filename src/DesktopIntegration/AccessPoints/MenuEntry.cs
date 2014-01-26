﻿/*
 * Copyright 2010-2014 Bastian Eicher
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
using Common.Utils;
using ZeroInstall.Store.Model;

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
        /// The category or folder in the menu to add the entry to. Leave empty for top-level entry.
        /// </summary>
        [Description("The category or folder in the menu to add the entry to. Leave empty for top-level entry.")]
        [XmlAttribute("category")]
        public string Category { get; set; }
        #endregion

        //--------------------//

        #region Conflict ID
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            return new[] {"menu:" + Category + @"\" + Name};
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var target = new InterfaceFeed(appEntry.InterfaceID, feed);
            if (WindowsUtils.IsWindows) Windows.Shortcut.Create(this, target, handler, machineWide);
            else if (MonoUtils.IsUnix) Unix.FreeDesktop.Create(this, target, machineWide, handler);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            if (WindowsUtils.IsWindows) Windows.Shortcut.Remove(this, machineWide);
            else if (MonoUtils.IsUnix) Unix.FreeDesktop.Remove(this, machineWide);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "MenuEntry: Name". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("MenuEntry: {0}", Name);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new MenuEntry {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Command = Command, Name = Name, Category = Category};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(MenuEntry other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Category == Category;
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
