﻿/*
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

using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// An access points that creates some form of icon in the dektop environment.
    /// </summary>
    [XmlType("icon-access-point", Namespace = AppList.XmlNamespace)]
    public abstract class IconAccessPoint : CommandAccessPoint
    {
        #region Properties
        /// <summary>
        /// The user-defined override for name of the icon.
        /// </summary>
        [Description("The user-defined override for name of the icon.")]
        [XmlAttribute("name")]
        public string Name { get; set; }
        #endregion

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(IconAccessPoint other)
        {
            if (other == null) return false;

            return base.Equals(other) && other.Name == Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Name ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
