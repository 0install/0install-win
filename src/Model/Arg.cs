﻿/*
 * Copyright 2010-2014 Bastian Eicher
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
    /// <summary>
    /// A single command-line arguments to be passed to an executable.
    /// </summary>
    [Description("A single command-line arguments to be passed to an executable.")]
    [Serializable]
    [XmlRoot("arg", Namespace = Feed.XmlNamespace), XmlType("arg", Namespace = Feed.XmlNamespace)]
    public class Arg : ArgBase, IEquatable<Arg>
    {
        #region Properties
        /// <summary>
        /// A single command-line arguments to be passed to an executable.
        /// Will be automatically escaped to allow proper concatenation of multiple arguments containing spaces.
        /// </summary>
        [Description("A single command-line arguments to be passed to an executable.\nWill be automatically escaped to allow proper concatenation of multiple arguments containing spaces.")]
        [XmlText]
        public string Value { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Convenience cast for turning strings into plain <see cref="Arg"/>s.
        /// </summary>
        public static implicit operator Arg(string value)
        {
            return new Arg {Value = value};
        }

        /// <summary>
        /// Returns <see cref="Value"/> directly. Safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Value;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Arg other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Value == Value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Arg && Equals((Arg)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (Value != null) result = (result * 397) ^ Value.GetHashCode();
                return result;
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Arg"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Arg"/>.</returns>
        public Arg CloneArg()
        {
            return new Arg {Value = Value};
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Arg"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Arg"/>.</returns>
        public override ArgBase Clone()
        {
            return CloneArg();
        }
        #endregion
    }
}
