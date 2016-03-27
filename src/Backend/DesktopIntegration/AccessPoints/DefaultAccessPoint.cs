/*
 * Copyright 2010-2016 Bastian Eicher
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
    /// Makes an application the default handler for something.
    /// </summary>
    /// <seealso cref="ZeroInstall.Store.Model.Capabilities.Capability"/>
    [XmlType("default-access-point", Namespace = AppList.XmlNamespace)]
    public abstract class DefaultAccessPoint : AccessPoint
    {
        #region Constants
        /// <summary>
        /// The name of this category of <see cref="AccessPoint"/>s as used by command-line interfaces.
        /// </summary>
        public const string CategoryName = "default-app";
        #endregion

        /// <summary>
        /// The ID of the <see cref="Capability"/> to be made the default handler.
        /// </summary>
        [Description("The ID of the Capability to be made the default handler.")]
        [XmlAttribute("capability")]
        public string Capability { get; set; }

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(DefaultAccessPoint other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Capability == Capability;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Capability ?? "").GetHashCode();
            }
        }
        #endregion
    }
}
