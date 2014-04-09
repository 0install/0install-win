/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;

namespace NanoByte.Common.Collections
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
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (key == null) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");
            // ReSharper restore CompareNonConstrainedGenericWithNull
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
            return obj is ComparableTuple<T> && Equals((ComparableTuple<T>)obj);
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
