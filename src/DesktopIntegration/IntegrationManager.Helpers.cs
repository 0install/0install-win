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
using Common.Tasks;
using Common.Utils;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    // Contains backend helper methods usable by sub-classes
    public partial class IntegrationManager
    {
        #region AppEntry
        /// <summary>
        /// Removes an <see cref="AppEntry"/> from the <see cref="AppList"/> while unapplying any remaining <see cref="AccessPoint"/>s.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        protected void RemoveAppEntry(AppEntry appEntry)
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
            var appEntry = new AppEntry {InterfaceID = target.InterfaceID, Name = target.Feed.Name, Timestamp = DateTime.UtcNow};
            foreach (var capabilityList in target.Feed.CapabilityLists)
                appEntry.CapabilityLists.Add(capabilityList.CloneCapabilityList());
            return appEntry;
        }
        #endregion

        #region AccessPoint
        /// <summary>
        /// Adds and applies <see cref="AccessPoint"/>s.
        /// </summary>
        /// <param name="accessPoints">The access points to unapply.</param>
        /// <param name="appEntry">The <see cref="AppEntry"/> containing the <paramref name="accessPoints"/>.</param>
        /// <param name="target">The application being integrated.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        protected void AddAccessPoints(IEnumerable<AccessPoint> accessPoints, AppEntry appEntry, InterfaceFeed target, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (appEntry.AccessPoints == null) appEntry.AccessPoints = new AccessPointList();

            CheckForConflicts(accessPoints, appEntry);

            EnumerableUtils.ApplyWithRollback(accessPoints,
                accessPoint => accessPoint.Apply(appEntry, target, SystemWide, handler),
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
        /// Removes and unapplies already applied <see cref="AccessPoint"/>s.
        /// </summary>
        /// <param name="accessPoints">The access points to unapply.</param>
        /// <param name="appEntry">The <see cref="AppEntry"/> containing the <paramref name="accessPoints"/>.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        protected void RemoveAccessPoints(IEnumerable<AccessPoint> accessPoints, AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            #endregion

            foreach (var accessPoint in accessPoints)
                accessPoint.Unapply(appEntry, SystemWide);

            // Remove the access points from the AppList
            appEntry.AccessPoints.Entries.RemoveAll(accessPoints);
            appEntry.Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks new <see cref="AccessPoint"/>s for conflicts with existing ones.
        /// </summary>
        /// <param name="accessPoints">The access point to check for conflicts.</param>
        /// <param name="appEntry">The <see cref="AppEntry"/> the <paramref name="accessPoints"/> will be added to.</param>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        private void CheckForConflicts(IEnumerable<AccessPoint> accessPoints, AppEntry appEntry)
        {
            #region Sanity checks
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
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
