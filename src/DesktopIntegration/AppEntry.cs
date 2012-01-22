/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Collections;
using Common.Utils;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Represents an application in the <see cref="AppList"/> indentified by its interface URI.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("app", Namespace = AppList.XmlNamespace)]
    public sealed class AppEntry : IMergeable<AppEntry>, ICloneable
    {
        #region Properties
        /// <summary>
        /// The URI or local path of the interface defining the application.
        /// </summary>
        [Description("The URI or local path of the interface defining the application.")]
        [XmlAttribute("interface")]
        public string InterfaceID { get; set; }

        string IMergeable<AppEntry>.MergeID { get { return InterfaceID; } }

        /// <summary>
        /// The name of the application. Usually equal to <see cref="Feed.Name"/>.
        /// </summary>
        [Description("The name of the application. Usually equal to the Name specified in the Feed.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        private bool _autoUpdate = true;

        /// <summary>
        /// Set to <see langword="true"/> to automatically download the newest available version of the application as a regular background task. Update checks will still be performed when the application is launched when set to <see langword="false"/>.
        /// </summary>
        [Description("Set to true to automatically download the newest available version of the application as a regular background task. Update checks will still be performed when the application is launched when set to false.")]
        [XmlAttribute("auto-update"), DefaultValue(true)]
        public bool AutoUpdate { get { return _autoUpdate; } set { _autoUpdate = value; } }

        // Preserve order
        private readonly C5.LinkedList<CapabilityList> _capabilityLists = new C5.LinkedList<CapabilityList>();

        /// <summary>
        /// A set of <see cref="Capability"/> lists to be registered in the desktop environment. Only compatible architectures are handled.
        /// </summary>
        [Description("A set of Capability lists to be registered in the desktop environment. Only compatible architectures are handled.")]
        [XmlElement("capabilities", Namespace = Capability.XmlNamespace)]
        // Note: Can not use ICollection<T> interface with XML Serialization
            public C5.LinkedList<CapabilityList> CapabilityLists
        {
            get { return _capabilityLists; }
        }

        /// <summary>
        /// A set of <see cref="AccessPoints"/>s to be registered in the desktop environment; may be <see langword="null"/>.
        /// </summary>
        [Description("A set of AccessPoints to be registered in the desktop environment; may be null.")]
        [XmlElement("access-points")]
        public AccessPointList AccessPoints { get; set; }

        /// <inheritdoc/>
        [Browsable(false)]
        [XmlIgnore]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The time this entry was last modified encoded as Unix time (number of seconds since the epoch).
        /// </summary>
        /// <remarks>This value is ignored by clone and equality methods.</remarks>
        [Browsable(false)]
        [XmlAttribute("timestamp"), DefaultValue(0)]
        public long TimestampUnix { get { return FileUtils.ToUnixTime(Timestamp); } set { Timestamp = FileUtils.FromUnixTime(value); } }
        #endregion

        //--------------------//

        #region Access
        /// <summary>
        /// Retrieves the first <see cref="Capability"/> that matches a specific type and ID and is compatible with <see cref="Architecture.CurrentSystem"/>.
        /// </summary>
        /// <typeparam name="T">The capability type to match.</typeparam>
        /// <param name="id">The <see cref="Capability.ID"/> to match.</param>
        /// <returns>The first matching <see cref="Capability"/> or <see langword="null"/> if none was found.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no capability matching <paramref name="id"/> and <typeparamref name="T"/> was found.</exception>
        public T GetCapability<T>(string id) where T : Capability
        {
            foreach (var capabilityList in _capabilityLists)
            {
                if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;
                foreach (var capability in capabilityList.Entries)
                {
                    var specificCapability = capability as T;
                    if (specificCapability != null && specificCapability.ID == id) return specificCapability;
                }
            }
            throw new KeyNotFoundException(string.Format(Resources.UnableToFindTypeID, typeof(T).Name, id));
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the entry in the form "Name (InterfaceID)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, InterfaceID);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AppEntry"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AppEntry"/>.</returns>
        public AppEntry Clone()
        {
            var appList = new AppEntry {Name = Name, InterfaceID = InterfaceID};
            if (AccessPoints != null) appList.AccessPoints = AccessPoints.Clone();
            foreach (var list in CapabilityLists) appList.CapabilityLists.Add(list.Clone());

            return appList;
        }

        object ICloneable.Clone()
        {
            return Clone();
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
            if (!Equals(AccessPoints, other.AccessPoints)) return false;
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
                if (AccessPoints != null) result = (result * 397) ^ AccessPoints.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
