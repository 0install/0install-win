using System;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    #region Enumerations
    /// <see cref="VersionPart.Modifier"/>
    internal enum VersionModifier
    {
        /// <summary>No modifier; empty string</summary>
        None = 0,
        /// <summary>Pre-release</summary>
        Pre = -2,
        /// <summary>Release candidate</summary>
        RC  = -1,
        /// <summary>Post-release</summary>
        Post=  1
    }
    #endregion

    /// <summary>
    /// Represents a part of a <see cref="ImplementationVersion"/> containing nothing, a <see cref="VersionModifier"/>, a <see cref="DottedList"/> or both.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    [Serializable]
    internal class VersionPart : IEquatable<VersionPart>, IComparable<VersionPart>
    {
        #region Singleton fields
        /// <summary>
        /// A version number with the value -1.
        /// </summary>
        /// <remarks>-1 or "not set" has an even lower value than a set "0".</remarks>
        public static readonly VersionPart Default = new VersionPart("-1");
        #endregion

        #region Properties
        /// <summary>
        /// The modifier part of the version part; may be <see langword="null"/>.
        /// </summary>
        public VersionModifier Modifier { get; private set; }

        /// <summary>
        /// The dotted list part of the version part; may be <see langword="null"/>.
        /// </summary>
        public DottedList DottedList { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new dotted-list from a a string.
        /// </summary>
        /// <param name="value">The string containing the dotted-list.</param>
        public VersionPart(string value)
        {
            // Detect and trim version modifiers
            if (value.StartsWith("pre"))
            {
                value = value.Substring("pre".Length);
                Modifier = VersionModifier.Pre;
            }
            else if (value.StartsWith("rc"))
            {
                value = value.Substring("rc".Length);
                Modifier = VersionModifier.RC;
            }
            else if (value.StartsWith("post"))
            {
                value = value.Substring("post".Length);
                Modifier = VersionModifier.Post;
            }
            else
            {
                Modifier = VersionModifier.None;
            }

            // Parse any rest as dotted list
            if (!string.IsNullOrEmpty(value)) DottedList = new DottedList(value);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            string result;
            switch (Modifier)
            {
                case VersionModifier.None: result = ""; break;
                case VersionModifier.Pre: result = "pre"; break;
                case VersionModifier.RC: result = "rc"; break;
                case VersionModifier.Post: result = "post"; break;
                default: throw new InvalidOperationException(Resources.UnknownModifier);
            }

            // Combine both parts without any separator
            if (DottedList != null) result += DottedList;

            return result;
        }
        #endregion

        #region Equality
        public bool Equals(VersionPart other)
        {
            if (ReferenceEquals(null, other)) return false;

            return Equals(other.Modifier, Modifier) && Equals(other.DottedList, DottedList);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(VersionPart) && Equals((VersionPart)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Modifier.GetHashCode() * 397;
                if (DottedList != null) result = (result * 397) ^ DottedList.GetHashCode();
                return result;
            }
        }
        #endregion

        #region Comparison
        public int CompareTo(VersionPart other)
        {
            #region Sanity checks
            if (ReferenceEquals(null, other)) throw new ArgumentNullException("other");
            #endregion

            var modifierComparison = Modifier.CompareTo(other.Modifier);
            if (modifierComparison != 0) return modifierComparison;

            var leftDottedList = DottedList ?? DottedList.Default;
            var rightDottedList = other.DottedList ?? DottedList.Default;
            return leftDottedList.CompareTo(rightDottedList);
        }
        #endregion
    }
}
