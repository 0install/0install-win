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
using System.Linq;
using System.Xml.Serialization;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Makes an application the default handler for a specific file type.
    /// </summary>
    /// <seealso cref="ZeroInstall.Store.Model.Capabilities.FileType"/>
    [XmlType("file-type", Namespace = AppList.XmlNamespace)]
    public class FileType : DefaultAccessPoint, IEquatable<FileType>
    {
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            #endregion

            var capability = appEntry.LookupCapability<Store.Model.Capabilities.FileType>(Capability);
            return capability.Extensions.Select(extension => "extension:" + extension.Value);
        }

        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            var capability = appEntry.LookupCapability<Store.Model.Capabilities.FileType>(Capability);
            var target = new FeedTarget(appEntry.InterfaceUri, feed);
            if (WindowsUtils.IsWindows) Windows.FileType.Register(target, capability, machineWide, handler, accessPoint: true);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            #endregion

            var capability = appEntry.LookupCapability<Store.Model.Capabilities.FileType>(Capability);
            if (WindowsUtils.IsWindows) Windows.FileType.Unregister(capability, machineWide, accessPoint: true);
        }

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "FileType: Capability". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return $"FileType: {Capability}";
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new FileType {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Capability = Capability};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FileType other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj.GetType() == typeof(FileType) && Equals((FileType)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
