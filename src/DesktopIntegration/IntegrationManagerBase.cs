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
using System.IO;
using Common.Storage;
using Common.Utils;
using ZeroInstall.DesktopIntegration.Model;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Common base class for classes that manage desktop integration.
    /// </summary>
    public abstract class IntegrationManagerBase
    {
        #region Variables
        /// <summary>Apply operations system-wide instead of just for the current user.</summary>
        private readonly bool _global;

        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        private readonly string _appListPath;
        #endregion

        #region Properties
        /// <summary>
        /// Stores a list of applications and their desktop integrations. Do not modify this externally! Use this class' methods instead.
        /// </summary>
        public AppList AppList { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new integration manager.
        /// </summary>
        /// <param name="global">Apply operations system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or wirte access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        protected IntegrationManagerBase(bool global)
        {
            _global = global;

            _appListPath = global
                ? FileUtils.PathCombine(Locations.SystemConfigDirs.Split(Path.PathSeparator)[0], "0install.net", "desktop-integration", "myapps.xml")
                : Locations.GetSaveConfigPath("0install.net", Path.Combine("desktop-integration", "myapps.xml"), false);

            if (File.Exists(_appListPath)) AppList = AppList.Load(_appListPath);
            else
            {
                AppList = new AppList();
                AppList.Save(_appListPath);
            }
        }
        #endregion

        //--------------------//

        #region Apps
        /// <summary>
        /// Adds an application to the application list.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to add.</param>
        /// <param name="feed">The <see cref="Feed"/> for the application to add.</param>
        /// <exception cref="InvalidOperationException">Thrown in the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void AddApp(string interfaceID, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            // Prevent double entries
            AppEntry existingEntry = FindAppEntry(interfaceID);
            if (existingEntry != null) throw new InvalidOperationException(string.Format(Resources.AppAlreadyInList, existingEntry.Name));

            AppList.Entries.Add(BuildAppEntry(interfaceID, feed));
            AppList.Save(_appListPath);
        }

        /// <summary>
        /// Removes an application from the application list.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the remove.</param>
        /// <exception cref="InvalidOperationException">Thrown in the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void RemoveApp(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));

            // Remove all AccessPoints first
            RemoveAccessPoints(appEntry, appEntry.AccessPoints.Entries);

            AppList.Entries.Remove(appEntry);
            AppList.Save(_appListPath);
        }
        #endregion

        #region AccessPoint handling
        /// <summary>
        /// Adds and applies <see cref="AccessPoint"/>s to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="appEntry">The <see cref="AppEntry"/> to add the <see cref="AccessPoint"/>s for.</param>
        /// <param name="feed">The <see cref="Feed"/> for the application the <see cref="AccessPoint"/>s refer to.</param>
        /// <param name="accessPoints">The <see cref="AccessPoint"/>s to add.</param>
        protected void AddAccessPoints(AppEntry appEntry, Feed feed, IEnumerable<AccessPoint> accessPoints)
        {
            foreach (var accessPoint in accessPoints)
            {
                if (!appEntry.AccessPoints.Entries.Contains(accessPoint))
                    appEntry.AccessPoints.Entries.Add(accessPoint);
            }

            AppList.Save(_appListPath);

            //foreach (var accessPoint in accessPoints)
            //    accessPoint.Apply(appEntry, feed, _global);
            WindowsUtils.NotifyAssocChanged();
        }

        /// <summary>
        /// Unapplies and removes  <see cref="AccessPoint"/>s from the <see cref="AppList"/>.
        /// </summary>
        /// <param name="appEntry">The <see cref="AppEntry"/> to remove the <see cref="AccessPoint"/>s from.</param>
        /// <param name="accessPoints">The <see cref="AccessPoint"/>s to remove.</param>
        protected void RemoveAccessPoints(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints)
        {
            appEntry.AccessPoints.Entries.RemoveAll(accessPoints);

            //foreach (var accessPoint in accessPoints)
            //    accessPoint.Unapply(appEntry, _global);
            WindowsUtils.NotifyAssocChanged();

            AppList.Save(_appListPath);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Tries to fo find an existing <see cref="AppEntry"/> in <see cref="AppList"/>.
        /// </summary>
        /// <param name="interfaceID">The <see cref="AppEntry.InterfaceID"/> to look for.</param>
        /// <returns>The first matching <see cref="AppEntry"/> that was found; <see langword="null"/> if no match was found.</returns>
        protected AppEntry FindAppEntry(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            AppEntry appEntry;
            AppList.Entries.Find(entry => entry.InterfaceID == interfaceID, out appEntry);
            return appEntry;
        }

        /// <summary>
        /// Creates a new <see cref="AppEntry"/>.
        /// </summary>
        /// <param name="interfaceID">The <see cref="AppEntry.InterfaceID"/> to set.</param>
        /// <param name="feed">The <see cref="Feed"/> to get data such as the name an <see cref="Capabilities.Capability"/>s from.</param>
        /// <returns>The newly created <see cref="AppEntry"/>.</returns>
        protected static AppEntry BuildAppEntry(string interfaceID, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            var appEntry = new AppEntry { InterfaceID = interfaceID, Name = feed.Name };
            foreach (var capabilityList in feed.CapabilityLists)
                appEntry.CapabilityLists.Add(capabilityList.CloneCapabilityList());
            return appEntry;
        }
        #endregion
    }
}
