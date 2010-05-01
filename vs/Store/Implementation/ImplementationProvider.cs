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

using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Manages a set of <see cref="Store"/>s, allowing the retrieval of <see cref="Implementation"/>s.
    /// </summary>
    public class ImplementationProvider
    {
        /// <summary>
        /// Gets an <see cref="Model.Implementation"/> from the local cache or downloads it.
        /// </summary>
        /// <param name="implementation">The implementation to get.</param>
        /// <returns>The local path containing the implementation.</returns>
        public string GetImplementation(Model.Implementation implementation)
        {
            return null;
        }
    }
}
