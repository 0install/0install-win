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

using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Adds a way to explicitly launch the application to the desktop environment.
    /// </summary>
    /// <seealso cref="ZeroInstall.Store.Model.Command"/>
    [XmlType("command-access-point", Namespace = AppList.XmlNamespace)]
    public abstract class CommandAccessPoint : AccessPoint
    {
        #region Properties
        /// <summary>
        /// The name of the menu entry, icon, command-line, etc..
        /// </summary>
        [Description("The name of the menu entry, icon, command-line, etc..")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The name of the <see cref="Store.Model.Command"/> to use when launching via this access point. Leave empty to use default.
        /// </summary>
        [Description("The name of the Command to use when launching via this access point. Leave empty to use default.")]
        [XmlAttribute("command")]
        public string Command { get; set; }
        #endregion

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(CommandAccessPoint other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Name == Name && other.Command == Command;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Command != null ? Command.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}
