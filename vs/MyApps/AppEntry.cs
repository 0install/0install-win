/*
 * Copyright 2010 Bastian Eicher
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

namespace ZeroInstall.MyApps
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
        /// The (possibly user-chosen) name of the application.
        /// </summary>
        [Description("The (possibly user-chosen) name of the application.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

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

        // Preserve order
        private readonly C5.ArrayList<Integration> _integrations = new C5.ArrayList<Integration>();
        /// <summary>
        /// A list of <see cref="Integration"/> handlers specifing how this application should be integrated into the system environment.
        /// </summary>
        [Description("A list of Integration handlers specifing how this application should be integrated into the system environment.")]
        [XmlElement(typeof(MenuEntry)), XmlElement(typeof(DesktopShortcut)), XmlElement(typeof(Bootstrapper))]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<Integration> Integrations { get { return _integrations; } }
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
            foreach (var integration in Integrations) appList.Integrations.Add(integration.CloneIntegration());

            return appList;
        }

        public object Clone()
        {
            return CloneEntry();
        }
        #endregion

        #region Equality
        public bool Equals(AppEntry other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (Name != other.Name) return false;
            if (Interface != other.Interface) return false;
            if (!Integrations.SequencedEquals(other.Integrations)) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AppEntry) && Equals((AppEntry)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name ?? "").GetHashCode();
                result = (result * 397) ^ (InterfaceString ?? "").GetHashCode();
                result = (result * 397) ^ Integrations.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}