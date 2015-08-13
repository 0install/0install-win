/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;

namespace ZeroInstall.Store.Model
{

    #region Enumerations
    /// <summary>
    /// Controls how <see cref="EnvironmentBinding.Insert"/> or <see cref="EnvironmentBinding.Value"/> is added to a variable.
    /// </summary>
    public enum EnvironmentMode
    {
        /// <summary>The absolute path of the item is prepended to the current value of the variable.</summary>
        [XmlEnum("prepend")]
        Prepend,

        /// <summary>The absolute path of the item is append to the end of the current value of the variable.</summary>
        [XmlEnum("append")]
        Append,

        /// <summary>The old value is overwritten, and the <see cref="EnvironmentBinding.Default"/> attribute is ignored.</summary>
        [XmlEnum("replace")]
        Replace
    }
    #endregion

    /// <summary>
    /// Make a chosen <see cref="Implementation"/> available by setting environment variables.
    /// </summary>
    [Description("Make a chosen implementation available by setting environment variables.")]
    [Serializable, XmlRoot("environment", Namespace = Feed.XmlNamespace), XmlType("environment", Namespace = Feed.XmlNamespace)]
    public sealed class EnvironmentBinding : Binding, IEquatable<EnvironmentBinding>
    {
        #region Properties
        /// <summary>
        /// The name of the environment variable.
        /// </summary>
        [Description("The name of the environment variable.")]
        [XmlAttribute("name")]
        [Localizable(false)]
        public string Name { get; set; }

        /// <summary>
        /// A static value to set the variable to.
        /// </summary>
        /// <remarks>If this is set <see cref="Insert"/> must be <see langword="null"/>.</remarks>
        [Description("A static value to set the variable to. If this is set 'Insert' must be empty.")]
        [XmlAttribute("value"), CanBeNull]
        public string Value { get; set; }

        /// <summary>
        /// The relative path of the item within the implementation to insert into the variable's value. Use <code>.</code> to publish the root directory.
        /// </summary>
        /// <remarks>If this is set <see cref="Value"/> must be <see langword="null"/>.</remarks>
        [Description("The relative path of the item within the implementation to insert into the variable's value. Use \".\" to publish the root directory. If this is set 'Value' must be empty.")]
        [XmlAttribute("insert"), CanBeNull]
        public string Insert { get; set; }

        /// <summary>
        /// Controls how the <see cref="Insert"/> or <see cref="Value"/> is added to the variable.
        /// </summary>
        [Description("Controls how 'Insert' or 'Value' is added to the variable.")]
        [XmlAttribute("mode"), DefaultValue(typeof(EnvironmentMode), "Prepend")]
        public EnvironmentMode Mode { get; set; }

        /// <summary>
        /// Overrides the default separator character (":" on POSIX and ";" on Windows).
        /// </summary>
        [Description("Overrides the default separator character (\":\" on POSIX and \";\" on Windows).")]
        [XmlAttribute("separator"), DefaultValue("")]
        public string Separator { get; set; }

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
        /// Returns the binding in the form "Name = Value (Mode, Default)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return (string.IsNullOrEmpty(Insert))
                ? string.Format("{0} = {1} ({2})", Name, Value, Mode)
                : string.Format("{0} = Impl+{1} ({2})", Name, Insert, Mode);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="EnvironmentBinding"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="EnvironmentBinding"/>.</returns>
        public override Binding Clone()
        {
            return new EnvironmentBinding {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Name = Name, Value = Value, Insert = Insert, Mode = Mode, Separator = Separator, Default = Default};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(EnvironmentBinding other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Name == Name && other.Value == Value && other.Insert == Insert && other.Mode == Mode && other.Separator == Separator && other.Default == Default;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is EnvironmentBinding && Equals((EnvironmentBinding)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Name ?? "").GetHashCode();
                result = (result * 397) ^ (Value ?? "").GetHashCode();
                result = (result * 397) ^ (Insert ?? "").GetHashCode();
                result = (result * 397) ^ Mode.GetHashCode();
                result = (result * 397) ^ (Separator ?? "").GetHashCode();
                result = (result * 397) ^ (Default ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
