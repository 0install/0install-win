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
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Contains a set of <see cref="AccessPoint"/>s to be registered in a desktop environment.
    /// </summary>
    [Serializable, XmlRoot("access-points", Namespace = AppList.XmlNamespace), XmlType("access-points", Namespace = AppList.XmlNamespace)]
    public sealed class AccessPointList : XmlUnknown, ICloneable, IEquatable<AccessPointList>
    {
        private readonly List<AccessPoint> _accessPoints = new List<AccessPoint>();

        /// <summary>
        /// A list of <see cref="AccessPoint"/>s.
        /// </summary>
        [Description("A list of access points.")]
        [XmlElement(typeof(AppAlias)), XmlElement(typeof(AutoStart)), XmlElement(typeof(AutoPlay)), XmlElement(typeof(CapabilityRegistration)), XmlElement(typeof(ContextMenu)), XmlElement(typeof(DefaultProgram)), XmlElement(typeof(DesktopIcon)), XmlElement(typeof(FileType)), XmlElement(typeof(MenuEntry)), XmlElement(typeof(SendTo)), XmlElement(typeof(UrlProtocol)), XmlElement(typeof(QuickLaunch)), XmlElement(typeof(MockAccessPoint))]
        [NotNull]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public List<AccessPoint> Entries
        {
            get { return _accessPoints; }
        }

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPointList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPointList"/>.</returns>
        public AccessPointList Clone()
        {
            var accessPointList = new AccessPointList {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements};
            accessPointList.Entries.AddRange(Entries.CloneElements());

            return accessPointList;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the access point list in the form "Entry; Entry; ...". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return StringUtils.Join("; ", Entries.Select(x => x.ToString()));
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AccessPointList other)
        {
            if (other == null) return false;
            return base.Equals(other) && Entries.SequencedEquals(other.Entries);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is AccessPointList && Equals((AccessPointList)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Entries.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
