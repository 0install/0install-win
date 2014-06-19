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
using System.ComponentModel;
using System.Linq;
using NanoByte.Common.Collections;
using NanoByte.Common.Dispatch;
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration.ViewModel
{
    partial class IntegrationState
    {
        public readonly BindingList<MenuEntry> MenuEntries = new BindingList<MenuEntry>();
        public readonly BindingList<DesktopIcon> DesktopIcons = new BindingList<DesktopIcon>();
        public readonly BindingList<AppAlias> Aliases = new BindingList<AppAlias>();

        /// <summary>
        /// Reads the <see cref="CommandAccessPoint"/>s from <see cref="DesktopIntegration.AppEntry.AccessPoints"/> or uses suggestion methods to fill in defaults.
        /// </summary>
        private void LoadCommandAccessPoints()
        {
            if (AppEntry.AccessPoints == null)
            { // Fill in default values for first integration
                foreach (var entry in Suggest.MenuEntries(Feed)) MenuEntries.Add(entry);
                foreach (var desktopIcon in Suggest.DesktopIcons(Feed)) DesktopIcons.Add(desktopIcon);
                foreach (var alias in Suggest.Aliases(Feed)) Aliases.Add(alias);
            }
            else
            { // Distribute existing CommandAccessPoints among type-specific binding lists
                new PerTypeDispatcher<AccessPoint>(true)
                {
                    (Action<MenuEntry>)MenuEntries.Add,
                    (Action<DesktopIcon>)DesktopIcons.Add,
                    (Action<AppAlias>)Aliases.Add,
                }.Dispatch(AppEntry.AccessPoints.Entries.CloneElements()); // Use clones so that user modifications can still be canceled
            }
        }

        private void CollectCommandAccessPointChanges(ICollection<AccessPoint> toAdd, ICollection<AccessPoint> toRemove)
        {
            // Build lists with current integration state
            var currentMenuEntries = new List<MenuEntry>();
            var currentDesktopIcons = new List<DesktopIcon>();
            var currentAliases = new List<AppAlias>();
            if (AppEntry.AccessPoints != null)
            {
                new PerTypeDispatcher<AccessPoint>(true)
                {
                    (Action<MenuEntry>)currentMenuEntries.Add,
                    (Action<DesktopIcon>)currentDesktopIcons.Add,
                    (Action<AppAlias>)currentAliases.Add
                }.Dispatch(AppEntry.AccessPoints.Entries);
            }

            // Handle WinForms DataGrid bug creating phantom entries
            PurgeEmpty(MenuEntries);
            PurgeEmpty(DesktopIcons);
            PurgeEmpty(Aliases);

            // Determine differences between current and desired state
            Merge.TwoWay(theirs: MenuEntries, mine: currentMenuEntries, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: DesktopIcons, mine: currentDesktopIcons, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: Aliases, mine: currentAliases, added: toAdd.Add, removed: toRemove.Add);
        }

        private static void PurgeEmpty<T>(ICollection<T> list)
            where T : IEquatable<T>, new()
        {
            var emptyReference = new T();
            foreach (var entry in list.Where(x => x.Equals(emptyReference)).ToList())
                list.Remove(entry);
        }
    }
}