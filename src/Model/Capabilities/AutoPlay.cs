﻿/*
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's ability to act as an AutoPlay handler for certain events.
    /// </summary>
    /// <remarks>A <see cref="FileType"/> is used for actually executing the application.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [XmlType("auto-play", Namespace = XmlNamespace)]
    public sealed class AutoPlay : IconCapability, IEquatable<AutoPlay>
    {
        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsSystemWideOnly { get { return false; } }

        /// <summary>
        /// The name of the application as shown in the AutoPlay selection list.
        /// </summary>
        [Description("The name of the application as shown in the AutoPlay selection list.")]
        [XmlAttribute("provider")]
        public string Provider { get; set; }

        /// <summary>
        /// The programmatic identifier used to store the <see cref="Verb"/>.
        /// </summary>
        [Description("The programmatic identifier used to store the verb.")]
        [XmlAttribute("prog-id")]
        public string ProgID { get; set; }

        /// <summary>
        /// The command to execute when the handler gets called.
        /// </summary>
        [Description("The command to execute when the handler gets called.")]
        [XmlElement("verb")]
        public Verb Verb { get; set; }

        // Preserve order
        private readonly C5.LinkedList<AutoPlayEvent> _events = new C5.LinkedList<AutoPlayEvent>();
        /// <summary>
        /// The IDs of the events this action can handle.
        /// </summary>
        [Description("The IDs of the events this action can handle.")]
        [XmlElement("event")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.LinkedList<AutoPlayEvent> Events { get { return _events; } }

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs
        {
            get { return new[] {"autoplay:" + ID, "progid:" + ProgID}; }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "AutoPlay: ID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AutoPlay: {0}", ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability CloneCapability()
        {
            var capability = new AutoPlay {ID = ID, Provider = Provider, ProgID = ProgID, Verb = Verb.CloneVerb()};
            capability.Icons.AddAll(Icons);
            foreach (var description in Descriptions) capability.Descriptions.Add(description.CloneString());
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
                other.Provider == Provider && other.ProgID == ProgID && Equals(other.Verb, Verb) &&
                Events.SequencedEquals(other.Events);
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
                result = (result * 397) ^ (Provider ?? "").GetHashCode();
                result = (result * 397) ^ (ProgID ?? "").GetHashCode();
                result = (result * 397) ^ Verb.GetHashCode();
                result = (result * 397) ^ Events.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
