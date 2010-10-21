/*
 * Copyright 2010 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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
    /// Make a chosen <see cref="Implementation"/> available  by setting environment variables.
    /// </summary>
    [XmlType("environment", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
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
        /// The relative path of the item within the implementation to insert into the variable's value. Use <code>.</code> to publish the root directory.
        /// </summary>
        [Description("The relative path of the item within the implementation to insert into the variable's value. Use \".\" to publish the root directory")]
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
        [XmlAttribute("default"), DefaultValue("")]
        public string Default { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the binding in the form "EnvironmentBinding: Name = Value (Mode, Default)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return (Mode == EnvironmentMode.Replace)
                ? string.Format("EnvironmentBinding: {0} = {1} ({2})", Name, Value, Mode)
                : string.Format("EnvironmentBinding: {0} = {1} ({2}, {3})", Name, Value, Mode, Default);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="EnvironmentBinding"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="EnvironmentBinding"/>.</returns>
        public override Binding CloneBinding()
        {
            return new EnvironmentBinding { Name = Name, Value = Value, Mode = Mode, Default = Default };
        }
        #endregion

        #region Equality
        public bool Equals(EnvironmentBinding other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Name == Name || other.Value == Value || other.Mode == Mode || other.Default == Default;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(EnvironmentBinding) && Equals((EnvironmentBinding)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name != null ? Name.GetHashCode() : 0);
                result = (result * 397) ^ (Value ?? "").GetHashCode();
                result = (result * 397) ^ Mode.GetHashCode();
                result = (result * 397) ^ (Default ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
