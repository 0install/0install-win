/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Linq;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A reference to an interface that is restricted to specific versions when used.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("restriction", Namespace = Feed.XmlNamespace)]
    public class Restriction : FeedElement, ICloneable, IEquatable<Restriction>
    {
        #region Properties
        /// <summary>
        /// The URI or local path used to identify the interface.
        /// </summary>
        [Description("The URI or local path used to identify the interface.")]
        [XmlAttribute("interface")]
        public string Interface { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Constraint> _constraints = new C5.ArrayList<Constraint>();

        /// <summary>
        /// A list of version <see cref="Constraint"/>s that must be fulfilled.
        /// </summary>
        [Description("A list of version constraints that must be fulfilled.")]
        [XmlElement("version")]
        public C5.ArrayList<Constraint> Constraints { get { return _constraints; } }

        /// <summary>
        /// A more flexible alternative to <see cref="Constraints"/>.
        /// </summary>
        [Description("A more flexible alternative to &lt;version&gt;s.")]
        [XmlIgnore]
        public VersionRange Versions { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Versions"/>
        [XmlAttribute("version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string VersionsString { get { return (Versions == null) ? null : Versions.ToString(); } set { Versions = string.IsNullOrEmpty(value) ? null : new VersionRange(value); } }

        /// <summary>
        /// A merged view of <see cref="Constraints"/> and <see cref="Versions"/>.
        /// </summary>
        public VersionRange EffectiveVersions { get { return Constraints.Aggregate(Versions ?? new VersionRange(), (current, constraint) => current.Intersect(constraint)); } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the dependency in the form "Restriction: Interface". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "Restriction: " + Interface;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Restriction"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Restriction"/>.</returns>
        public virtual Restriction Clone()
        {
            var restriction = new Restriction {Interface = Interface, Versions = Versions};
            foreach (var constraint in Constraints) restriction.Constraints.Add(constraint.Clone());

            return restriction;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Restriction other)
        {
            if (other == null) return false;
            return base.Equals(other) && Interface == other.Interface && Versions == other.Versions && Constraints.SequencedEquals(other.Constraints);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Restriction) && Equals((Restriction)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Interface ?? "").GetHashCode();
                result = (result * 397) ^ Constraints.GetSequencedHashCode();
                if (Versions != null) result = (result * 397) ^ Versions.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
