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
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// An executable implementation of an <see cref="Interface"/>.
    /// </summary>
    /// <remarks>This class does not contain information on how to download the implementation in case it is not in cache. That must be obtained from a <see cref="Implementation"/> instance.</remarks>
    public sealed class ImplementationSelection : IDImplementation, ICloneable, IEquatable<ImplementationSelection>
    {
        #region Properties
        /// <summary>
        /// The URL of the interface this selection is for.
        /// </summary>
        [Description("The URL of the interface this selection is for.")]
        [XmlIgnore]
        public Uri Interface
        { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Interface"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false)]
        public String InterfaceString
        {
            get { return (Interface == null ? null : Interface.ToString()); }
            set { Interface = (value == null ? null : new Uri(value)); }
        }

        /// <summary>
        /// The name of the package in the distribution-specific package manager.
        /// </summary>
        [Category("Identity"), Description("The name of the package in the distribution-specific package manager.")]
        [XmlAttribute("package")]
        public string Package { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return base.ToString() + " (" + Interface + ")";
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ImplementationSelection"/>
        /// </summary>
        /// <returns>The cloned <see cref="ImplementationSelection"/>.</returns>
        public ImplementationSelection CloneImplementation()
        {
            var implementation = new ImplementationSelection {Interface = Interface, Package = Package};
            CloneFromTo(this, implementation);
            return implementation;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="ImplementationSelection"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ImplementationSelection"/> casted to a generic <see cref="object"/>.</returns>
        public object Clone()
        {
            return CloneImplementation();
        }
        #endregion

        #region Equality
        public bool Equals(ImplementationSelection other)
        {
            if (ReferenceEquals(null, other)) return false;

            return base.Equals(other) && Equals(other.Interface, Interface) && Equals(other.Package, Package);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ImplementationSelection) && Equals((ImplementationSelection)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Interface != null ? Interface.GetHashCode() : 0);
                result = (result * 397) ^ (Package != null ? Package.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
