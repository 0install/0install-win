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
using System.IO;
using System.Linq;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages desktop integration via <see cref="AccessPoint"/>s, grouping them into categories.
    /// </summary>
    /// <remarks>
    /// To prevent raceconditions there may only be one desktop integration class instance active at any given time.
    /// This class acquires a mutex upon calling its constructor and releases it upon calling <see cref="IntegrationManager.Dispose()"/>.
    /// </remarks>
    public class CategoryIntegrationManager : IntegrationManager, ICategoryIntegrationManager
    {
        #region Constants
        /// <summary>Indicates that all <see cref="Store.Model.Capabilities.Capability"/>s and <see cref="AccessPoint"/>s shall be integrated.</summary>
        private const string AllCategoryName = "all";

        /// <summary>A list of all known <see cref="AccessPoint"/> categories.</summary>
        public static readonly ICollection<string> Categories = new[] {CapabilityRegistration.CategoryName, DefaultAccessPoint.CategoryName, IconAccessPoint.CategoryName, AppAlias.CategoryName, AllCategoryName};
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public CategoryIntegrationManager(string appListPath, ITaskHandler handler, bool machineWide = false) : base(appListPath, handler, machineWide)
        {}

        /// <inheritdoc/>
        public CategoryIntegrationManager(ITaskHandler handler, bool machineWide = false) : base(handler, machineWide)
        {}
        #endregion

        //--------------------//

        #region Add
        /// <inheritdoc/>
        public void AddAccessPointCategories(AppEntry appEntry, Feed feed, params string[] categories)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            if (categories == null) throw new ArgumentNullException("categories");
            #endregion

            // Parse categories list
            bool all = categories.Contains(AllCategoryName);
            bool capabilities = categories.Contains(CapabilityRegistration.CategoryName) || all;
            bool defaults = categories.Contains(DefaultAccessPoint.CategoryName) || all;
            bool icons = categories.Contains(IconAccessPoint.CategoryName) || all;
            bool aliases = categories.Contains(AppAlias.CategoryName) || all;

            // Build capability list
            var accessPointsToAdd = new List<AccessPoint>();
            if (capabilities) accessPointsToAdd.Add(new CapabilityRegistration());
            if (defaults)
            {
                // Add AccessPoints for all suitable Capabilities
                accessPointsToAdd.AddRange((
                    from capability in appEntry.CapabilityLists.CompatibleCapabilities().OfType<DefaultCapability>()
                    where !capability.WindowsMachineWideOnly || MachineWide || !WindowsUtils.IsWindows
                    where !capability.ExplicitOnly
                    select capability.ToAcessPoint()));
            }
            if (icons)
            {
                accessPointsToAdd.AddRange(Suggest.MenuEntries(feed).Cast<AccessPoint>());
                accessPointsToAdd.AddRange(Suggest.DesktopIcons(feed).Cast<AccessPoint>());
            }
            if (aliases)
                accessPointsToAdd.AddRange(Suggest.Aliases(feed).Cast<AccessPoint>());

            try
            {
                AddAccessPointsInternal(appEntry, feed, accessPointsToAdd);
                if (icons && MachineWide) ToggleIconsVisible(appEntry, true);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
            }
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public void RemoveAccessPointCategories(AppEntry appEntry, params string[] categories)
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
            bool aliases = categories.Contains(AppAlias.CategoryName) || all;

            // Build capability list
            var accessPointsToRemove = new List<AccessPoint>();
            if (capabilities) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<CapabilityRegistration>());
            if (defaults) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<DefaultAccessPoint>());
            if (icons) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<IconAccessPoint>());
            if (aliases) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<AppAlias>());

            try
            {
                RemoveAccessPointsInternal(appEntry, accessPointsToRemove);
                if (icons && MachineWide) ToggleIconsVisible(appEntry, false);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
            }
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

            foreach (var defaultProgram in appEntry.CapabilityLists.CompatibleCapabilities().OfType<Store.Model.Capabilities.DefaultProgram>())
                Windows.DefaultProgram.ToggleIconsVisible(defaultProgram, iconsVisible);
        }
        #endregion
    }
}
