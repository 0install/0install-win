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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NanoByte.Common;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Models information about elements in a cache for display in a UI.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for INamed sorting")]
    public abstract class CacheNode : Node, INamed<CacheNode>
    {
        /// <summary>
        /// A counter that can be used to prevent naming collisions.
        /// </summary>
        /// <remarks>If this value is not zero it is appended to the <see cref="Node.Name"/>.</remarks>
        public int SuffixCounter;

        /// <summary>
        /// Deletes this element from the cache it is stored in.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if no matching element could be found in the cache.</exception>
        /// <exception cref="IOException">Thrown if the element could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        public abstract void Delete();

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(CacheNode other)
        {
            #region Sanity checks
            if (other == null) throw new ArgumentNullException("other");
            #endregion

            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
