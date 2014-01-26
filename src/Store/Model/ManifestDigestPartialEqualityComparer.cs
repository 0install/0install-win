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

using System.Collections.Generic;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Uses <see cref="ManifestDigest.PartialEquals"/> to compare <see cref="ManifestDigest"/>s.
    /// </summary>
    public class ManifestDigestPartialEqualityComparer : IEqualityComparer<ManifestDigest>
    {
        /// <inheritdoc/>
        public bool Equals(ManifestDigest x, ManifestDigest y)
        {
            return x.PartialEquals(y);
        }

        /// <inheritdoc/>
        public int GetHashCode(ManifestDigest obj)
        {
            // Cannot hash with partial equality
            return 0;
        }
    }

    /// <summary>
    /// Uses <see cref="ManifestDigest.PartialEquals"/> to compare <see cref="ImplementationBase"/>s.
    /// </summary>
    public class ManifestDigestPartialEqualityComparer<T> : IEqualityComparer<T> where T : ImplementationBase
    {
        /// <inheritdoc/>
        public bool Equals(T x, T y)
        {
            if (x == null || y == null) return false;
            return x.ManifestDigest.PartialEquals(y.ManifestDigest);
        }

        /// <inheritdoc/>
        public int GetHashCode(T obj)
        {
            // Cannot hash with partial equality
            return 0;
        }
    }
}
