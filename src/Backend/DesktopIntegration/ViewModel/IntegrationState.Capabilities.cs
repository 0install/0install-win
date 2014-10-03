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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NanoByte.Common.Dispatch;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.ViewModel
{
    partial class IntegrationState
    {
        /// <summary>
        /// Controls whether <see cref="AccessPoints.CapabilityRegistration"/> is used.
        /// </summary>
        public bool CapabilitiyRegistration { get; set; }

        public readonly BindingList<FileTypeModel> FileTypes = new BindingList<FileTypeModel>();
        public readonly BindingList<UrlProtocolModel> UrlProtocols = new BindingList<UrlProtocolModel>();
        public readonly BindingList<AutoPlayModel> AutoPlay = new BindingList<AutoPlayModel>();
        public readonly BindingList<ContextMenuModel> ContextMenu = new BindingList<ContextMenuModel>();
        public readonly BindingList<DefaultProgramModel> DefaultProgram = new BindingList<DefaultProgramModel>();

        /// <summary>
        /// List of all <see cref="CapabilityModel"/>s handled by this View-Model.
        /// </summary>
        private readonly List<CapabilityModel> _capabilityModels = new List<CapabilityModel>();

        /// <summary>
        /// Reads the <see cref="DefaultCapability"/>s from <see cref="Store.Model.Feed.CapabilityLists"/> and creates a coressponding model for turning <see cref="AccessPoints.DefaultAccessPoint"/> on and off.
        /// </summary>
        private void LoadDefaultAccessPoints()
        {
            var dispatcher = new PerTypeDispatcher<Capability>(true)
            {
                (FileType fileType) =>
                {
                    var model = new FileTypeModel(fileType, IsCapabillityUsed<AccessPoints.FileType>(fileType));
                    FileTypes.Add(model);
                    _capabilityModels.Add(model);
                },
                (UrlProtocol urlProtocol) =>
                {
                    var model = new UrlProtocolModel(urlProtocol, IsCapabillityUsed<AccessPoints.UrlProtocol>(urlProtocol));
                    UrlProtocols.Add(model);
                    _capabilityModels.Add(model);
                },
                (AutoPlay autoPlay) =>
                {
                    var model = new AutoPlayModel(autoPlay, IsCapabillityUsed<AccessPoints.AutoPlay>(autoPlay));
                    AutoPlay.Add(model);
                    _capabilityModels.Add(model);
                },
                (ContextMenu contextMenu) =>
                {
                    var model = new ContextMenuModel(contextMenu, IsCapabillityUsed<AccessPoints.ContextMenu>(contextMenu));
                    ContextMenu.Add(model);
                    _capabilityModels.Add(model);
                }
            };
            if (_integrationManager.MachineWide)
            {
                dispatcher.Add((DefaultProgram defaultProgram) =>
                {
                    var model = new DefaultProgramModel(defaultProgram, IsCapabillityUsed<AccessPoints.DefaultProgram>(defaultProgram));
                    DefaultProgram.Add(model);
                    _capabilityModels.Add(model);
                });
            }

            dispatcher.Dispatch(AppEntry.CapabilityLists.CompatibleCapabilities());
        }

        /// <summary>
        /// Checks whether a <see cref="DefaultCapability"/> is already used by the user.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="toCheck">The <see cref="Capability"/> to check for usage.</param>
        /// <returns><see langword="true"/>, if <paramref name="toCheck"/> is already in usage.</returns>
        private bool IsCapabillityUsed<T>(DefaultCapability toCheck)
            where T : AccessPoints.DefaultAccessPoint
        {
            if (AppEntry.AccessPoints == null) return false;

            return AppEntry.AccessPoints.Entries.OfType<T>().Any(accessPoint => accessPoint.Capability == toCheck.ID);
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
