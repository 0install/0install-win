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
using System.IO;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Allows detailed management of desktop integration, down to individual <see cref="AccessPoint"/>s.
    /// </summary>
    public class DetailedIntegrationManagerBase : IntegrationManagerBase
    {
        #region Constructor
        /// <inheritdoc/>
        public DetailedIntegrationManagerBase(bool systemWide) : base(systemWide)
        {}
        #endregion

        //--------------------//

        /// <summary>
        /// Applies a single <see cref="AccessPoint"/> for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="feed">The <see cref="Feed"/> for the application to perform the operation on.</param>
        /// <param name="accessPoint">The <see cref="AccessPoint"/> to apply.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void AddAccessPoint(string interfaceID, Feed feed, AccessPoint accessPoint)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            if (accessPoint == null) throw new ArgumentNullException("accessPoint");
            #endregion

            // Implicitly add application to list if missing
            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null)
            {
                appEntry = BuildAppEntry(interfaceID, feed);
                AppList.Entries.Add(appEntry);
            }
            if (appEntry.AccessPoints == null) appEntry.AccessPoints = new AccessPointList();

            AddAccessPoints(appEntry, feed, new[] {accessPoint});
        }

        /// <summary>
        /// Removes a single already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="accessPoint">The <see cref="AccessPoint"/> to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown in the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void RemoveAccessPoint(string interfaceID, AccessPoint accessPoint)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (accessPoint == null) throw new ArgumentNullException("accessPoint");
            #endregion

            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));
            if (appEntry.AccessPoints == null) return;

            RemoveAccessPoints(appEntry, new[] {accessPoint});
        }
    }
}
