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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An object that contains <see cref="Group"/>s and <see cref="FeedReference"/>s. Supports a composite pattern.
    /// </summary>
    interface IGroupContainer
    {
        /// <summary>
        /// A list of <see cref="Group"/>s contained within this element.
        /// </summary>
        [Category("Implementation"), Description("A list of groups contained within this element.")]
        [XmlElement("group")]
        Collection<Group> Groups { get; }

        #region Implementations
        /// <summary>
        /// A list of <see cref="Implementation"/>s contained within this element.
        /// </summary>
        [Category("Implementation"), Description("A list of implementations contained within this element.")]
        [XmlElement("implementation")]
        Collection<Implementation> Implementations { get; }

        /// <summary>
        /// A list of distribution-provided <see cref="PackageImplementation"/>s contained within this element.
        /// </summary>
        [Category("Implementation"), Description("A list of distribution-provided package implementations contained within this element.")]
        [XmlElement("implementation")]
        Collection<PackageImplementation> PackageImplementations { get; }
        #endregion
    }
}
