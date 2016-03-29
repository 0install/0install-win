/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Tasks;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Models information about elements in a cache for display in a UI.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for INamed sorting")]
    public abstract class CacheNode : INamed<CacheNode>, IEquatable<CacheNode>
    {
        /// <summary>
        /// The UI path name of this node. Uses a backslash as the separator in hierarchical names.
        /// </summary>
        [Browsable(false)]
        public virtual string Name { get; set; }

        /// <summary>
        /// A counter that can be used to prevent naming collisions.
        /// </summary>
        /// <remarks>If this value is not zero it is appended to the <see cref="Name"/>.</remarks>
        public int SuffixCounter;

        /// <summary>
        /// Deletes this element from the cache it is stored in.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about IO tasks.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="KeyNotFoundException">No matching element could be found in the cache.</exception>
        /// <exception cref="IOException">The element could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the cache is not permitted.</exception>
        public abstract void Delete(ITaskHandler handler);

        #region Equality
        /// <inheritdoc/>
        public bool Equals(CacheNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CacheNode)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
        #endregion

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
