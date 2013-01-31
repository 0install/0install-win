/*
 * Copyright 2010-2013 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// An object that contains <see cref="Dependency"/>s and <see cref="Restriction"/>s.
    /// </summary>
    public interface IDependencyContainer
    {
        /// <summary>
        /// A list of interfaces this implementation depends upon.
        /// </summary>
        [Category("Execution"), Description("A list of interfaces this implementation depends upon.")]
        [XmlElement("requires")]
        C5.ArrayList<Dependency> Dependencies { get; }

        /// <summary>
        /// A list of interfaces that are restricted to specific versions when used.
        /// </summary>
        [Category("Execution"), Description("A list of interfaces that are restricted to specific versions when used.")]
        [XmlElement("restricts")]
        C5.ArrayList<Restriction> Restrictions { get; }
    }
}
