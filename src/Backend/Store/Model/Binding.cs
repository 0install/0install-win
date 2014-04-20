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

using System;
using System.Xml.Serialization;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Bindings specify how the chosen implementation is made known to the running program.
    /// </summary>
    /// <remarks>
    /// Bindings can appear in <see cref="Dependency"/>s, in which case they tell a component how to find its dependency,
    /// or in <see cref="Element"/>, where they tell a component how to find itself.
    /// </remarks>
    [XmlType("binding-base", Namespace = Feed.XmlNamespace)]
    public abstract class Binding : FeedElement, ICloneable
    {
        /// <summary>
        /// Creates a deep copy of this <see cref="Binding"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Binding"/>.</returns>
        public abstract Binding Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
