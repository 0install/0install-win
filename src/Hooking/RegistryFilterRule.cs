/*
 * Copyright 2006-2011 Bastian Eicher
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

namespace ZeroInstall.Hooking
{
    /// <summary>
    /// Represents a filtering/replacement rule in <see cref="RegistryFilter"/>.
    /// </summary>
    [Serializable]
    public struct RegistryFilterRule : IEquatable<RegistryFilterRule>, IComparable<RegistryFilterRule>
    {
        #region Variables
        /// <summary>
        /// The value as it is seen by the process.
        /// </summary>
        public readonly string ProcessValue;

        /// <summary>
        /// The value as it is actually stored in the registry.
        /// </summary>
        public readonly string RegistryValue;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new filter rule.
        /// </summary>
        /// <param name="processValue">The value as it is seen by the process; may not be <see langword="null"/>!</param>
        /// <param name="registryValue">The value as it is actually stored in the registry; may not be <see langword="null"/>!</param>
        public RegistryFilterRule(string processValue, string registryValue)
        {
            #region Sanity checks
            if (processValue == null) throw new ArgumentNullException("processValue");
            if (registryValue == null) throw new ArgumentNullException("registryValue");
            #endregion

            ProcessValue = processValue;
            RegistryValue = registryValue;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the rule in the form "ProcessValue = RegistryValue". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return ProcessValue + " = " + RegistryValue;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(RegistryFilterRule other)
        {
            return Equals(ProcessValue, other.ProcessValue) && Equals(RegistryValue, other.RegistryValue);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(RegistryFilterRule) && Equals((RegistryFilterRule)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = ProcessValue.GetHashCode();
                result = (result * 397) ^ RegistryValue.GetHashCode();
                return result;
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(RegistryFilterRule left, RegistryFilterRule right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(RegistryFilterRule left, RegistryFilterRule right)
        {
            return !left.Equals(right);
        }
        #endregion

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(RegistryFilterRule other)
        {
            // Compare by ProcessValue first, then by RegistryValue if that was equal
            int processCompare = ProcessValue.CompareTo(other.ProcessValue);
            return (processCompare == 0) ? RegistryValue.CompareTo(other.RegistryValue) : processCompare;
        }

        /// <inheritdoc/>
        public static bool operator <(RegistryFilterRule left, RegistryFilterRule right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <inheritdoc/>
        public static bool operator >(RegistryFilterRule left, RegistryFilterRule right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <inheritdoc/>
        public static bool operator <=(RegistryFilterRule left, RegistryFilterRule right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <inheritdoc/>
        public static bool operator >=(RegistryFilterRule left, RegistryFilterRule right)
        {
            return left.CompareTo(right) >= 0;
        }
        #endregion
    }
}
