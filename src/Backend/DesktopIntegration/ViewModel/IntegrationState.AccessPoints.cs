/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common.Collections;
using NanoByte.Common.Dispatch;
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration.ViewModel
{
    partial class IntegrationState
    {
        public readonly BindingList<MenuEntry> MenuEntries = new BindingList<MenuEntry>();
        public readonly BindingList<DesktopIcon> DesktopIcons = new BindingList<DesktopIcon>();
        public readonly BindingList<SendTo> SendTo = new BindingList<SendTo>();
        public readonly BindingList<AppAlias> Aliases = new BindingList<AppAlias>();
        public readonly BindingList<AutoStart> AutoStarts = new BindingList<AutoStart>();

        /// <summary>
        /// Reads the <see cref="CommandAccessPoint"/>s from <see cref="DesktopIntegration.AppEntry.AccessPoints"/> or uses suggestion methods to fill in defaults.
        /// </summary>
        private void LoadCommandAccessPoints()
        {
            if (AppEntry.AccessPoints == null)
            { // Fill in default values for first integration
                MenuEntries.AddRange(Suggest.MenuEntries(Feed));
                SendTo.AddRange(Suggest.SendTo(Feed));
                Aliases.AddRange(Suggest.Aliases(Feed));
            }
            else
            { // Distribute existing CommandAccessPoints among type-specific binding lists
                foreach (var element in AppEntry.AccessPoints.Entries.CloneElements()) // Use clones so that user modifications can still be canceled
                {
                    switch (element)
                    {
                        case MenuEntry x: MenuEntries.Add(x); break;
                        case DesktopIcon x: DesktopIcons.Add(x); break;
                        case SendTo x: SendTo.Add(x); break;
                        case AppAlias x: Aliases.Add(x); break;
                        case AutoStart x: AutoStarts.Add(x); break;
                    }
                }
            }
        }

        private void CollectCommandAccessPointChanges(ICollection<AccessPoint> toAdd, ICollection<AccessPoint> toRemove)
        {
            // Build lists with current integration state
            var currentMenuEntries = new List<MenuEntry>();
            var currentDesktopIcons = new List<DesktopIcon>();
            var currentSendTo = new List<SendTo>();
            var currentAliases = new List<AppAlias>();
            var currentAutoStarts = new List<AutoStart>();
            if (AppEntry.AccessPoints != null)
            {
                foreach (var entry in AppEntry.AccessPoints.Entries)
                {
                    switch (entry)
                    {
                        case MenuEntry x: currentMenuEntries.Add(x); break;
                        case DesktopIcon x: currentDesktopIcons.Add(x); break;
                        case SendTo x: currentSendTo.Add(x); break;
                        case AppAlias x: currentAliases.Add(x); break;
                        case AutoStart x: currentAutoStarts.Add(x); break;
                    }
                }
            }

            // Remove incomplete entries
            MenuEntries.RemoveAll(x => string.IsNullOrEmpty(x.Name));
            DesktopIcons.RemoveAll(x => string.IsNullOrEmpty(x.Name));
            SendTo.RemoveAll(x => string.IsNullOrEmpty(x.Name));
            Aliases.RemoveAll(x => string.IsNullOrEmpty(x.Name));
            AutoStarts.RemoveAll(x => string.IsNullOrEmpty(x.Name));

            // Determine differences between current and desired state
            Merge.TwoWay(theirs: MenuEntries, mine: currentMenuEntries, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: DesktopIcons, mine: currentDesktopIcons, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: SendTo, mine: currentSendTo, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: Aliases, mine: currentAliases, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: AutoStarts, mine: currentAutoStarts, added: toAdd.Add, removed: toRemove.Add);
        }
    }
}
