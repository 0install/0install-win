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

using System;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Interface
{
    /// <summary>
    /// Manages the <see cref="Interface"/> cache.
    /// </summary>
    public class InterfaceProvider
    {
        #region Properties
        /// <summary>
        /// Set to <see langword="true"/> to prevent anything from being downloaded.
        /// </summary>
        public bool Offline { get; set; }

        /// <summary>
        /// Set to <see langword="true"/> to force all cached <see cref="Interface"/>s to be updated, no matter how old they are.
        /// </summary>
        /// <remarks>This will be ignored if <see cref="Offline"/> is set to <see langword="true"/>.</remarks>
        public bool Refresh { get; set; }

        /// <summary>
        /// The maximum age a cached <see cref="Interface"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        public int MaximumAge { get; set; }
        #endregion

        //--------------------//

        #region Get
        /// <summary>
        /// Gets an <see cref="Interface"/> from the local cache or downloads it.
        /// </summary>
        /// <param name="source">The URI used to identify (and download) the <see cref="Interface"/>.</param>
        /// <returns>The parsed <see cref="Interface"/> object.</returns>
        public Model.Interface GetInterface(Uri source)
        {
            // ToDo: Implement
            throw new NotImplementedException();
        }
        #endregion
    }
}
