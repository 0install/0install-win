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
        /// The URI or local path of the interface defining the application.
        /// </summary>
        [Description("The URI or local path of the interface defining the application.")]
        [XmlAttribute("interface")]
        public string InterfaceID
        { get; set; }

        /// <summary>
        /// The name of the application. Usually equal to <see cref="Feed.Name"/>.
        /// </summary>
        [Description("he name of the application. Usually equal to the Name specified in the Feed.")]
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
        /// A set of <see cref="Capability"/> lists to be registered in the desktop environment. Only compatible architectures are handled.
        /// </summary>
        [Description("A set of Capability lists to be registered in the desktop environment. Only compatible architectures are handled.")]
        [XmlElement("capabilities", Namespace = Capability.XmlNamespace)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<CapabilityList> CapabilityLists { get { return _capabilityLists; } }

        // Preserve order
        private readonly C5.ArrayList<AccessPointList> _accessPointLists = new C5.ArrayList<AccessPointList>();
        /// <summary>
        /// A set of <see cref="AccessPoint"/> lists to be registered in the desktop environment. Only compatible architectures are handled.
        /// </summary>
        [Description("A set of AccessPoint lists to be registered in the desktop environment. Only compatible architectures are handled.")]
        [XmlElement("access-points")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<AccessPointList> AccessPointLists { get { return _accessPointLists; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the entry in the form "AppEntry: Name (InterfaceID)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AppEntry: {0} ({1})", Name, InterfaceID);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AppEntry"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AppEntry"/>.</returns>
        public AppEntry CloneEntry()
        {
            var appList = new AppEntry {Name = Name, InterfaceID = InterfaceID};
            foreach (var list in CapabilityLists) appList.CapabilityLists.Add(list.CloneCapabilityList());
            foreach (var list in AccessPointLists) appList.AccessPointLists.Add(list.CloneAccessPointList());

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
            if (InterfaceID != other.InterfaceID) return false;
            if (!CapabilityLists.SequencedEquals(other.CapabilityLists)) return false;
            if (!AccessPointLists.SequencedEquals(other.AccessPointLists)) return false;
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
                result = (result * 397) ^ (InterfaceID ?? "").GetHashCode();
                result = (result * 397) ^ CapabilityLists.GetSequencedHashCode();
                result = (result * 397) ^ AccessPointLists.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}