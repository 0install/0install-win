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
using System.Linq;
using System.Xml.Serialization;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Restricts the versions of an <see cref="Implementation"/> that are allowed without creating a dependency on the implementation if its was not already chosen.
    /// </summary>
    [Description("Restricts the versions of an Implementation that are allowed without creating a dependency on the implementation if its was not already chosen.")]
    [Serializable]
    [XmlRoot("restriction", Namespace = Feed.XmlNamespace), XmlType("restriction", Namespace = Feed.XmlNamespace)]
    public class Restriction : FeedElement, ICloneable, IEquatable<Restriction>
    {
        #region Properties
        /// <summary>
        /// The URI or local path used to identify the interface.
        /// </summary>
        [Description("The URI or local path used to identify the interface.")]
        [XmlAttribute("interface")]
        public string InterfaceID { get; set; }

        /// <summary>
        /// Determines for which operating systems this dependency is required.
        /// </summary>
        [Description("Determines for which operating systems this dependency is required.")]
        [XmlAttribute("os"), DefaultValue(typeof(OS), "All")]
        public OS OS { get; set; }

        /// <summary>
        /// Specifies that the selected implementation must be from the given distribution (e.g. Debian, RPM).
        /// The special value '0install' may be used to require an implementation provided by Zero Install (i.e. one not provided by a <see cref="PackageImplementation"/>). 
        /// </summary>
        [Description("Specifies that the selected implementation must be from the given distribution (e.g. Debian, RPM).\nThe special value '0install' may be used to require an implementation provided by Zero Install (i.e. one not provided by a <package-implementation>).")]
        [XmlAttribute("distribution")]
        [TypeConverter(typeof(DistributionNameConverter))]
        public string Distribution { get; set; }

        /// <summary>
        /// A more flexible alternative to <see cref="Constraints"/>.
        /// Each range is in the form "START..!END". The range matches versions where START &lt;= VERSION &lt; END. The start or end may be omitted. A single version number may be used instead of a range to match only that version, or !VERSION to match everything except that version.
        /// </summary>
        [Description("A more flexible alternative to Constraints.\nEach range is in the form \"START..!END\". The range matches versions where START < VERSION < END. The start or end may be omitted. A single version number may be used instead of a range to match only that version, or !VERSION to match everything except that version.")]
        [XmlIgnore]
        public VersionRange Versions { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Versions"/>
        [XmlAttribute("version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string VersionsString { get { return (Versions == null) ? null : Versions.ToString(); } set { Versions = string.IsNullOrEmpty(value) ? null : new VersionRange(value); } }

        private readonly List<Constraint> _constraints = new List<Constraint>();

        /// <summary>
        /// A list of version <see cref="Constraint"/>s that must be fulfilled.
        /// </summary>
        [Browsable(false)]
        [XmlElement("version")]
        public List<Constraint> Constraints { get { return _constraints; } }
        #endregion

        //--------------------//

        #region Normalize
        /// <summary>
        /// Handles legacy elements (converts <see cref="Constraints"/> to <see cref="Versions"/>).
        /// </summary>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing. Do not call it if you plan on serializing the feed again since it may loose some of its structure.</remarks>
        public void Normalize()
        {
            Versions = Constraints.Aggregate(Versions ?? new VersionRange(), (current, constraint) => current.Intersect(constraint));
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the dependency in the form "Interface". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return InterfaceID;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Restriction"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Restriction"/>.</returns>
        public virtual Restriction Clone()
        {
            var restriction = new Restriction {InterfaceID = InterfaceID, OS = OS, Distribution = Distribution, Versions = Versions};
            restriction.Constraints.AddRange(Constraints.CloneElements());
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
            return base.Equals(other) && InterfaceID == other.InterfaceID && OS == other.OS && Versions == other.Versions && Constraints.SequencedEquals(other.Constraints);
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
                result = (result * 397) ^ (InterfaceID ?? "").GetHashCode();
                result = (result * 397) ^ OS.GetHashCode();
                result = (result * 397) ^ Constraints.GetSequencedHashCode();
                if (Versions != null) result = (result * 397) ^ Versions.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
