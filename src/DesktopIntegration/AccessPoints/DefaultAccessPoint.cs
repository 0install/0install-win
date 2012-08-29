/*
 * Copyright 2010-2012 Bastian Eicher
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
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Makes an application the default handler for something.
    /// </summary>
    /// <seealso cref="ZeroInstall.Model.Capabilities.Capability"/>
    [XmlType("default-access-point", Namespace = AppList.XmlNamespace)]
    public abstract class DefaultAccessPoint : AccessPoint
    {
        #region Constants
        /// <summary>
        /// The name of this category of <see cref="AccessPoint"/>s as used by command-line interfaces.
        /// </summary>
        public const string CategoryName = "defaults";
        #endregion

        #region Properties
        /// <summary>
        /// The ID of the <see cref="Capability"/> to be made the default handler.
        /// </summary>
        [Description("The ID of the Capability to be made the default handler.")]
        [XmlAttribute("capability")]
        public string Capability { get; set; }
        #endregion

        /// <summary>
        /// Creates a <see cref="DefaultAccessPoint"/> referencing a specific <see cref="Capabilities.DefaultCapability"/>.
        /// </summary>
        /// <param name="capability">The <see cref="Capabilities.DefaultCapability"/> to create a <see cref="DefaultAccessPoint"/> for.</param>
        /// <returns>The newly created <see cref="DefaultAccessPoint"/>.</returns>
        public static DefaultAccessPoint FromCapability(Capabilities.DefaultCapability capability)
        {
            #region Sanity checks
            if (capability == null) throw new ArgumentNullException("capability");
            #endregion

            DefaultAccessPoint accessPoint;
            if (capability is Capabilities.AutoPlay) accessPoint = new AutoPlay();
            else if (capability is Capabilities.ContextMenu) accessPoint = new ContextMenu();
            else if (capability is Capabilities.DefaultProgram) accessPoint = new DefaultProgram();
            else if (capability is Capabilities.FileType) accessPoint = new FileType();
            else if (capability is Capabilities.UrlProtocol) accessPoint = new UrlProtocol();
            else throw new ArgumentException("Unknown default capability type.");

            accessPoint.Capability = capability.ID;
            return accessPoint;
        }

        //--------------------//

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
            return (base.GetHashCode() * 397) ^ (Capability ?? "").GetHashCode();
        }
        #endregion
    }
}
