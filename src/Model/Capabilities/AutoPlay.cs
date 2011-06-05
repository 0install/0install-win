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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's ability to act as an AutoPlay handler for certain events.
    /// </summary>
    /// <remarks>A <see cref="FileType"/> is used for actually executing the application.</remarks>
    [XmlType("auto-play", Namespace = XmlNamespace)]
    public class AutoPlay : Capability, IEquatable<AutoPlay>
    {
        #region Properties
        /// <inheritdoc/>
        public override bool MachineWideOnly { get { return false; } }

        /// <summary>
        /// The name of the application as shown in the AutoPlay selection list.
        /// </summary>
        [Description("The name of the application as shown in the AutoPlay selection list.")]
        [XmlAttribute("provider")]
        public string Provider { get; set; }

        /// <summary>
        /// A human-readable description of the action such as "Burn CD".
        /// </summary>
        [Description("A human-readable description of the action such as \"Burn CD\".")]
        [XmlAttribute("description")]
        public string Description { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Icon> _icons = new C5.ArrayList<Icon>();
        /// <summary>
        /// Zero or more icons to use for the action.
        /// </summary>
        /// <remarks>The first compatible one is selected. If empty the application icon is used.</remarks>
        [Description("Zero or more icons to use for the action. (The first compatible one is selected. If empty the application icon is used.)")]
        [XmlElement("icon", Namespace = Feed.XmlNamespace)]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<Icon> Icons { get { return _icons; } }

        /// <summary>
        /// The <see cref="FileType"/> ID used to perform the action.
        /// </summary>
        [Description("The FileType ID used to perform the action.")]
        [XmlAttribute("file-type-id")]
        public string FileTypeID { get; set; }

        /// <summary>
        /// The verb from the file type used to perform the action.
        /// </summary>
        [Description("The verb from the file type used to perform the action.")]
        [XmlAttribute("file-type-verb")]
        public string FileTypeIDVerb { get; set; }

        // Preserve order
        private readonly C5.ArrayList<AutoPlayEvent> _events = new C5.ArrayList<AutoPlayEvent>();
        /// <summary>
        /// The IDs of the events this action can handle.
        /// </summary>
        [Description("The IDs of the events this action can handle.")]
        [XmlElement("event")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<AutoPlayEvent> Events { get { return _events; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "AutoPlay: Description (ID)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AutoPlay: {0} ({1})", Description, ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability CloneCapability()
        {
            var capability = new AutoPlay {ID = ID, Provider = Provider, Description = Description, FileTypeID = FileTypeID, FileTypeIDVerb = FileTypeIDVerb};
            capability.Icons.AddAll(Icons);
            capability.Events.AddAll(Events);
            return capability;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AutoPlay other)
        {
            if (other == null) return false;

            return base.Equals(other) &&
                other.Provider == Provider && other.Description == Description && other.FileTypeID == FileTypeID && other.FileTypeIDVerb == FileTypeIDVerb &&
                Icons.SequencedEquals(other.Icons) && Events.SequencedEquals(other.Events);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AutoPlay) && Equals((AutoPlay)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Provider ?? "").GetHashCode();
                result = (result * 397) ^ (Description ?? "").GetHashCode();
                result = (result * 397) ^ Icons.GetSequencedHashCode();
                result = (result * 397) ^ (FileTypeID ?? "").GetHashCode();
                result = (result * 397) ^ (FileTypeIDVerb ?? "").GetHashCode();
                result = (result * 397) ^ Events.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
