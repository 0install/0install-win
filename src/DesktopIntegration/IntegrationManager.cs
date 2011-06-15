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
using ZeroInstall.Model.Capabilities;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages desktop integration operations.
    /// </summary>
    public class IntegrationManager
    {
        #region Constants
        /// <summary>Indicates that all <see cref="Capabilities.Capability"/>s and <see cref="AccessPoint"/>s shall be integrated.</summary>
        public const string AllCategoryName = "all";

        /// <summary>A list of all known <see cref="AccessPoint"/> categories.</summary>
        public static readonly ICollection<string> Categories = new[] {CapabilityRegistration.CategoryName, DefaultAccessPoint.CategoryName, IconAccessPoint.CategoryName, AppPath.CategoryName, AllCategoryName};
        #endregion

        #region Variables
        /// <summary>Apply operations system-wide instead of just for the current user.</summary>
        private readonly bool _global;

        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        private readonly string _appListPath;

        /// <summary>Stores a list of applications and their desktop integrations.</summary>
        private readonly AppList _appList;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new integration manager.
        /// </summary>
        /// <param name="global">Apply operations system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or wirte access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool global)
        {
            _global = global;

            _appListPath = global
                ? FileUtils.PathCombine(Locations.SystemConfigDirs.Split(Path.PathSeparator)[0], "0install.net", "desktop-integration", "myapps.xml")
                : Locations.GetSaveConfigPath("0install.net", Path.Combine("desktop-integration", "myapps.xml"), false);

            if (File.Exists(_appListPath)) _appList = AppList.Load(_appListPath);
            else
            {
                _appList = new AppList();
                _appList.Save(_appListPath);
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

            _appList.Entries.Add(BuildAppEntry(interfaceID, feed));
            _appList.Save(_appListPath);
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

            // Prevent missing entries
            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));

            RemoveAccessPoints(interfaceID, new[] {AllCategoryName});

            _appList.Entries.Remove(appEntry);
            _appList.Save(_appListPath);
        }
        #endregion

        #region AccessPoints
        /// <summary>
        /// Applies new <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="feed">The <see cref="Feed"/> for the application to perform the operation on.</param>
        /// <param name="categories">A list of all <see cref="AccessPoint"/> categories to be added to the already applied ones.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void AddAccessPoint(string interfaceID, Feed feed, ICollection<string> categories)
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
                _appList.Entries.Add(appEntry);
            }

            bool all = categories.Contains(AllCategoryName);
            bool capabilities = categories.Contains(CapabilityRegistration.CategoryName) || all;
            bool defaults = categories.Contains(DefaultAccessPoint.CategoryName) || all;
            bool icons = categories.Contains(IconAccessPoint.CategoryName) || all;
            bool appPath = categories.Contains(AppPath.CategoryName) || all;

            // ToDo: Modify model
            if (appEntry.AccessPoints == null) appEntry.AccessPoints = new AccessPointList();

            _appList.Save(_appListPath);

            // ToDo: Apply changes
            if (capabilities)
            {
                foreach (var capabilityList in appEntry.CapabilityLists)
                {
                    if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;

                    foreach (var capability in capabilityList.Entries)
                        ApplyCapability(interfaceID, feed, capability, defaults);
                }
            }
            WindowsUtils.NotifyAssocChanged();
        }

        private void ApplyCapability(string interfaceID, Feed feed, Capability capability, bool defaults)
        {
            var fileType = capability as Capabilities.FileType;
            if (fileType != null && WindowsUtils.IsWindows)
                Windows.FileType.Apply(interfaceID, feed, fileType, defaults, _global);
        }

        /// <summary>
        /// Removes already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="categories">A list of all <see cref="AccessPoint"/> categories to be removed from the already applied ones.</param>
        /// <exception cref="InvalidOperationException">Thrown in the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void RemoveAccessPoints(string interfaceID, ICollection<string> categories)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (categories == null) throw new ArgumentNullException("categories");
            #endregion

            // Prevent missing entries
            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));

            bool all = categories.Contains(AllCategoryName);
            bool capabilities = categories.Contains(CapabilityRegistration.CategoryName) || all;
            bool defaults = categories.Contains(DefaultAccessPoint.CategoryName) || all;
            bool icons = categories.Contains(IconAccessPoint.CategoryName) || all;
            bool appPath = categories.Contains(AppPath.CategoryName) || all;

            // ToDo: Modify model

            // ToDo: Apply changes
            WindowsUtils.NotifyAssocChanged();

            _appList.Save(_appListPath);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Tries to fo find an existing <see cref="AppEntry"/> in <see cref="_appList"/>.
        /// </summary>
        /// <param name="interfaceID">The <see cref="AppEntry.InterfaceID"/> to look for.</param>
        /// <returns>The first matching <see cref="AppEntry"/> that was found; <see langword="null"/> if no match was found.</returns>
        private AppEntry FindAppEntry(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            AppEntry appEntry;
            _appList.Entries.Find(entry => entry.InterfaceID == interfaceID, out appEntry);
            return appEntry;
        }

        /// <summary>
        /// Creates a mew <see cref="AppEntry"/>.
        /// </summary>
        /// <param name="interfaceID">The <see cref="AppEntry.InterfaceID"/> to set.</param>
        /// <param name="feed">The feed to get data such as the name an <see cref="Capabilities.Capability"/>s from.</param>
        /// <returns>The newly created <see cref="AppEntry"/>.</returns>
        private static AppEntry BuildAppEntry(string interfaceID, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            var appEntry = new AppEntry { InterfaceID = interfaceID, Name = feed.Name };
            foreach (var capabilityList in feed.CapabilityLists)
            {
                if (capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem))
                    appEntry.CapabilityLists.Add(capabilityList.CloneCapabilityList());
            }
            return appEntry;
        }
        #endregion
    }
}
