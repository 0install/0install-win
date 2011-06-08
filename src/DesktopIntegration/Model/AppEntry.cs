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
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// Represents an application in the <see cref="AppList"/> indentified by its interface URI.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("app", Namespace = AppList.XmlNamespace)]
    public class AppEntry : IEquatable<AppEntry>
    {
        #region Properties
        /// <summary>
        /// The URI used to identify the interface and locate the feed.
        /// </summary>
        [Description("The URI used to identify the interface and locate the feed.")]
        [XmlIgnore]
        public Uri Interface
        { get; set; }
        
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public String InterfaceString
        {
            get { return (Interface == null ? null : Interface.ToString()); }
            set { Interface = new Uri(value); }
        }

        /// <summary>
        /// A user-definied alternative name for the appliaction, overriding the name specified in the feed.
        /// </summary>
        [Description("A user-definied alternative name for the appliaction, overriding the name specified in the feed.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Set to <see langword="true"/> to automatically download the newest available version of the application as a regular background task. Update checks will still be performed when the application is launched when set to <see langword="false"/>.
        /// </summary>
        [Description("Set to true to automatically download the newest available version of the application as a regular background task. Update checks will still be performed when the application is launched when set to false.")]
        [XmlAttribute("auto-update"), DefaultValue(false)]
        public bool AutoUpdate { get; set; }

        // Preserve order
        private readonly C5.ArrayList<CapabilityList> _capabilityLists = new C5.ArrayList<CapabilityList>();
        /// <summary>
        /// A list of <see cref="Capability"/>s to be registered in the desktop environment.
        /// </summary>
        [Description("A list of capabilities to be registered in the desktop environment.")]
        [XmlElement("capabilities", Namespace = Capability.XmlNamespace)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<CapabilityList> CapabilityLists { get { return _capabilityLists; } }

        // Preserve order
        private readonly C5.ArrayList<AccessPoint> _accessPoints = new C5.ArrayList<AccessPoint>();
        /// <summary>
        /// A list of <see cref="AccessPoint"/>s to be created in the desktop environment.
        /// </summary>
        [Description("A list of access points to be created in the desktop environment.")]
        [XmlElement(typeof(AppPath)), XmlElement(typeof(AutoPlay)), XmlElement(typeof(ContextMenu)), XmlElement(typeof(DefaultProgram)), XmlElement(typeof(DesktopShortcut)), XmlElement(typeof(FileType)), XmlElement(typeof(MenuEntry)), XmlElement(typeof(UrlProtocol))]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<AccessPoint> AccessPoints { get { return _accessPoints; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the entry in the form "AppEntry: Name (Interface)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AppEntry: {0} ({1})", Name, Interface);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AppEntry"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AppEntry"/>.</returns>
        public AppEntry CloneEntry()
        {
            var appList = new AppEntry {Name = Name, Interface = Interface};
            foreach (var list in CapabilityLists) appList.CapabilityLists.Add(list.CloneCapabilityList());
            foreach (var accessPoint in AccessPoints) appList.AccessPoints.Add(accessPoint.CloneAccessPoint());

            return appList;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="AppEntry"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AppEntry"/>.</returns>
        public object Clone()
        {
            return CloneEntry();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AppEntry other)
        {
            if (other == null) return false;

            if (Name != other.Name) return false;
            if (Interface != other.Interface) return false;
            if (!CapabilityLists.SequencedEquals(other.CapabilityLists)) return false;
            if (!AccessPoints.SequencedEquals(other.AccessPoints)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AppEntry) && Equals((AppEntry)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name ?? "").GetHashCode();
                result = (result * 397) ^ (InterfaceString ?? "").GetHashCode();
                result = (result * 397) ^ CapabilityLists.GetSequencedHashCode();
                result = (result * 397) ^ AccessPoints.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}