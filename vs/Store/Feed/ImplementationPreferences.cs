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

using System.ComponentModel;
using System.Xml.Serialization;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// Stores user-specific preferences for an <see cref="Implementation"/>.
    /// </summary>
    [XmlType("implementation-preferences", Namespace = Model.Feed.XmlNamespace)]
    public sealed class ImplementationPreferences : XmlUnknown
    {
        #region Properties
        private Stability _userStability = Stability.Unset;
        /// <summary>
        /// A user-specified override for <see cref="Element.Stability"/> specified in the feed.
        /// </summary>
        [Description("A user-specified override for the implementation stability specified in the feed.")]
        [XmlAttribute("user-stability"), DefaultValue(typeof(Stability), "Unset")]
        public Stability UserStability { get { return _userStability; } set { _userStability = value; } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ImplementationPreferences"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ImplementationPreferences"/>.</returns>
        public ImplementationPreferences CloneImplementationPreferences()
        {
            var feedPreferences = new ImplementationPreferences {UserStability = UserStability};

            return feedPreferences;
        }

        public object Clone()
        {
            return CloneImplementationPreferences();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the preferences in the form "ImplementationPreferences: UserStability". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ImplementationPreferences: {0}", UserStability);
        }
        #endregion

        #region Equality
        public bool Equals(ImplementationPreferences other)
        {
            if (ReferenceEquals(null, other)) return false;

            return UserStability == other.UserStability;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ImplementationPreferences) && Equals((ImplementationPreferences)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return UserStability.GetHashCode();
            }
        }
        #endregion
    }
}
