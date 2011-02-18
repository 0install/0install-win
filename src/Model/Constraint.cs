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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Restricts the set of versions from which the injector may choose an <see cref="Implementation"/>. 
    /// </summary>
    [Serializable]
    [XmlType("constraint", Namespace = Feed.XmlNamespace)]
    public class Constraint : XmlUnknown, ICloneable, IEquatable<Constraint>
    {
        #region Properties
        /// <summary>
        /// This is the lowest-numbered version that can be chosen.
        /// </summary>
        [Description("This is the lowest-numbered version that can be chosen.")]
        [XmlIgnore]
        public ImplementationVersion NotBeforeVersion { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="NotBeforeVersion"/>
        [XmlAttribute("not-before"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NotBeforeVersionString
        {
            get { return (NotBeforeVersion == null ? null : NotBeforeVersion.ToString()); }
            set { NotBeforeVersion = string.IsNullOrEmpty(value) ? null : new ImplementationVersion(value); }
        }

        /// <summary>
        /// This version and all later versions are unsuitable.
        /// </summary>
        [Description("This version and all later versions are unsuitable.")]
        [XmlIgnore]
        public ImplementationVersion BeforeVersion { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="BeforeVersion"/>
        [XmlAttribute("before"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BeforeVersionString
        {
            get { return (BeforeVersion == null ? null : BeforeVersion.ToString()); }
            set { BeforeVersion = string.IsNullOrEmpty(value) ? null : new ImplementationVersion(value); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty constraint
        /// </summary>
        public Constraint()
        {}

        /// <summary>
        /// Creates a new constraint structure with pre-set values.
        /// </summary>
        /// <param name="notBeforeVersion">This is the lowest-numbered version that can be chosen.</param>
        /// <param name="beforeVersion">This version and all later versions are unsuitable.</param>
        public Constraint(ImplementationVersion notBeforeVersion, ImplementationVersion beforeVersion) : this()
        {
            NotBeforeVersion = notBeforeVersion;
            BeforeVersion = beforeVersion;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the constraint in the form "Constraint: NotBeforeVersion =&lt; Ver %lt; BeforeVersion". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("Constraint: {0} =< Ver < {1}", NotBeforeVersion, BeforeVersion);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a copy of this <see cref="Constraint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Constraint"/>.</returns>
        public Constraint CloneConstraint()
        {
            return new Constraint {NotBeforeVersion = NotBeforeVersion, BeforeVersion = BeforeVersion};
        }

        /// <summary>
        /// Creates a copy of this <see cref="Constraint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Constraint"/> casted to a generic <see cref="object"/>.</returns>
        public object Clone()
        {
            return CloneConstraint();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Constraint other)
        {
            if (other == null) return false;

            return other.NotBeforeVersion == NotBeforeVersion && other.BeforeVersion == BeforeVersion;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Constraint) && Equals((Constraint)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((NotBeforeVersion != null ? NotBeforeVersion.GetHashCode() : 0) * 397) ^ (BeforeVersion != null ? BeforeVersion.GetHashCode() : 0);
            }
        }
        #endregion
    }
}
