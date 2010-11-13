/*
 * Copyright 2010 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// A reference to a <see cref="Feed"/> URI.
    /// </summary> 
    /// <seealso cref="Feed.FeedFor"/>
    [Serializable]
    [XmlType("interface-reference", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    public sealed class InterfaceReference : XmlUnknown, ICloneable, IEquatable<InterfaceReference>
    {
        #region Properties
        /// <summary>
        /// The URI used to locate the interface.
        /// </summary>
        [Description("The URI used to locate the interface.")]
        [XmlIgnore]
        public Uri Target
        { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Target"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public String TargetString
        {
            get { return (Target == null ? null : Target.ToString()); }
            set { Target = (value == null ? null : new Uri(value)); }
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="InterfaceReference"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="InterfaceReference"/>.</returns>
        public InterfaceReference CloneReference()
        {
            return new InterfaceReference {Target = Target};
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="InterfaceReference"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="InterfaceReference"/>.</returns>
        public object Clone()
        {
            return CloneReference();
        }
        #endregion

        #region Equality
        public bool Equals(InterfaceReference other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Target == Target;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(InterfaceReference) && Equals((InterfaceReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TargetString != null ? TargetString.GetHashCode() : 0);
            }
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the interface reference in the form "InterfaceReference: Target". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "InterfaceReference: " + Target;
        }
        #endregion
    }
}
