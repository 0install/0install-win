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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// A reference to an interface URI, e.g. for specifying which interface this feed implements or by which interface it is replaced.
    /// </summary>
    /// <seealso cref="Feed.FeedFor"/>
    /// <seealso cref="Feed.ReplacedBy"/>
    [Description("A reference to an interface URI, e.g. for specifying which interface this feed implements or by which interface it is replaced.")]
    [Serializable, XmlRoot("feed-for", Namespace = Feed.XmlNamespace), XmlType("feed-for", Namespace = Feed.XmlNamespace)]
    public sealed class InterfaceReference : FeedElement, ICloneable, IEquatable<InterfaceReference>
    {
        /// <summary>
        /// The URI used to locate the interface.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public FeedUri Target { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization and PropertyGrid.</summary>
        /// <seealso cref="Target"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [DisplayName(@"Target"), Description("The URI used to locate the interface.")]
        [XmlAttribute("interface"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string TargetString { get { return Target?.ToStringRfc(); } set { Target = (string.IsNullOrEmpty(value) ? null : new FeedUri(value)); } }
        #endregion

        #region Normalize
        /// <summary>
        /// Performs sanity checks.
        /// </summary>
        /// <exception cref="InvalidDataException">One or more required fields are not set.</exception>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing. Do not call it if you plan on serializing the feed again since it may loose some of its structure.</remarks>
        public void Normalize()
        {
            EnsureNotNull(Target, xmlAttribute: "interface", xmlTag: "feed-for");
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the interface reference in the form "Target". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return $"{Target}";
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="InterfaceReference"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="InterfaceReference"/>.</returns>
        public InterfaceReference Clone()
        {
            return new InterfaceReference {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Target = Target};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(InterfaceReference other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Target == Target;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is InterfaceReference && Equals((InterfaceReference)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (TargetString ?? "").GetHashCode();
            }
        }
        #endregion
    }
}
