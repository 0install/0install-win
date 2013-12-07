/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Collections;
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains extension methods for <see cref="Model.Capabilities.Capability"/>s.
    /// </summary>
    public static class CapabilityExtensions
    {
        /// <summary>
        /// Creates a <see cref="DefaultAccessPoint"/> referencing a specific <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/>.
        /// </summary>
        /// <param name="capability">The <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/> to create a <see cref="DefaultAccessPoint"/> for.</param>
        /// <returns>The newly created <see cref="DefaultAccessPoint"/>.</returns>
        public static AccessPoint ToAcessPoint(this Model.Capabilities.DefaultCapability capability)
        {
            #region Sanity checks
            if (capability == null) throw new ArgumentNullException("capability");
            #endregion

            return new PerTypeDispatcher<Model.Capabilities.DefaultCapability, DefaultAccessPoint>(ignoreMissing: false)
            {
                (Model.Capabilities.AutoPlay x) => new AutoPlay {Capability = capability.ID},
                (Model.Capabilities.ContextMenu x) => new ContextMenu {Capability = capability.ID},
                (Model.Capabilities.DefaultProgram x) => new DefaultProgram {Capability = capability.ID},
                (Model.Capabilities.FileType x) => new FileType {Capability = capability.ID},
                (Model.Capabilities.UrlProtocol x) => new UrlProtocol {Capability = capability.ID},
            }.Dispatch(capability);
        }
    }
}
