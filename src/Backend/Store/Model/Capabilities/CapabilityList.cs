/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Xml.Serialization;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// Groups a number of application <see cref="Capability"/>s (for a specific operating system) that can be registered in a desktop environment.
    /// </summary>
    [Description("Groups a number of application capabilities (for a specific operating system) that can be registered in a desktop environment.")]
    [Serializable]
    [XmlRoot("capabilities", Namespace = XmlNamespace), XmlType("capabilities", Namespace = XmlNamespace)]
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
        /// Determines for which operating system the <see cref="Capability"/>s are applicable.
        /// </summary>
        [Description("Determines for which operating system the capabilities are applicable.")]
        [XmlAttribute("os"), DefaultValue(typeof(OS), "All")]
        public OS OS { get; set; }

        private readonly List<Capability> _entries = new List<Capability>();

        /// <summary>
        /// A list of <see cref="Capability"/>s.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(AppRegistration)), XmlElement(typeof(AutoPlay)), XmlElement(typeof(ComServer)), XmlElement(typeof(ContextMenu)), XmlElement(typeof(DefaultProgram)), XmlElement(typeof(FileType)), XmlElement(typeof(UrlProtocol))]
        public List<Capability> Entries { get { return _entries; } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="CapabilityList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="CapabilityList"/>.</returns>
        public CapabilityList Clone()
        {
            var capabilityList = new CapabilityList {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, OS = OS};
            capabilityList.Entries.AddRange(Entries.CloneElements());
            return capabilityList;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the capability list in the form "OS". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return OS.ToString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(CapabilityList other)
        {
            if (other == null) return false;
            return base.Equals(other) && (OS == other.OS && Entries.SequencedEquals(other.Entries));
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
                result = (result * 397) ^ (int)OS;
                result = (result * 397) ^ Entries.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
