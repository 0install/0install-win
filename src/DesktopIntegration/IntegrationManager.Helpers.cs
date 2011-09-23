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
using Common.Collections;
using Common.Utils;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    // Contains backend helper methods usable by sub-classes
    public partial class IntegrationManager
    {
        #region Apps
        /// <summary>
        /// Creates a new <see cref="AppEntry"/> and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application to remove.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <returns>The newly created application entry (already added to <see cref="AppList"/>).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        protected AppEntry AddAppHelper(string interfaceID, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            // Prevent double entries
            if (AppList.ContainsEntry(interfaceID)) throw new InvalidOperationException(string.Format(Resources.AppAlreadyInList, feed.Name));

            // Get basic metadata and copy of capabilities from feed
            var appEntry = new AppEntry {InterfaceID = interfaceID, Name = feed.Name, Timestamp = DateTime.UtcNow};
            appEntry.CapabilityLists.AddAll(feed.CapabilityLists.Map(list => list.CloneCapabilityList()));

            AppList.Entries.Add(appEntry);
            return appEntry;
        }

        /// <summary>
        /// Removes an <see cref="AppEntry"/> from the <see cref="AppList"/> while unapplying any remaining <see cref="AccessPoint"/>s.
        /// </summary>
        /// <param name="appEntry">The application to remove.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to <see cref="Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        protected void RemoveAppHelper(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            if (appEntry.AccessPoints != null)
            {
                // Unapply any remaining access points
                foreach (var accessPoint in appEntry.AccessPoints.Entries)
                    accessPoint.Unapply(appEntry, SystemWide);
            }

            AppList.Entries.Remove(appEntry);
        }

        /// <summary>
        /// Updates an <see cref="AppEntry"/> with new metadata and capabilities from a <see cref="Feed"/>. This may unapply and remove some existing <see cref="AccessPoint"/>s.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to <see cref="Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <param name="appEntry">The application entry to update.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        private void UpdateAppHelper(AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            var toReapply = new List<AccessPoint>();
            if (appEntry.AccessPoints != null)
            {
                foreach (var accessPoint in appEntry.AccessPoints.Entries)
                {
                    if (accessPoint is DefaultAccessPoint || accessPoint is CapabilityRegistration)
                        toReapply.Add(accessPoint);
                }
            }
            RemoveAccessPointsHelper(appEntry, toReapply);

            // Update metadata and capabilities
            appEntry.Name = feed.Name;
            appEntry.CapabilityLists.Clear();
            appEntry.CapabilityLists.AddAll(feed.CapabilityLists.Map(list => list.CloneCapabilityList()));

            foreach (var accessPoint in toReapply)
            {
                try
                {
                    AddAccessPointsHelper(appEntry, feed, new[] {accessPoint});
                }
                catch (KeyNotFoundException)
                {
                    Log.Warn(string.Format("Access point '{0}' no longer compatible with interface '{1}'.", accessPoint, appEntry.InterfaceID));
                }
            }

            appEntry.Timestamp = DateTime.UtcNow;
        }
        #endregion

        #region AccessPoint
        /// <summary>
        /// Applies <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        /// <param name="accessPoints">The access points to apply.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to <see cref="Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        protected void AddAccessPointsHelper(AppEntry appEntry, Feed feed, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            #endregion

            if (appEntry.AccessPoints == null) appEntry.AccessPoints = new AccessPointList();

            CheckForConflicts(appEntry, accessPoints);

            EnumerableUtils.ApplyWithRollback(accessPoints,
                accessPoint => accessPoint.Apply(appEntry, feed, SystemWide, _handler),
                accessPoint =>
                {
                    // Don't perform rollback if the access point was already applied previously and this was only a refresh
                    if (!appEntry.AccessPoints.Entries.Contains(accessPoint))
                        accessPoint.Unapply(appEntry, SystemWide);
                });

            // Add the access points to the AppList
            foreach (var accessPoint in accessPoints)
                appEntry.AccessPoints.Entries.UpdateOrAdd(accessPoint); // Replace pre-existing entries
            appEntry.Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The <see cref="AppEntry"/> containing the <paramref name="accessPoints"/>.</param>
        /// <param name="accessPoints">The access points to unapply.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to <see cref="Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        protected void RemoveAccessPointsHelper(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            #endregion

            if (appEntry.AccessPoints == null) return;

            foreach (var accessPoint in accessPoints)
                accessPoint.Unapply(appEntry, SystemWide);

            // Remove the access points from the AppList
            appEntry.AccessPoints.Entries.RemoveAll(accessPoints);
            appEntry.Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks new <see cref="AccessPoint"/>s for conflicts with existing ones.
        /// </summary>
        /// <param name="appEntry">The <see cref="AppEntry"/> the <paramref name="accessPoints"/> will be added to.</param>
        /// <param name="accessPoints">The access point to check for conflicts.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to <see cref="Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        private void CheckForConflicts(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            #endregion

            var conflictIDs = AppList.GetConflictIDs();
            foreach (var accessPoint in accessPoints)
            {
                // Check for conflicts with existing access points
                foreach (string conflictID in accessPoint.GetConflictIDs(appEntry))
                {
                    ConflictData conflictData;
                    if (conflictIDs.TryGetValue(conflictID, out conflictData))
                    {
                        // Ignore conflicts that are actually just re-applications of existing access points
                        if (appEntry.InterfaceID != conflictData.AppEntry.InterfaceID || !accessPoint.Equals(conflictData.AccessPoint))
                            throw new InvalidOperationException(string.Format(Resources.AccessPointConflict, conflictData.AccessPoint, conflictData.AppEntry, accessPoint, appEntry));
                    }
                }
            }

            // ToDo: Check for system conflicts
            // ToDo: Check for inner conflicts
        }
        #endregion

        #region Complete
        /// <summary>
        /// To be called after integration operations have been completed to inform the desktop environment and save the <see cref="AppList"/>.
        /// </summary>
        protected void Complete()
        {
            WindowsUtils.NotifyAssocChanged();
            WindowsUtils.NotifyEnvironmentChanged();

            AppList.Save(AppListPath);
        }
        #endregion
    }
}
