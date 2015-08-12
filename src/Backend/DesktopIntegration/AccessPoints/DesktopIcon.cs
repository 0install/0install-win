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
using System.Xml.Serialization;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Creates an icon for an application on the user's desktop.
    /// </summary>
    [XmlType("desktop-icon", Namespace = AppList.XmlNamespace)]
    public class DesktopIcon : IconAccessPoint, IEquatable<DesktopIcon>
    {
        #region Constants
        /// <summary>
        /// The name of this category of <see cref="AccessPoint"/>s as used by command-line interfaces.
        /// </summary>
        public const string CategoryName = "desktop";
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            return new[] {"desktop:" + Name};
        }

        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            var target = new FeedTarget(appEntry.InterfaceUri, feed);
            if (WindowsUtils.IsWindows) Windows.Shortcut.Create(this, target, handler, machineWide);
            else if (UnixUtils.IsUnix) Unix.FreeDesktop.Create(this, target, machineWide, handler);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            #endregion

            if (WindowsUtils.IsWindows) Windows.Shortcut.Remove(this, machineWide);
            else if (UnixUtils.IsUnix) Unix.FreeDesktop.Remove(this, machineWide);
        }

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new DesktopIcon {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Name = Name, Command = Command};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(DesktopIcon other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj.GetType() == typeof(DesktopIcon) && Equals((DesktopIcon)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
