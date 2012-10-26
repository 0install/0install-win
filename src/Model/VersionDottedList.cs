﻿using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a dotted-list part of a <see cref="ImplementationVersion"/>.
    /// </summary>
    /// <remarks>
    /// This is the syntax for valid dot-separated decimals:
    /// <code>
    /// DottedList := (Integer ("." Integer)*)
    /// </code>
    /// </remarks>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    [Serializable]
    internal sealed class VersionDottedList : IEquatable<VersionDottedList>, IComparable<VersionDottedList>
    {
        #region Singleton fields
        /// <summary>
        /// A version number with the value 0.
        /// </summary>
        public static readonly VersionDottedList Default = new VersionDottedList("0");
        #endregion

        #region Variables
        /// <summary>The individual decimals.</summary>
        private readonly long[] _decimals;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new dotted-list from a a string.
        /// </summary>
        /// <param name="value">The string containing the dotted-list.</param>
        public VersionDottedList(string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            string[] parts = value.Split('.');
            _decimals = new long[parts.Length];

            // ReSharper disable LoopCanBeConvertedToQuery
            for (int i = 0; i < parts.Length; i++)
            {
                if (!long.TryParse(parts[i], out _decimals[i]))
                    throw new ArgumentException(Resources.MustBeDottedList);
            }
            // ReSharper restore LoopCanBeConvertedToQuery
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            var output = new StringBuilder();
            for (int i = 0; i < _decimals.Length; i++)
            {
                output.Append(_decimals[i]);

                // Separate parts with dots, no trailing dot
                if (i < _decimals.Length - 1) output.Append(".");
            }

            return output.ToString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(VersionDottedList other)
        {
            if (other == null) return false;

            // Cancel if the the number of decimal blocks don't match
            if (_decimals.Length != other._decimals.Length)
                return false;

            // Cacnel if one of the decimal blocks does not match
            return !_decimals.Where((part, i) => part != other._decimals[i]).Any();

            // If we reach this, everything was equal
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is VersionDottedList && Equals((VersionDottedList)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable LoopCanBeConvertedToQuery
                int result = 397;
                foreach (long dec in _decimals) result = (result * 397) ^ (int)dec;
                return result;
                // ReSharper restore LoopCanBeConvertedToQuery
            }
        }
        #endregion

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(VersionDottedList other)
        {
            #region Sanity checks
            if (other == null) throw new ArgumentNullException("other");
            #endregion

            int upperBound = Math.Max(_decimals.Length, other._decimals.Length);
            for (var i = 0; i < upperBound; ++i)
            {
                long left = i >= _decimals.Length ? -1 : _decimals[i];
                long right = i >= other._decimals.Length ? -1 : other._decimals[i];
                int comparisonResult = left.CompareTo(right);
                if (comparisonResult != 0) return left.CompareTo(right);
            }
            return 0;
        }
        #endregion

        //--------------------//

        #region Static helpers
        /// <summary>
        /// Checks whether a string represents a valid dotted-list.
        /// </summary>
        public static bool IsValid(string value)
        {
            return Regex.IsMatch(value, @"^(\d+(\.\d+)*)$");
        }
        #endregion
    }
}
