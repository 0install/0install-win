using System;
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
    /// <remarks>This class is immutable.</remarks>
    [Serializable]
    internal sealed class DottedList : IEquatable<DottedList>, IComparable<DottedList>
    {
        #region Singleton fields
        /// <summary>
        /// A version number with the value 0.
        /// </summary>
        public static readonly DottedList Default = new DottedList("0");
        #endregion

        #region Variables
        /// <summary>The individual decimals.</summary>
        private readonly int[] _decimals;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new dotted-list from a a string.
        /// </summary>
        /// <param name="value">The string containing the dotted-list.</param>
        public DottedList(string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            string[] parts = value.Split('.');
            _decimals = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out _decimals[i]))
                    throw new ArgumentException(Resources.MustBeDottedList);
            }
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
        public bool Equals(DottedList other)
        {
            if (ReferenceEquals(null, other)) return false;

            // Cancel if the the number of decimal blocks don't match
            if (_decimals.Length != other._decimals.Length)
                return false;

            // Cacnel if one of the decimal blocks does not match
            for (int i = 0; i < _decimals.Length; i++)
            {
                if (_decimals[i] != other._decimals[i])
                    return false;
            }

            // If we reach this, everything was equal
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(DottedList) && Equals((DottedList)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 397;
                foreach (int dec in _decimals)
                    result = (result * 397) ^ dec;
                return result;
            }
        }
        #endregion

        #region Comparison
        public int CompareTo(DottedList other)
        {
            #region Sanity checks
            if (ReferenceEquals(null, other)) throw new ArgumentNullException("other");
            #endregion

            int upperBound = Math.Max(_decimals.Length, other._decimals.Length);
            for (var i = 0; i < upperBound; ++i)
            {
                int left = i >= _decimals.Length ? -1 : _decimals[i];
                int right = i >= other._decimals.Length ? -1 : other._decimals[i];
                int comparisonResult = left.CompareTo(right);
                if (comparisonResult != 0)
                    return left.CompareTo(right);
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
