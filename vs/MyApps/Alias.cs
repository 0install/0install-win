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
    /// Registers a command-line alias for launching the application.
    /// </summary>
    public class Alias : Integration, IEquatable<Alias>
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
        /// Creates a deep copy of this <see cref="Alias"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Alias"/>.</returns>
        public override Integration CloneIntegration()
        {
            return new Alias {Name = Name};
        }
        #endregion

        #region Equality
        public bool Equals(Alias other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Alias) && Equals((Alias)obj);
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
