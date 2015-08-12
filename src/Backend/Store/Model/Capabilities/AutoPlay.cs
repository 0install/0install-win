/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// An application's ability to handle one or more AutoPlay events.
    /// </summary>
    [Description("An application's ability to handle one or more AutoPlay events.")]
    [Serializable, XmlRoot("auto-play", Namespace = CapabilityList.XmlNamespace), XmlType("auto-play", Namespace = CapabilityList.XmlNamespace)]
    public sealed class AutoPlay : IconCapability, ISingleVerb, IEquatable<AutoPlay>
    {
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsMachineWideOnly => false;

        /// <summary>
        /// The name of the application as shown in the AutoPlay selection list.
        /// </summary>
        [Description("The name of the application as shown in the AutoPlay selection list.")]
        [XmlAttribute("provider")]
        public string Provider { get; set; }

        /// <summary>
        /// The command to execute when the handler gets called.
        /// </summary>
        [Browsable(false)]
        [XmlElement("verb"), CanBeNull]
        public Verb Verb { get; set; }

        /// <summary>
        /// The IDs of the events this action can handle.
        /// </summary>
        [Browsable(false)]
        [XmlElement("event"), NotNull]
        public List<AutoPlayEvent> Events { get; } = new List<AutoPlayEvent>();

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs => new[] {"autoplay:" + ID};

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "ID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return ID;
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            var capability = new AutoPlay {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID, ExplicitOnly = ExplicitOnly, Provider = Provider, Verb = Verb.Clone()};
            capability.Icons.AddRange(Icons);
            capability.Descriptions.AddRange(Descriptions.CloneElements());
            capability.Events.AddRange(Events);
            return capability;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AutoPlay other)
        {
            if (other == null) return false;
            return base.Equals(other) &&
                   other.Provider == Provider && Equals(other.Verb, Verb) &&
                   Events.UnsequencedEquals(other.Events);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is AutoPlay && Equals((AutoPlay)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Provider?.GetHashCode() ?? 0;
                result = (result * 397) ^ Provider?.GetHashCode() ?? 0;
                result = (result * 397) ^ Verb?.GetHashCode() ?? 0;
                result = (result * 397) ^ Events.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
