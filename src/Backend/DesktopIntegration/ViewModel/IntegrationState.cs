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
using System.Linq;
using NanoByte.Common.Collections;
using NanoByte.Common.Dispatch;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.ViewModel
{
    /// <summary>
    /// A View-Model for modifying the current desktop integration state.
    /// </summary>
    public partial class IntegrationState
    {
        #region Dependencies
        /// <summary>
        /// The integration manager used to apply selected integration options.
        /// </summary>
        private readonly IIntegrationManager _integrationManager;

        /// <summary>
        /// The application being integrated.
        /// </summary>
        public AppEntry AppEntry { get; private set; }

        /// <summary>
        /// The feed providing additional metadata, icons, etc. for the application.
        /// </summary>
        public Feed Feed { get; private set; }

        /// <summary>
        /// Creates a new integration state View-Model.
        /// </summary>
        /// <param name="integrationManager">The integration manager used to apply selected integration options.</param>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        public IntegrationState(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            _integrationManager = integrationManager;
            AppEntry = appEntry;
            Feed = feed;
        }
        #endregion

        public void ApplyChanges(bool capabilitiyRegistration)
        {
            var toAdd = new List<AccessPoints.AccessPoint>();
            var toRemove = new List<AccessPoints.AccessPoint>();
            (capabilitiyRegistration ? toAdd : toRemove).Add(new AccessPoints.CapabilityRegistration());
            CollectCommandAccessPointChanges(toAdd, toRemove);
            CollectDefaultAccessPointChanges(toAdd, toRemove);

            if (toRemove.Any()) _integrationManager.RemoveAccessPoints(AppEntry, toRemove);
            if (toAdd.Any()) _integrationManager.AddAccessPoints(AppEntry, Feed, toAdd);
        }

        private void CollectCommandAccessPointChanges(ICollection<AccessPoints.AccessPoint> toAdd, ICollection<AccessPoints.AccessPoint> toRemove)
        {
            // Build lists with current integration state
            var currentMenuEntries = new List<AccessPoints.MenuEntry>();
            var currentDesktopIcons = new List<AccessPoints.DesktopIcon>();
            var currentAliases = new List<AccessPoints.AppAlias>();
            if (AppEntry.AccessPoints != null)
            {
                new PerTypeDispatcher<AccessPoints.AccessPoint>(true)
                {
                    (Action<AccessPoints.MenuEntry>)currentMenuEntries.Add,
                    (Action<AccessPoints.DesktopIcon>)currentDesktopIcons.Add,
                    (Action<AccessPoints.AppAlias>)currentAliases.Add
                }.Dispatch(AppEntry.AccessPoints.Entries);
            }

            // Determine differences between current and desired state
            Merge.TwoWay(theirs: MenuEntries, mine: currentMenuEntries, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: DesktopIcons, mine: currentDesktopIcons, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: Aliases, mine: currentAliases, added: toAdd.Add, removed: toRemove.Add);
        }

        private void CollectDefaultAccessPointChanges(ICollection<AccessPoints.AccessPoint> toAdd, ICollection<AccessPoints.AccessPoint> toRemove)
        {
            foreach (var model in _capabilityModels.Where(model => model.Changed))
            {
                var accessPoint = model.Capability.ToAcessPoint();
                if (model.Use) toAdd.Add(accessPoint);
                else toRemove.Add(accessPoint);
            }
        }
    }
}