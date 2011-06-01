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

using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// An access points that directly points to a <see cref="ZeroInstall.Model.Command"/>.
    /// </summary>
    [XmlType("command-access-point", Namespace = XmlNamespace)]
    public abstract class CommandAccessPoint : AccessPoint
    {
        #region Properties
        /// <summary>
        /// The name of the command in the <see cref="ZeroInstall.Model.Feed"/> to use when launching via this access point.
        /// </summary>
        [Description("The name of the command in the feed to use when launching via this access point.")]
        [XmlAttribute("command")]
        public string Command { get; set; }
        #endregion

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(CommandAccessPoint other)
        {
            if (other == null) return false;

            return other.Command == Command;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Command ?? "").GetHashCode();
        }
        #endregion
    }
}
