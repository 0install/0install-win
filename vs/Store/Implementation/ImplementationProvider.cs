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
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Manages a set of <see cref="Store"/>s, allowing the retrieval of <see cref="Implementation"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class ImplementationProvider
    {
        #region Properties
        // Preserve order, duplicate entries are not allowed
        private readonly C5.IList<Store> _stores = new C5.HashedArrayList<Store>();
        /// <summary>
        /// A priority-sorted list of <see cref="Store"/>s used to provide <see cref="Implementation"/>s.
        /// </summary>
        public IEnumerable<Store> Stores { get { return _stores; } }
        #endregion

        //--------------------//

        #region Get
        /// <summary>
        /// Gets an <see cref="Model.Implementation"/> from the local cache or downloads it.
        /// </summary>
        /// <param name="implementation">The implementation to get.</param>
        /// <returns>The local path containing the implementation.</returns>
        public string GetImplementation(Model.Implementation implementation)
        {
            return null;
        }
        #endregion
    }
}
