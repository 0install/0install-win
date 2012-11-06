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
using System.Linq;
using System.Xml.Serialization;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Creates a shortcut for an application in the Quick Launch bar.
    /// </summary>
    [XmlType("quick-launch", Namespace = AppList.XmlNamespace)]
    public class QuickLaunch : IconAccessPoint, IEquatable<QuickLaunch>
    {
        #region Conflict ID
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            return new[] {"quick-launch:" + Name};
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        private string GetWindowsShortcutPath()
        {
            if (string.IsNullOrEmpty(Name) || Name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, Name));

            return new[] {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Internet Explorer", "Quick Launch", Name + ".lnk"}.Aggregate(Path.Combine);
        }

        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, bool machineWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (WindowsUtils.IsWindows && !machineWide)
                Windows.ShortcutManager.CreateShortcut(GetWindowsShortcutPath(), new InterfaceFeed(appEntry.InterfaceID, feed), Command, false, handler);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            if (WindowsUtils.IsWindows && !machineWide)
                if (File.Exists(GetWindowsShortcutPath())) File.Delete(GetWindowsShortcutPath());
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "QuickLaunch". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("QuickLaunch");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new QuickLaunch {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Command = Command, Name = Name};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(QuickLaunch other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(QuickLaunch) && Equals((QuickLaunch)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
