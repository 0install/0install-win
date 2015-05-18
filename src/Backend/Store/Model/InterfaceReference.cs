/*
 * Copyright 2010-2015 Bastian Eicher
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

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// A reference to an interface URI, e.g. for specifying which interface this feed implements or by which interface it is replaced.
    /// </summary>
    /// <seealso cref="Feed.FeedFor"/>
    /// <seealso cref="Feed.ReplacedBy"/>
    [Description("A reference to an interface URI, e.g. for specifying which interface this feed implements or by which interface it is replaced.")]
    [Serializable]
    [XmlRoot("interface-reference", Namespace = Feed.XmlNamespace), XmlType("interface-reference", Namespace = Feed.XmlNamespace)]
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
        public string TargetString { get { return (Target == null ? null : Target.ToStringRfc()); } set { Target = (string.IsNullOrEmpty(value) ? null : new FeedUri(value)); } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the interface reference in the form "Target". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}", Target);
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
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
