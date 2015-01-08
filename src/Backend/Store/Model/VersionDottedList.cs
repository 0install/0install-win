using System;
using System.Text;
using System.Text.RegularExpressions;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
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
    internal struct VersionDottedList : IEquatable<VersionDottedList>, IComparable<VersionDottedList>
    {
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

            for (int i = 0; i < parts.Length; i++)
            {
                if (!long.TryParse(parts[i], out _decimals[i]))
                    throw new ArgumentException(Resources.MustBeDottedList);
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            if (_decimals == null) return "";

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
        public bool Equals(VersionDottedList other)
        {
            if (_decimals == null || other._decimals == null)
                return (_decimals == other._decimals);

            return _decimals.SequencedEquals(other._decimals);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VersionDottedList && Equals((VersionDottedList)obj);
        }

        public static bool operator ==(VersionDottedList left, VersionDottedList right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VersionDottedList left, VersionDottedList right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return (_decimals != null ? _decimals.GetSequencedHashCode() : 0);
        }
        #endregion

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(VersionDottedList other)
        {
            var leftArray = _decimals ?? new long[0];
            var rightArray = other._decimals ?? new long[0];

            int upperBound = Math.Max(leftArray.Length, rightArray.Length);
            for (var i = 0; i < upperBound; ++i)
            {
                long left = i >= leftArray.Length ? -1 : leftArray[i];
                long right = i >= rightArray.Length ? -1 : rightArray[i];
                int comparisonResult = left.CompareTo(right);
                if (comparisonResult != 0) return left.CompareTo(right);
            }
            return 0;
        }
        #endregion

        //--------------------//

        #region Static helpers
        private static readonly Regex _dottedListPattern = new Regex(@"^(\d+(\.\d+)*)$");

        /// <summary>
        /// Checks whether a string represents a valid dotted-list.
        /// </summary>
        public static bool IsValid(string value)
        {
            return _dottedListPattern.IsMatch(value);
        }
        #endregion
    }
}
