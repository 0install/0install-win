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
        public override bool GlobalOnly { get { return false; } }

        /// <summary>
        /// The name of the application as shown in the AutoPlay selection list.
        /// </summary>
        [Description("The name of the application as shown in the AutoPlay selection list.")]
        [XmlAttribute("provider")]
        public string Provider { get; set; }

        /// <summary>
        /// A human-readable description of the AutoPlay operation.
        /// </summary>
        [Description("A human-readable description of the AutoPlay operation.")]
        [XmlAttribute("description")]
        public string Description { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Icon> _icons = new C5.ArrayList<Icon>();
        /// <summary>
        /// Zero or more icons to represent the AutoPlay operation.
        /// </summary>
        /// <remarks>The first compatible one is selected. If empty the application icon is used.</remarks>
        [Description("Zero or more icons to represent the AutoPlay operation. (The first compatible one is selected. If empty the application icon is used.)")]
        [XmlElement("icon", Namespace = Feed.XmlNamespace)]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<Icon> Icons { get { return _icons; } }

        /// <summary>
        /// The programatic identifier used to store the <see cref="Verb"/>.
        /// </summary>
        [Description("The programatic identifier used to store the Verb.")]
        [XmlAttribute("prog-id")]
        public string ProgID { get; set; }

        /// <summary>
        /// The command to execute when the handler gets called.
        /// </summary>
        [Description("The command to execute when the handler gets called.")]
        [XmlElement("verb")]
        public Verb Verb { get; set; }

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
            var capability = new AutoPlay {ID = ID, Description = Description, Provider = Provider, ProgID = ProgID, Verb = Verb};
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
                other.Description == Description && other.Provider == Provider && other.ProgID == ProgID && other.Verb == Verb &&
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
                result = (result * 397) ^ (Provider ?? "").GetHashCode();
                result = (result * 397) ^ (ProgID ?? "").GetHashCode();
                result = (result * 397) ^ Verb.GetHashCode();
                result = (result * 397) ^ Icons.GetSequencedHashCode();
                result = (result * 397) ^ Events.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
