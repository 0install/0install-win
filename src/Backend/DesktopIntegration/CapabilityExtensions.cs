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
using JetBrains.Annotations;
using NanoByte.Common.Dispatch;
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains extension methods for <see cref="Store.Model.Capabilities.Capability"/>s.
    /// </summary>
    public static class CapabilityExtensions
    {
        /// <summary>
        /// Creates a <see cref="DefaultAccessPoint"/> referencing a specific <see cref="Store.Model.Capabilities.DefaultCapability"/>.
        /// </summary>
        /// <param name="capability">The <see cref="Store.Model.Capabilities.DefaultCapability"/> to create a <see cref="DefaultAccessPoint"/> for.</param>
        /// <returns>The newly created <see cref="DefaultAccessPoint"/>.</returns>
        public static AccessPoint ToAcessPoint([NotNull] this Store.Model.Capabilities.DefaultCapability capability)
        {
            #region Sanity checks
            if (capability == null) throw new ArgumentNullException("capability");
            #endregion

            return new PerTypeDispatcher<Store.Model.Capabilities.DefaultCapability, DefaultAccessPoint>(ignoreMissing: false)
            {
                (Store.Model.Capabilities.AutoPlay x) => new AutoPlay {Capability = capability.ID},
                (Store.Model.Capabilities.ContextMenu x) => new ContextMenu {Capability = capability.ID},
                (Store.Model.Capabilities.DefaultProgram x) => new DefaultProgram {Capability = capability.ID},
                (Store.Model.Capabilities.FileType x) => new FileType {Capability = capability.ID},
                (Store.Model.Capabilities.UrlProtocol x) => new UrlProtocol {Capability = capability.ID},
            }.Dispatch(capability);
        }
    }
}
