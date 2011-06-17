/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// A set of <see cref="AccessPoint"/>s that can be registered in a desktop environment.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("access-points", Namespace = AppList.XmlNamespace)]
    public sealed class AccessPointList : XmlUnknown, ICloneable, IEquatable<AccessPointList>
    {
        #region Properties
        // Preserve order
        private readonly C5.HashedLinkedList<AccessPoint> _accessPoints = new C5.HashedLinkedList<AccessPoint>();
        /// <summary>
        /// A list of <see cref="AccessPoint"/>s.
        /// </summary>
        [Description("A list of access points.")]
        [XmlElement(typeof(AppPath)), XmlElement(typeof(AutoPlay)), XmlElement(typeof(CapabilityRegistration)), XmlElement(typeof(ContextMenu)), XmlElement(typeof(DefaultProgram)), XmlElement(typeof(DesktopIcon)), XmlElement(typeof(FileType)), XmlElement(typeof(MenuEntry)), XmlElement(typeof(UrlProtocol)), XmlElement(typeof(QuickLaunch))]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedLinkedList<AccessPoint> Entries { get { return _accessPoints; } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPointList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPointList"/>.</returns>
        public AccessPointList CloneAccessPointList()
        {
            var accessPointList = new AccessPointList();
            foreach (var entry in Entries) accessPointList.Entries.Add(entry.CloneAccessPoint());

            return accessPointList;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPointList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPointList"/>.</returns>
        public object Clone()
        {
            return CloneAccessPointList();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the access point list in the form "AccessPoints: Entries". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AccessPoints: {0}", Entries);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AccessPointList other)
        {
            if (other == null) return false;

            if (!Entries.SequencedEquals(other.Entries)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AccessPointList) && Equals((AccessPointList)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Entries.GetSequencedHashCode();
        }
        #endregion
    }
}
