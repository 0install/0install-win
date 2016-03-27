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

using System.Collections.Generic;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Uses <see cref="ManifestDigest.PartialEquals"/> to compare <see cref="ManifestDigest"/>s.
    /// </summary>
    public sealed class ManifestDigestPartialEqualityComparer : IEqualityComparer<ManifestDigest>
    {
        /// <summary>A singleton instance of the comparer.</summary>
        public static readonly ManifestDigestPartialEqualityComparer Instance = new ManifestDigestPartialEqualityComparer();

        private ManifestDigestPartialEqualityComparer()
        {}

        /// <inheritdoc/>
        public bool Equals(ManifestDigest x, ManifestDigest y)
        {
            return x.PartialEquals(y);
        }

        /// <summary>
        /// Always returns 0. The concept of hashing is not applicable to partial equality.
        /// </summary>
        public int GetHashCode(ManifestDigest obj)
        {
            return 0;
        }
    }

    /// <summary>
    /// Uses <see cref="ManifestDigest.PartialEquals"/> to compare <see cref="ImplementationBase"/>s.
    /// </summary>
    public sealed class ManifestDigestPartialEqualityComparer<T> : IEqualityComparer<T> where T : ImplementationBase
    {
        /// <summary>A singleton instance of the comparer.</summary>
        public static readonly ManifestDigestPartialEqualityComparer<T> Instance = new ManifestDigestPartialEqualityComparer<T>();

        private ManifestDigestPartialEqualityComparer()
        {}

        /// <inheritdoc/>
        public bool Equals(T x, T y)
        {
            if (x == null || y == null) return false;
            return x.ManifestDigest.PartialEquals(y.ManifestDigest);
        }

        /// <summary>
        /// Always returns 0. The concept of hashing is not applicable to partial equality.
        /// </summary>
        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
