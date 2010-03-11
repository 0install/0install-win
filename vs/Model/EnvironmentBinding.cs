using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    #region Enumerations
    /// <summary>
    /// Controls how a <see cref="EnvironmentBinding.Value"/> is added to the variable.
    /// </summary>
    public enum EnvironmentMode
    {
        /// <summary>The absolute path of the item is prepended to the current value of the variable.</summary>
        [XmlEnum("prepend")] Prepend,

        /// <summary>The absolute path of the item is append to the end of the current value of the variable.</summary>
        [XmlEnum("append")] Append,

        /// <summary>The old value is overwritten, and the <see cref="EnvironmentBinding.Default"/> attribute is ignored.</summary>
        [XmlEnum("replace")] Replace
    }
    #endregion

    /// <summary>
    /// The location of the chosen <see cref="Implementation"/> is passed to the program by setting environment variables.
    /// </summary>
    public sealed class EnvironmentBinding : Binding, IEquatable<EnvironmentBinding>
    {
        #region Properties
        /// <summary>
        /// The name of the environment variable.
        /// </summary>
        [Description("The name of the environment variable.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The relative path of the item within the implementation to insert into the variable's value.
        /// </summary>
        [Description("The relative path of the item within the implementation to insert into the variable's value.")]
        [XmlAttribute("insert")]
        public string Value { get; set; }

        /// <summary>
        /// Controls how the <see cref="Value"/> is added to the variable.
        /// </summary>
        [Description("Controls how the value is added to the variable.")]
        [XmlAttribute("mode"), DefaultValue(typeof(EnvironmentMode), "Prepend")]
        public EnvironmentMode Mode { get; set; }

        /// <summary>
        /// If the environment variable is not currently set then this value is used for prepending or appending.
        /// </summary>
        [Description("If the environment variable is not currently set then this value is used for prepending or appending.")]
        [XmlAttribute("default")]
        public string Default { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            if (Mode == EnvironmentMode.Replace) return string.Format("{0} = {1} ({2})", Name, Value, Mode);
            return string.Format("{0} = {1} ({2}, Default: {3})", Name, Value, Mode, Default);
        }
        #endregion

        #region Equality
        public bool Equals(EnvironmentBinding other)
        {
            if (other == null) return false;
            if (ReferenceEquals(other, this)) return true;
            return other.Name == Name || other.Value == Value || other.Mode == Mode || other.Default == Default;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(EnvironmentBinding) && Equals((EnvironmentBinding)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name != null ? Name.GetHashCode() : 0);
                result = (result * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                result = (result * 397) ^ Mode.GetHashCode();
                result = (result * 397) ^ (Default != null ? Default.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
