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

using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Abstract base class for capabilities that can have multiple <see cref="Verb"/>s associated with them.
    /// </summary>
    public abstract class VerbCapability : Capability
    {
        #region Properties
        /// <summary>
        /// A human-readable description.
        /// </summary>
        [Description("A human-readable description.")]
        [XmlAttribute("description"), DefaultValue("")]
        public string Description { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Icon> _icons = new C5.ArrayList<Icon>();
        /// <summary>
        /// Zero or more icons to represent the element or the application.
        /// </summary>
        /// <remarks>The first compatible icon is selected. If empty <see cref="Feed.Icons"/> is used.</remarks>
        [Description("Zero or more icons to represent the element or the application. (The first compatible icon is selected. If empty the main feed icon is used.)")]
        [XmlElement("icon", Namespace = Feed.XmlNamespace)]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<Icon> Icons { get { return _icons; } }

        // Preserve order
        private readonly C5.ArrayList<Verb> _verbs = new C5.ArrayList<Verb>();
        /// <summary>
        /// A list of all available operations for the element.
        /// </summary>
        [Description("A list of all available operations for the element.")]
        [XmlElement("verb")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<Verb> Verbs { get { return _verbs; } }
        #endregion

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(VerbCapability other)
        {
            if (other == null) return false;

            return base.Equals(other) && other.Description == Description && Icons.SequencedEquals(other.Icons) && Verbs.SequencedEquals(other.Verbs);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(UrlProtocol) && Equals((UrlProtocol)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Description ?? "").GetHashCode();
                result = (result * 397) ^ Icons.GetSequencedHashCode();
                result = (result * 397) ^ Verbs.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
