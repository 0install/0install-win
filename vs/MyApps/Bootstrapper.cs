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
using System.Xml.Serialization;

namespace ZeroInstall.MyApps
{
    /// <summary>
    /// Creates a bootstrapper executable for launching the application.
    /// </summary>
    [XmlType("bootstrapper", Namespace = AppList.XmlNamespace)]
    public class Bootstrapper : Integration, IEquatable<Bootstrapper>
    {
        #region Properties
        /// <summary>
        /// The name of the command-line alias.
        /// </summary>
        [Description("The name of the command-line alias.")]
        [XmlAttribute("name")]
        public string Name { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the integration in the form "Alias: Name". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "Alias: " + Name;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Bootstrapper"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Bootstrapper"/>.</returns>
        public override Integration CloneIntegration()
        {
            return new Bootstrapper {Name = Name};
        }
        #endregion

        #region Equality
        public bool Equals(Bootstrapper other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Bootstrapper) && Equals((Bootstrapper)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name != null ? Name.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
