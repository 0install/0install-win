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
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.ViewModel
{
    partial class IntegrationState
    {
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
    }
}