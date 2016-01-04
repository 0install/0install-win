/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
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
        /// <summary>A list of all known <see cref="AccessPoint"/> categories.</summary>
        public static readonly string[] AllCategories = {CapabilityRegistration.CategoryName, MenuEntry.CategoryName, DesktopIcon.CategoryName, SendTo.CategoryName, AppAlias.CategoryName, AutoStart.CategoryName, DefaultAccessPoint.CategoryName};

        /// <summary>A list of recommended standard <see cref="AccessPoint"/> categories.</summary>
        public static readonly string[] StandardCategories = {CapabilityRegistration.CategoryName, MenuEntry.CategoryName, SendTo.CategoryName, AppAlias.CategoryName};
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public CategoryIntegrationManager([NotNull] string appListPath, [NotNull] ITaskHandler handler, bool machineWide = false) : base(appListPath, handler, machineWide)
        {}

        /// <inheritdoc/>
        public CategoryIntegrationManager([NotNull] ITaskHandler handler, bool machineWide = false) : base(handler, machineWide)
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
            bool capabilities = categories.Contains(CapabilityRegistration.CategoryName);
            bool menu = categories.Contains(MenuEntry.CategoryName);
            bool desktop = categories.Contains(DesktopIcon.CategoryName);
            bool sendTo = categories.Contains(SendTo.CategoryName);
            bool alias = categories.Contains(AppAlias.CategoryName);
            bool autoStart = categories.Contains(AutoStart.CategoryName);
            bool defaults = categories.Contains(DefaultAccessPoint.CategoryName);

            // Build capability list
            var accessPointsToAdd = new List<AccessPoint>();
            if (capabilities) accessPointsToAdd.Add(new CapabilityRegistration());
            if (menu) accessPointsToAdd.AddRange(Suggest.MenuEntries(feed));
            if (desktop) accessPointsToAdd.AddRange(Suggest.DesktopIcons(feed));
            if (sendTo) accessPointsToAdd.AddRange(Suggest.SendTo(feed));
            if (alias) accessPointsToAdd.AddRange(Suggest.Aliases(feed));
            if (autoStart) accessPointsToAdd.AddRange(Suggest.AutoStart(feed));
            if (defaults)
            {
                // Add AccessPoints for all suitable Capabilities
                accessPointsToAdd.AddRange((
                    from capability in appEntry.CapabilityLists.CompatibleCapabilities().OfType<DefaultCapability>()
                    where !capability.WindowsMachineWideOnly || MachineWide || !WindowsUtils.IsWindows
                    where !capability.ExplicitOnly
                    select capability.ToAcessPoint()));
            }

            try
            {
                AddAccessPointsInternal(appEntry, feed, accessPointsToAdd);
                if (menu && MachineWide) ToggleIconsVisible(appEntry, true);
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
            bool capabilities = categories.Contains(CapabilityRegistration.CategoryName);
            bool menu = categories.Contains(MenuEntry.CategoryName);
            bool desktop = categories.Contains(DesktopIcon.CategoryName);
            bool sendTo = categories.Contains(SendTo.CategoryName);
            bool alias = categories.Contains(AppAlias.CategoryName);
            bool autoStart = categories.Contains(AutoStart.CategoryName);
            bool defaults = categories.Contains(DefaultAccessPoint.CategoryName);

            // Build capability list
            var accessPointsToRemove = new List<AccessPoint>();
            if (capabilities) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<CapabilityRegistration>());
            if (menu) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<MenuEntry>());
            if (desktop) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<DesktopIcon>());
            if (sendTo) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<SendTo>());
            if (alias) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<AppAlias>());
            if (autoStart) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<AutoStart>());
            if (defaults) accessPointsToRemove.AddRange(appEntry.AccessPoints.Entries.OfType<DefaultAccessPoint>());

            try
            {
                RemoveAccessPointsInternal(appEntry, accessPointsToRemove);
                if (menu && MachineWide) ToggleIconsVisible(appEntry, false);
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
        /// <param name="iconsVisible"><c>true</c> if the icons are currently visible, <c>false</c> if the icons are currently not visible.</param>
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
