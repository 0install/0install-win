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
using System.Diagnostics.CodeAnalysis;

namespace Common.Undo
{
    /// <summary>
    /// An undo command that removes an element from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements the collection contains.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "The complete name is not ambiguous.")]
    public sealed class RemoveFromCollection<T> : CollectionCommand<T>
    {
        #region Constructor
        /// <summary>
        /// Creates a new remove from collection command.
        /// </summary>
        /// <param name="collection">The collection to be modified.</param>
        /// <param name="element">The element to be removed from <paramref name="collection"/>.</param>
        public RemoveFromCollection(ICollection<T> collection, T element) : base(collection, element)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Removes the <see cref="CollectionCommand{T}.Element"/> from the <see cref="CollectionCommand{T}.Collection"/>.
        /// </summary>
        protected override void OnExecute()
        {
            Collection.Remove(Element);
        }
        #endregion

        #region Undo
        /// <summary>
        /// Adds the <see cref="CollectionCommand{T}.Element"/> to the <see cref="CollectionCommand{T}.Collection"/>.
        /// </summary>
        protected override void OnUndo()
        {
            Collection.Add(Element);
        }
        #endregion
    }
}
