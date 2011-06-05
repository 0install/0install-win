/*
 * Copyright 2006-2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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

namespace Common.Collections
{
    /// <summary>
    /// Represents a tuple of comparable elements.
    /// </summary>
    [Serializable]
    public struct ComparableTuple<T> : IEquatable<ComparableTuple<T>>, IComparable<ComparableTuple<T>>
        where T : IEquatable<T>, IComparable<T>
    {
        #region Variables
        /// <summary>
        /// The first element of the tuple.
        /// </summary>
        public readonly T Key;

        /// <summary>
        /// The second element of the tuple.
        /// </summary>
        public readonly T Value;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new comparable tuple.
        /// </summary>
        /// <param name="key">The first element of the tuple; may not be <see langword="null"/>!</param>
        /// <param name="value">The second element of the tuple; may not be <see langword="null"/>!</param>
        public ComparableTuple(T key, T value)
        {
            #region Sanity checks
            if (key == null) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            Key = key;
            Value = value;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the tuple in the form "Key = Value". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Key + " = " + Value;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ComparableTuple<T> other)
        {
            return Equals(Key, other.Key) && Equals(Value, other.Value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(ComparableTuple<T>) && Equals((ComparableTuple<T>)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Key.GetHashCode();
                result = (result * 397) ^ Value.GetHashCode();
                return result;
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(ComparableTuple<T> left, ComparableTuple<T> right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(ComparableTuple<T> left, ComparableTuple<T> right)
        {
            return !left.Equals(right);
        }
        #endregion

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(ComparableTuple<T> other)
        {
            // Compare by Key first, then by Value if that was equal
            int keyCompare = Key.CompareTo(other.Key);
            return (keyCompare == 0) ? Value.CompareTo(other.Value) : keyCompare;
        }

        /// <inheritdoc/>
        public static bool operator <(ComparableTuple<T> left, ComparableTuple<T> right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <inheritdoc/>
        public static bool operator >(ComparableTuple<T> left, ComparableTuple<T> right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <inheritdoc/>
        public static bool operator <=(ComparableTuple<T> left, ComparableTuple<T> right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <inheritdoc/>
        public static bool operator >=(ComparableTuple<T> left, ComparableTuple<T> right)
        {
            return left.CompareTo(right) >= 0;
        }
        #endregion
    }
}
