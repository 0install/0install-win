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
        public void LoadCommandAccessPoints()
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

        /// <summary>
        /// Reads the <see cref="Store.Model.Capabilities.DefaultCapability"/>s from <see cref="Store.Model.Feed.CapabilityLists"/> and creates a coressponding model for turning <see cref="DefaultAccessPoint"/> on and off.
        /// </summary>
        public void LoadDefaultAccessPoints()
        {
            foreach (var capabilityList in AppEntry.CapabilityLists.Where(x => x.Architecture.IsCompatible()))
            {
                var dispatcher = new PerTypeDispatcher<Store.Model.Capabilities.Capability>(true)
                {
                    (Store.Model.Capabilities.FileType fileType) =>
                    {
                        var model = new FileTypeModel(fileType, IsCapabillityUsed<FileType>(fileType));
                        FileTypes.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Store.Model.Capabilities.UrlProtocol urlProtocol) =>
                    {
                        var model = new UrlProtocolModel(urlProtocol, IsCapabillityUsed<UrlProtocol>(urlProtocol));
                        UrlProtocols.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Store.Model.Capabilities.AutoPlay autoPlay) =>
                    {
                        var model = new AutoPlayModel(autoPlay, IsCapabillityUsed<AutoPlay>(autoPlay));
                        AutoPlay.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Store.Model.Capabilities.ContextMenu contextMenu) =>
                    {
                        var model = new ContextMenuModel(contextMenu, IsCapabillityUsed<ContextMenu>(contextMenu));
                        ContextMenu.Add(model);
                        _capabilityModels.Add(model);
                    }
                };
                if (_integrationManager.MachineWide)
                {
                    dispatcher.Add((Store.Model.Capabilities.DefaultProgram defaultProgram) =>
                    {
                        var model = new DefaultProgramModel(defaultProgram, IsCapabillityUsed<DefaultProgram>(defaultProgram));
                        DefaultProgram.Add(model);
                        _capabilityModels.Add(model);
                    });
                }

                dispatcher.Dispatch(capabilityList.Entries);
            }
        }
    }
}