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
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Creates a shortcut for an application in the "Send to" menu.
    /// </summary>
    [XmlType("send-to", Namespace = AppList.XmlNamespace)]
    public class SendTo : IconAccessPoint, IEquatable<SendTo>
    {
        #region Conflict ID
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            return new[] {"send-to:" + Name};
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        private string GetWindowsShortcutPath()
        {
            if (string.IsNullOrEmpty(Name) || Name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, Name));

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), Name + ".lnk");
        }

        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (WindowsUtils.IsWindows && !systemWide)
                Windows.ShortcutManager.CreateShortcut(GetWindowsShortcutPath(), new InterfaceFeed(appEntry.InterfaceID, feed), Command, false, handler);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool systemWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            if (WindowsUtils.IsWindows && !systemWide)
            {
                string filePath = GetWindowsShortcutPath();
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "SendTo". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("SendTo");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new SendTo {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Command = Command, Name = Name};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(SendTo other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(SendTo) && Equals((SendTo)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
