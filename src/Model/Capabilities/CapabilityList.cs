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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using C5;
using Common.Collections;
using Common.Storage;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Groups a number of application <see cref="Capability"/>s (for a specific operating system) that can be registered in a desktop environment.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("capabilities", Namespace = XmlNamespace)]
    [XmlNamespace("xsi", XmlStorage.XsiNamespace)]
    public sealed class CapabilityList : XmlUnknown, ICloneable, IEquatable<CapabilityList>
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing application capabilities.
        /// </summary>
        public const string XmlNamespace = "http://0install.de/schema/desktop-integration/capabilities";

        /// <summary>
        /// The URI to retrieve an XSD containing the XML Schema information for this class in serialized form.
        /// </summary>
        public const string XsdLocation = XmlNamespace + "/capabilities.xsd";
        #endregion

        #region Properties
        /// <summary>
        /// Determines for which operating systems the <see cref="Capability"/>s are applicable.
        /// </summary>
        [Description("Determines for which operating systems the capabilities are applicable.")]
        [XmlIgnore]
        public Architecture Architecture { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [XmlAttribute("arch"), DefaultValue("*-*")]
        public string ArchitectureString { get { return Architecture.ToString(); } set { Architecture = new Architecture(value); } }

        // Preserve order
        private readonly ArrayList<Capability> _entries = new ArrayList<Capability>();

        /// <summary>
        /// A list of <see cref="Capability"/>s.
        /// </summary>
        [Description("A list of capabilities.")]
        [XmlElement(typeof(AppRegistration)), XmlElement(typeof(AutoPlay)), XmlElement(typeof(ComServer)), XmlElement(typeof(ContextMenu)), XmlElement(typeof(DefaultProgram)), XmlElement(typeof(FileType)), XmlElement(typeof(GamesExplorer)), XmlElement(typeof(UrlProtocol))]
        public ArrayList<Capability> Entries { get { return _entries; } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="CapabilityList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="CapabilityList"/>.</returns>
        public CapabilityList Clone()
        {
            var capabilityList = new CapabilityList {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Architecture = Architecture};
            capabilityList.Entries.AddAll(Entries.CloneElements());
            return capabilityList;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the capability list in the form "Capabilities for Architecture". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return String.Format("Capabilities for {0}", Architecture);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(CapabilityList other)
        {
            if (other == null) return false;
            return base.Equals(other) && (Architecture == other.Architecture && Entries.SequencedEquals(other.Entries));
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CapabilityList && Equals((CapabilityList)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Architecture.GetHashCode();
                result = (result * 397) ^ Entries.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
