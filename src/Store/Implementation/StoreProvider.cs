/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Provides access to <see cref="IStore"/> implementations.
    /// </summary>
    public static class StoreProvider
    {
        private static readonly IStore _default = new StoreSet(new IStore[] {new ServiceStore(), new DirectoryStore()});
        /// <summary>
        /// Returns an implementation of <see cref="IStore"/> that uses the default cache locations.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static IStore Default { get { return _default; } }
    }
}
