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
    /// Groups a number of <see cref="AccessPoint"/>s that can be registered in a desktop environment.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("access-points", Namespace = AppList.XmlNamespace)]
    public sealed class AccessPointList : XmlUnknown, ICloneable, IEquatable<AccessPointList>
    {
        #region Properties
        /// <summary>
        /// Determines for which operating systems the <see cref="AccessPoint"/>s are aplicable.
        /// </summary>
        [Description("Determines for which operating systems the access-points are aplicable.")]
        [XmlIgnore]
        public Architecture Architecture { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [XmlAttribute("arch"), DefaultValue("*-*")]
        public string ArchitectureString
        {
            get { return Architecture.ToString(); }
            set { Architecture = new Architecture(value); }
        }

        // Preserve order
        private readonly C5.ArrayList<AccessPoint> _accessPoints = new C5.ArrayList<AccessPoint>();
        /// <summary>
        /// A list of <see cref="AccessPoint"/>s.
        /// </summary>
        [Description("A list of access points.")]
        [XmlElement(typeof(AppPath)), XmlElement(typeof(AutoPlay)), XmlElement(typeof(ContextMenu)), XmlElement(typeof(DefaultProgram)), XmlElement(typeof(DesktopIcon)), XmlElement(typeof(FileType)), XmlElement(typeof(MenuEntry)), XmlElement(typeof(UrlProtocol)), XmlElement(typeof(QuickLaunch))]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<AccessPoint> Entries { get { return _accessPoints; } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPointList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPointList"/>.</returns>
        public AccessPointList CloneAccessPointList()
        {
            var accessPointList = new AccessPointList { Architecture = Architecture };
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
        /// Returns the access point list in the form "AccessPoints for Architecture". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AccessPoints for {0}", Architecture);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AccessPointList other)
        {
            if (other == null) return false;

            if (Architecture != other.Architecture) return false;
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
            unchecked
            {
                int result = Architecture.GetHashCode();
                result = (result * 397) ^ Entries.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
