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
using System.Globalization;
using System.IO;
using Common.Collections;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages desktop integration via <see cref="AccessPoint"/>s, grouping them into categories.
    /// </summary>
    /// <remarks>
    /// To prevent raceconditions there may only be one desktop integration class active at any given time.
    /// This class aquires a mutex upon calling its constructor and releases it upon calling <see cref="IntegrationManager.Dispose()"/>.
    /// </remarks>
    public class CategoryIntegrationManager : IntegrationManager, ICategoryIntegrationManager
    {
        #region Constants
        /// <summary>Indicates that all <see cref="Capabilities.Capability"/>s and <see cref="AccessPoint"/>s shall be integrated.</summary>
        private const string AllCategoryName = "all";

        /// <summary>A list of all known <see cref="AccessPoint"/> categories.</summary>
        public static readonly ICollection<string> Categories = new[] {CapabilityRegistration.CategoryName, DefaultAccessPoint.CategoryName, IconAccessPoint.CategoryName, AllCategoryName};
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public CategoryIntegrationManager(bool systemWide, ITaskHandler handler) : base(systemWide, handler)
        {}
        #endregion

        //--------------------//

        #region Add
        /// <inheritdoc/>
        public void AddAccessPointCategories(AppEntry appEntry, Feed feed, ICollection<string> categories, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            if (categories == null) throw new ArgumentNullException("categories");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Parse categories list
            bool all = categories.Contains(AllCategoryName);
            bool capabilities = categories.Contains(CapabilityRegistration.CategoryName) || all;
            bool defaults = categories.Contains(DefaultAccessPoint.CategoryName) || all;
            bool icons = categories.Contains(IconAccessPoint.CategoryName) || all;

            // Build capability list
            var accessPointsToAdd = new LinkedList<AccessPoint>();
            if (capabilities) accessPointsToAdd.AddLast(new CapabilityRegistration());
            if (defaults)
            {
                // Add AccessPoints for all suitable Capabilities
                foreach (var capabilityList in appEntry.CapabilityLists)
                {
                    if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;
                    foreach (var capability in EnumerableUtils.OfType<Capabilities.DefaultCapability>(capabilityList.Entries))
                    {
                        if (capability.WindowsSystemWideOnly && !SystemWide && WindowsUtils.IsWindows) continue;
                        if (!capability.ExplicitOnly)
                            accessPointsToAdd.AddLast(DefaultAccessPoint.FromCapability(capability));
                    }
                }
            }
            if (icons)
            {
                // Add icons for main entry point
                accessPointsToAdd.AddLast(new DesktopIcon {Name = appEntry.Name});
                if (feed.EntryPoints.IsEmpty)
                    accessPointsToAdd.AddLast(new MenuEntry {Name = appEntry.Name, Category = appEntry.Name});

                // Add icons for additional entry points
                foreach (var entryPoint in feed.EntryPoints)
                {
                    string entryPointName = entryPoint.Names.GetBestLanguage(CultureInfo.CurrentUICulture);
                    if (!string.IsNullOrEmpty(entryPoint.Command) && !string.IsNullOrEmpty(entryPointName))
                    {
                        accessPointsToAdd.AddLast(new MenuEntry
                        {
                            Name = entryPointName,
                            Category = appEntry.Name,
                            // Don't explicitly write the "run" command 
                            Command = (entryPoint.Command == Command.NameRun ? null : entryPoint.Command)
                        });
                    }
                }
            }

            try
            {
                AddAccessPointsHelper(appEntry, feed, accessPointsToAdd);
            }
                #region Error handling
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            #endregion

            if (icons && SystemWide) ToggleIconsVisible(appEntry, true);
            Complete();
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public void RemoveAccessPointCategories(AppEntry appEntry, ICollection<string> categories)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (categories == null) throw new ArgumentNullException("categories");
            #endregion

            if (appEntry.AccessPoints == null) return;

            // Parse categories list
            bool all = categories.Contains(AllCategoryName);
            bool capabilities = categories.Contains(CapabilityRegistration.CategoryName) || all;
            bool defaults = categories.Contains(DefaultAccessPoint.CategoryName) || all;
            bool icons = categories.Contains(IconAccessPoint.CategoryName) || all;

            // Build capability list
            var accessPointsToRemove = new C5.LinkedList<AccessPoint>();
            if (capabilities) accessPointsToRemove.AddAll(EnumerableUtils.OfType<CapabilityRegistration>(appEntry.AccessPoints.Entries));
            if (defaults) accessPointsToRemove.AddAll(EnumerableUtils.OfType<DefaultAccessPoint>(appEntry.AccessPoints.Entries));
            if (icons) accessPointsToRemove.AddAll(EnumerableUtils.OfType<IconAccessPoint>(appEntry.AccessPoints.Entries));

            try
            {
                RemoveAccessPointsHelper(appEntry, accessPointsToRemove);
            }
                #region Error handling
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            #endregion

            if (icons && SystemWide) ToggleIconsVisible(appEntry, false);
            Complete();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Toggles registry entries indicating whether icons for the application are currently visible.
        /// </summary>
        /// <param name="appEntry">The application being modified.</param>
        /// <param name="iconsVisible"><see langword="true"/> if the icons are currently visible, <see langword="false"/> if the icons are currently not visible.</param>
        /// <remarks>This is a special handler to support <see cref="Windows.DefaultProgram"/>.</remarks>
        private static void ToggleIconsVisible(AppEntry appEntry, bool iconsVisible)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            foreach (var capabilityList in appEntry.CapabilityLists)
            {
                if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;
                foreach (var defaultProgram in EnumerableUtils.OfType<Capabilities.DefaultProgram>(capabilityList.Entries))
                    Windows.DefaultProgram.ToggleIconsVisible(defaultProgram, iconsVisible);
            }
        }
        #endregion
    }
}
