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
    internal sealed class DottedList : IEquatable<DottedList>, IComparable<DottedList>
    {
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
            if (other == null) return false;

            // Cancel if the the number of decimal blocks don't match
            if (_decimals.Length != other._decimals.Length)
                return false;

            // Cacnel if one of the decimal blocks does not match
            for (int i = 0; i < _decimals.Length; i++)
            {
                if (_decimals[i] != other._decimals[i])
                    return false;
            }

            // If we get here, everything was equal
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
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
            throw new NotImplementedException();
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
