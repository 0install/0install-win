using System;
using System.Text;
using System.Text.RegularExpressions;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Immutably stores a version number consisting of dot-separated decimals and optional modifier strings.
    /// </summary>
    /// <remarks>
    /// This defines the valid syntax for version strings:
    /// <code>
    /// Version := DottedList ("-" Modifier? DottedList?)*
    /// DottedList := (Integer ("." Integer)*)
    /// Modifier := "pre" | "rc" | "post"
    /// </code>
    /// </remarks>
    public sealed class ImplementationVersion : IEquatable<ImplementationVersion>, IComparable<ImplementationVersion>
    {
        #region Variables
        /// <summary>The first part of the version number.</summary>
        private readonly DottedList _firstPart;

        /// <summary>All additional parts of the version number.</summary>
        private readonly VersionPart[] _additionalParts;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new version from a a string.
        /// </summary>
        /// <param name="value">The string containing the version information.</param>
        public ImplementationVersion(string value)
        {
            string[] parts = value.Split('-');

            // Ensure the first part is a dotted list
            if (!DottedList.IsValid(parts[0])) throw new ArgumentException(Resources.MustStartWithDottedList, "value");
            _firstPart = new DottedList(parts[0]);

            // Iterate through all additional parts
            _additionalParts = new VersionPart[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
            {
                // Ensure the additional parts are either dotted lists or version modifiers
                if (DottedList.IsValid(parts[i])) _additionalParts[i - 1] = new DottedList(parts[i]);
                else if (VersionModifier.IsValid(parts[i])) _additionalParts[i - 1] = new VersionModifier(parts[i]);
                else throw new ArgumentException(Resources.MustBeValidVersionPart, "value");
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            var output = new StringBuilder();
            for (int i = 0; i < _additionalParts.Length; i++)
            {
                output.Append(_additionalParts[i]);

                // Separate parts with hyphens, no trailing hyphen
                if (i < _additionalParts.Length - 1) output.Append("-");
            }

            return output.ToString();
        }
        #endregion

        #region Equality
        public bool Equals(ImplementationVersion other)
        {
            if (other == null) return false;

            // Cancel if the first part of the version or the number of additional parts don't match
            if (!_firstPart.Equals(other._firstPart) ||
                _additionalParts.Length != other._additionalParts.Length)
                return false;

            // Cacnel if one of the additional parts does not match
            for (int i = 0; i < _additionalParts.Length; i++)
            {
                if (!_additionalParts[i].Equals(other._additionalParts[i]))
                    return false;
            }

            // If we get here, everything was equal
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(ImplementationVersion) && Equals((ImplementationVersion)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _firstPart.GetHashCode();
                foreach (var part in _additionalParts)
                    result = (result * 397) ^ part.GetHashCode();
                return result;
            }
        }
        #endregion

        #region Comparison
        public int CompareTo(ImplementationVersion other)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
