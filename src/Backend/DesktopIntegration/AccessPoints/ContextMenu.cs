/*
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
using System.Xml.Serialization;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Integrates an application into a file manager's context menu.
    /// </summary>
    /// <seealso cref="ZeroInstall.Store.Model.Capabilities.ContextMenu"/>
    [XmlType("context-menu", Namespace = AppList.XmlNamespace)]
    public class ContextMenu : DefaultAccessPoint, IEquatable<ContextMenu>
    {
        #region Conflict ID
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            var capability = appEntry.GetCapability<Store.Model.Capabilities.ContextMenu>(Capability);
            return new[] {"context-menu-" + capability.Target + ":" + capability.ID + @"\" + capability.Verb};
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

            var capability = appEntry.GetCapability<Store.Model.Capabilities.ContextMenu>(Capability);
            if (capability == null) return;

            var target = new InterfaceFeed(appEntry.InterfaceID, feed);
            if (WindowsUtils.IsWindows) Windows.ContextMenu.Apply(target, capability, machineWide, handler);
            else if (UnixUtils.IsUnix) Unix.ContextMenu.Apply(target, capability, machineWide, handler);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            var capability = appEntry.GetCapability<Store.Model.Capabilities.ContextMenu>(Capability);
            if (capability == null) return;

            if (WindowsUtils.IsWindows) Windows.ContextMenu.Remove(capability, machineWide);
            else if (UnixUtils.IsUnix) Unix.ContextMenu.Remove(capability, machineWide);
        }
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
        public override AccessPoint Clone()
        {
            return new ContextMenu {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Capability = Capability};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ContextMenu other)
        {
            return base.Equals(other);
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
            return base.GetHashCode();
        }
        #endregion
    }
}
