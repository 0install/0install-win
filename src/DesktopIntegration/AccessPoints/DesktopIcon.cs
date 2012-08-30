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
using System.IO;
using System.Xml.Serialization;
using Common.Tasks;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Creates an icon for an application on the user's desktop.
    /// </summary>
    [XmlType("desktop-icon", Namespace = AppList.XmlNamespace)]
    public class DesktopIcon : IconAccessPoint, IEquatable<DesktopIcon>
    {
        #region Conflict ID
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            return new[] {"desktop:" + Name};
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        private string GetWindowsShortcutPath(bool systemWide)
        {
            if (string.IsNullOrEmpty(Name) || Name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, Name));

            string desktopDir = systemWide
                ? Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "Common Desktop", "").ToString()
                : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            return Path.Combine(desktopDir, Name + ".lnk");
        }

        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (WindowsUtils.IsWindows)
                Windows.ShortcutManager.CreateShortcut(GetWindowsShortcutPath(systemWide), new InterfaceFeed(appEntry.InterfaceID, feed), Command, systemWide, handler);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool systemWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            if (WindowsUtils.IsWindows)
            {
                string filePath = GetWindowsShortcutPath(systemWide);
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "DesktopIcon: Name". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("DesktopIcon: {0}", Name);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new DesktopIcon {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Command = Command, Name = Name };
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
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
