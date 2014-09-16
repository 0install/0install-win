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
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Helper methods for creating <see cref="ConflictData"/> lists.
    /// </summary>
    public static class ConflictDataUtils
    {
        /// <summary>
        /// Checks new <see cref="AccessPoint"/> candidates for conflicts with existing ones.
        /// </summary>
        /// <param name="appList">The <see cref="AppList"/> containing the existing <see cref="AccessPoint"/>s.</param>
        /// <param name="accessPoints">The set of <see cref="AccessPoint"/>s candidates to check.</param>
        /// <param name="appEntry">The <see cref="AppEntry"/> the <paramref name="accessPoints"/> are intended for.</param>
        /// <exception cref="KeyNotFoundException">An <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="ConflictException">One or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        public static void CheckForConflicts(this AppList appList, IEnumerable<AccessPoint> accessPoints, AppEntry appEntry)
        {
            #region Sanity checks
            if (appList == null) throw new ArgumentNullException("appList");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            var newConflictData = accessPoints.GetConflictData(appEntry);
            var existingConflictData = appList.Entries.GetConflictData();

            foreach (var pair in newConflictData)
            {
                ConflictData existingEntry, newEntry = pair.Value;
                if (existingConflictData.TryGetValue(pair.Key, out existingEntry))
                {
                    // Ignore conflicts that are actually just re-applications of existing access points
                    if (existingEntry != newEntry) throw ConflictException.NewConflict(existingEntry, newEntry);
                }
            }
        }

        /// <summary>
        /// Returns all <see cref="ConflictData"/>s for a set of new <see cref="AccessPoint"/> candidates.
        /// </summary>
        /// <param name="accessPoints">The set of <see cref="AccessPoint"/>s candidates to build the list for.</param>
        /// <param name="appEntry">The <see cref="AppEntry"/> the <paramref name="accessPoints"/> are intended for.</param>
        /// <returns>A dictionary of <see cref="AccessPoint.GetConflictIDs"/> mapping to the according <see cref="ConflictData"/>.</returns>
        /// <exception cref="ConflictException">There are inner conflicts within <paramref name="accessPoints"/>.</exception>
        /// <seealso cref="AccessPoint.GetConflictIDs"/>
        public static IDictionary<string, ConflictData> GetConflictData(this IEnumerable<AccessPoint> accessPoints, AppEntry appEntry)
        {
            #region Sanity checks
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            var newConflictIDs = new Dictionary<string, ConflictData>();
            foreach (var accessPoint in accessPoints)
            {
                foreach (string conflictID in accessPoint.GetConflictIDs(appEntry))
                {
                    var conflictData = new ConflictData(accessPoint, appEntry);
                    try
                    {
                        newConflictIDs.Add(conflictID, conflictData);
                    }
                        #region Error handling
                    catch (ArgumentException)
                    {
                        throw ConflictException.InnerConflict(conflictData, newConflictIDs[conflictID]);
                    }
                    #endregion
                }
            }
            return newConflictIDs;
        }

        /// <summary>
        /// Returns all <see cref="ConflictData"/>s for a set of existing <see cref="AppEntry"/>s.
        /// </summary>
        /// <param name="appEntries">The <see cref="AppEntry"/>s to build the list for.</param>
        /// <returns>A dictionary of <see cref="AccessPoint.GetConflictIDs"/> mapping to the according <see cref="ConflictData"/>.</returns>
        /// <exception cref="ConflictException">There are preexisting conflicts within <paramref name="appEntries"/>.</exception>
        /// <seealso cref="AccessPoint.GetConflictIDs"/>
        public static IDictionary<string, ConflictData> GetConflictData(this IEnumerable<AppEntry> appEntries)
        {
            #region Sanity checks
            if (appEntries == null) throw new ArgumentNullException("appEntries");
            #endregion

            var conflictIDs = new Dictionary<string, ConflictData>();
            foreach (var appEntry in appEntries)
            {
                if (appEntry.AccessPoints == null) continue;
                foreach (var accessPoint in appEntry.AccessPoints.Entries)
                {
                    foreach (string conflictID in accessPoint.GetConflictIDs(appEntry))
                    {
                        var conflictData = new ConflictData(accessPoint, appEntry);
                        try
                        {
                            conflictIDs.Add(conflictID, conflictData);
                        }
                            #region Error handling
                        catch (ArgumentException)
                        {
                            throw ConflictException.ExistingConflict(conflictIDs[conflictID], conflictData);
                        }
                        #endregion
                    }
                }
            }
            return conflictIDs;
        }
    }
}
