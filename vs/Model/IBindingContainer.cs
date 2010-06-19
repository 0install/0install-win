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
using C5;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An object that contains <see cref="Binding"/>s.
    /// </summary>
    public interface IBindingContainer
    {
        /// <summary>
        /// A list of <see cref="EnvironmentBinding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Description("A list of bindings for implementatiosn to locate dependencies.")]
        [XmlElement("environment")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        HashedArrayList<EnvironmentBinding> EnvironmentBindings { get; }

        /// <summary>
        /// A list of <see cref="OverlayBinding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Description("A list of bindings for implementatiosn to locate dependencies.")]
        [XmlElement("overlay")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        HashedArrayList<OverlayBinding> OverlayBindings { get; }
    }
}
