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
using ZeroInstall.DesktopIntegration.Model;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;
using Capability = ZeroInstall.Model.Capabilities.Capability;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Allows category-based management of desktop integration, grouping <see cref="AccessPoint"/>s into broad categories.
    /// </summary>
    public class CategoryIntegrationManager : IntegrationManagerBase
    {
        #region Constants
        /// <summary>Indicates that all <see cref="Capability"/>s and <see cref="AccessPoint"/>s shall be integrated.</summary>
        public const string AllCategoryName = "all";

        /// <summary>A list of all known <see cref="AccessPoint"/> categories.</summary>
        public static readonly ICollection<string> Categories = new[] { CapabilityRegistration.CategoryName, DefaultAccessPoint.CategoryName, IconAccessPoint.CategoryName, AllCategoryName };
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public CategoryIntegrationManager(bool global) : base(global)
        {}
        #endregion

        //--------------------//

        /// <summary>
        /// Applies a category of <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="feed">The <see cref="Feed"/> for the application to perform the operation on.</param>
        /// <param name="categories">A list of all <see cref="AccessPoint"/> categories to be added to the already applied ones.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void AddAccessPointCategory(string interfaceID, Feed feed, ICollection<string> categories)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            if (categories == null) throw new ArgumentNullException("categories");
            #endregion

            // Implicitly add application to list if missing
            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null)
            {
                appEntry = BuildAppEntry(interfaceID, feed);
                AppList.Entries.Add(appEntry);
            }
            if (appEntry.AccessPoints == null) appEntry.AccessPoints = new AccessPointList();

            AddAccessPoints(appEntry, feed, CategoriesToAccessPoints(appEntry, categories));
        }

        /// <summary>
        /// Removes a category of already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="categories">A list of all <see cref="AccessPoint"/> categories to be removed from the already applied ones.</param>
        /// <exception cref="InvalidOperationException">Thrown in the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void RemoveAccessPointCategory(string interfaceID, ICollection<string> categories)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (categories == null) throw new ArgumentNullException("categories");
            #endregion

            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));
            if (appEntry.AccessPoints == null) return;

            RemoveAccessPoints(appEntry, CategoriesToAccessPoints(appEntry, categories));
        }

        #region Helpers
        /// <summary>
        /// Builds a list of <see cref="AccessPoint"/>s for specified categories.
        /// </summary>
        /// <param name="appEntry">The application entry to build <see cref="AccessPoint"/>s for.</param>
        /// <param name="categories">The <see cref="AccessPoint"/> categories to build <see cref="AccessPoint"/>s for</param>
        /// <returns>The newly generated lis of <see cref="AccessPoint"/>.</returns>
        private static IEnumerable<AccessPoint> CategoriesToAccessPoints(AppEntry appEntry, ICollection<string> categories)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (categories == null) throw new ArgumentNullException("categories");
            #endregion

            // Parse categories list
            bool all = categories.Contains(AllCategoryName);
            bool capabilities = categories.Contains(CapabilityRegistration.CategoryName) || all;
            bool defaults = categories.Contains(DefaultAccessPoint.CategoryName) || all;
            bool icons = categories.Contains(IconAccessPoint.CategoryName) || all;

            ICollection<AccessPoint> accessPoints = new LinkedList<AccessPoint>();
            
            if (capabilities) accessPoints.Add(new CapabilityRegistration());
            if (defaults)
            { // Add AccessPoints for all suitable Capabilities
                foreach (var capabilityList in appEntry.CapabilityLists.FindAll(list => list.Architecture.IsCompatible(Architecture.CurrentSystem)))
                {
                    foreach (var capability in capabilityList.Entries)
                    {
                        DefaultAccessPoint accessPoint = GetDefaultAccessPoint(capability);
                        if (accessPoint != null) accessPoints.Add(accessPoint);
                    }
                }
            }
            if (icons)
            { // Add icons for main entry point
                accessPoints.Add(new MenuEntry {Name = appEntry.Name, Command = Command.NameRun});
                accessPoints.Add(new DesktopIcon {Name = appEntry.Name, Command = Command.NameRun});
            }
            return accessPoints;
        }

        /// <summary>
        /// Creates a <see cref="DefaultAccessPoint"/> referencing a specific <see cref="Capability"/>.
        /// </summary>
        /// <param name="capability">The <see cref="Capability"/> to create a <see cref="DefaultAccessPoint"/> for.</param>
        /// <returns>The newly created <see cref="DefaultAccessPoint"/> or null if <paramref name="capability"/> was not a suitable type of <see cref="Capability"/>.</returns>
        private static DefaultAccessPoint GetDefaultAccessPoint(Capability capability)
        {
            DefaultAccessPoint accessPoint;
            if (capability is Capabilities.AutoPlay) accessPoint = new AutoPlay();
            else if (capability is Capabilities.ContextMenu) accessPoint = new ContextMenu();
            else if (capability is Capabilities.DefaultProgram) accessPoint = new DefaultProgram();
            else if (capability is Capabilities.FileType) accessPoint = new FileType();
            else if (capability is Capabilities.UrlProtocol) accessPoint = new UrlProtocol();
            else return null;

            accessPoint.Capability = capability.ID;
            return accessPoint;
        }
        #endregion
    }
}
