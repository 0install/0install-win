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
using System.Net;
using Common;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages desktop integration via <see cref="AccessPoint"/>s.
    /// </summary>
    public class IntegrationManager
    {
        #region Variables
        /// <summary>Apply operations system-wide instead of just for the current user.</summary>
        protected readonly bool SystemWide;

        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        protected readonly string AppListPath;
        #endregion

        #region Properties
        /// <summary>
        /// Stores a list of applications and their desktop integrations. Do not modify this externally! Use this class' methods instead.
        /// </summary>
        public AppList AppList { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new integration manager using a custom <see cref="DesktopIntegration.AppList"/>.
        /// </summary>
        /// <param name="systemWide">Apply operations system-wide instead of just for the current user.</param>
        /// <param name="appListPath">The storage location of the <see cref="AppList"/> file.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool systemWide, string appListPath)
        {
            SystemWide = systemWide;
            AppListPath = appListPath;

            if (File.Exists(AppListPath)) AppList = AppList.Load(AppListPath);
            else
            {
                AppList = new AppList();
                AppList.Save(AppListPath);
            }
        }

        /// <summary>
        /// Creates a new integration manager using the default <see cref="DesktopIntegration.AppList"/>.
        /// </summary>
        /// <param name="systemWide">Apply operations system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool systemWide) : this(systemWide, GetAppListPath(systemWide))
        {}

        private static string GetAppListPath(bool systemWide)
        {
            return systemWide ?
                // Note: Ignore Portable mode when operating system-wide
                Path.Combine(Locations.GetIntegrationDirPath("0install.net", true, "desktop-integration"), "app-list.xml")
                : Locations.GetSaveConfigPath("0install.net", true, "desktop-integration", "app-list.xml");
        }
        #endregion

        //--------------------//

        #region Apps
        /// <summary>
        /// Adds an application to the application list.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void AddApp(InterfaceFeed target)
        {
            // Prevent double entries
            AppEntry existingEntry = FindAppEntry(target.InterfaceID);
            if (existingEntry != null) throw new InvalidOperationException(string.Format(Resources.AppAlreadyInList, existingEntry.Name));

            AppList.Entries.Add(BuildAppEntry(target));
            AppList.Save(AppListPath);
        }

        /// <summary>
        /// Removes an application from the application list.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        public void RemoveApp(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));

            if (appEntry.AccessPoints != null)
            {
                // Unapply any remaining access points
                foreach (var accessPoint in appEntry.AccessPoints.Entries)
                    accessPoint.Unapply(appEntry, SystemWide);
                if (WindowsUtils.IsWindows) WindowsUtils.NotifyAssocChanged();
            }

            AppList.Entries.Remove(appEntry);
            AppList.Save(AppListPath);
        }
        #endregion

        #region AccessPoints
        /// <summary>
        /// Applies <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="accessPoints">The <see cref="AccessPoint"/>s to apply.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        public void AddAccessPoints(InterfaceFeed target, IEnumerable<AccessPoint> accessPoints, ITaskHandler handler)
        {
            #region Sanity checks
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Implicitly add application to list if missing
            AppEntry appEntry = FindAppEntry(target.InterfaceID);
            if (appEntry == null)
            {
                appEntry = BuildAppEntry(target);
                AppList.Entries.Add(appEntry);
            }
            if (appEntry.AccessPoints == null) appEntry.AccessPoints = new AccessPointList();

            var filteredAccessPoints = GetFilteredAccessPoints(accessPoints, appEntry);

            // Apply the access points
            foreach (var accessPoint in filteredAccessPoints)
                accessPoint.Apply(appEntry, target, SystemWide, handler);
            if (WindowsUtils.IsWindows) WindowsUtils.NotifyAssocChanged();

            // Add the access points to the AppList
            foreach (var accessPoint in filteredAccessPoints)
            {
                accessPoint.Timestamp = FileUtils.ToUnixTime(DateTime.UtcNow);
                appEntry.AccessPoints.Entries.Add(accessPoint);
            }
            AppList.Save(AppListPath);
        }

        /// <summary>
        /// Checks new <see cref="AccessPoint"/>s for conflicts with existing ones and drops duplicates of existing ones.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        private IEnumerable<AccessPoint> GetFilteredAccessPoints(IEnumerable<AccessPoint> accessPoints, AppEntry appEntry)
        {
            var filteredAccessPoints = new LinkedList<AccessPoint>();

            var conflictIDs = AppList.GetConflictIDs();
            foreach (var accessPoint in accessPoints)
            {
                // Drop duplicates of existing access points
                if (appEntry.AccessPoints.Entries.Contains(accessPoint)) continue;

                // Check for conflicts with existing access points
                foreach (string conflictID in accessPoint.GetConflictIDs(appEntry))
                {
                    ConflictData conflictData;
                    if (conflictIDs.TryGetValue(conflictID, out conflictData))
                        throw new InvalidOperationException(string.Format(Resources.AccessPointConflict, conflictData.AccessPoint, conflictData.AppEntry, accessPoint, appEntry));
                }

                filteredAccessPoints.AddLast(accessPoint);
            }

            // ToDo: Check for inner conflicts

            return filteredAccessPoints;
        }

        /// <summary>
        /// Removes already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="accessPoints">The <see cref="AccessPoint"/>s to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        public void RemoveAccessPoints(string interfaceID, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            #endregion

            // Handle missing entries
            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));
            if (appEntry.AccessPoints == null) return;

            // Unapply the access points
            foreach (var accessPoint in accessPoints)
                accessPoint.Unapply(appEntry, SystemWide);
            if (WindowsUtils.IsWindows) WindowsUtils.NotifyAssocChanged();

            // Remove the access points from the AppList
            appEntry.AccessPoints.Entries.RemoveAll(accessPoints);
            AppList.Save(AppListPath);
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
        /// <param name="target">The application being integrated.</param>
        /// <returns>The newly created <see cref="AppEntry"/>.</returns>
        protected static AppEntry BuildAppEntry(InterfaceFeed target)
        {
            var appEntry = new AppEntry {InterfaceID = target.InterfaceID, Name = target.Feed.Name, Timestamp = FileUtils.ToUnixTime(DateTime.UtcNow)};
            foreach (var capabilityList in target.Feed.CapabilityLists)
                appEntry.CapabilityLists.Add(capabilityList.CloneCapabilityList());
            return appEntry;
        }
        #endregion
    }
}
