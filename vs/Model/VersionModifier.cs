using System;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    #region Enumerations
    internal enum ModifierValue
    {
        /// <summary>Pre-release</summary>
        Pre,
        /// <summary>Release candidate</summary>
        RC,
        /// <summary>Post-release</summary>
        Post
    }
    #endregion

    /// <summary>
    /// Represents a modifier part of a <see cref="ImplementationVersion"/>.
    /// </summary>
    /// <remarks>
    /// This defines the valid syntax for version modifiers:
    /// <code>
    /// Modifier := "pre" | "rc" | "post"
    /// </code>
    /// </remarks>
    internal sealed class VersionModifier : VersionPart
    {
        #region Variables
        /// <summary>The specific version modifier.</summary>
        private ModifierValue _value;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new version modifier from a a string.
        /// </summary>
        /// <param name="value">The string containing the version modifier.</param>
        public VersionModifier(string value)
        {
            switch (value)
            {
                case "pre": _value = ModifierValue.Pre; break;
                case "rc": _value = ModifierValue.RC; break;
                case "post": _value = ModifierValue.Post; break;
                default: throw new InvalidOperationException(Resources.UnknownModifier);
            };
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            switch (_value)
            {
                case ModifierValue.Pre: return "pre";
                case ModifierValue.RC: return "rc";
                case ModifierValue.Post: return "post";
                default: throw new InvalidOperationException(Resources.UnknownModifier);
            };
        }
        #endregion

        #region Equality
        public override bool Equals(VersionPart other)
        {
            var otherModifier = other as VersionModifier;
            if (otherModifier == null) return false;

            return _value == otherModifier._value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(DottedList) && Equals((DottedList)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        #endregion

        #region Comparison
        public override int CompareTo(VersionPart other)
        {
            throw new NotImplementedException();
        }
        #endregion

        //--------------------//

        #region Static helpers
        /// <summary>
        /// Checks whether a string represents a valid version modifier.
        /// </summary>
        public static bool IsValid(string value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
