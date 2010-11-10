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

using System.Collections.Generic;

namespace Common.Undo
{
    /// <summary>
    /// An undo command that adds or removes an element from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements the collection contains.</typeparam>
    public abstract class CollectionCommand<T> : SimpleCommand
    {
        #region Variables
        /// <summary>
        /// The collection to be modified.
        /// </summary>
        protected readonly ICollection<T> Collection;

        /// <summary>
        /// The element to be added or removed from <see cref="Collection"/>.
        /// </summary>
        protected readonly T Element;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new collection command.
        /// </summary>
        /// <param name="collection">The collection to be modified.</param>
        /// <param name="element">The element to be added or removed from <paramref name="collection"/>.</param>
        protected CollectionCommand(ICollection<T> collection, T element)
        {
            Collection = collection;
            Element = element;
        }
        #endregion
    }
}
