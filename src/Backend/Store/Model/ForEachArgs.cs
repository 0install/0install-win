/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Expands an environment variable to multiple arguments.
    /// The variable specified in <see cref="ItemFrom"/> is split using <see cref="Separator"/> and the <see cref="Arguments"/> are added once for each item.
    /// </summary>
    [Description("Expands an environment variable to multiple arguments.\r\nThe variable specified in ItemFrom is split using Separator and the arguments are added once for each item.")]
    [Serializable, XmlRoot("for-each", Namespace = Feed.XmlNamespace), XmlType("for-each", Namespace = Feed.XmlNamespace)]
    public class ForEachArgs : ArgBase, IEquatable<ForEachArgs>
    {
        /// <summary>
        /// The name of the environment variable to be expanded.
        /// </summary>
        [Description("The name of the environment variable to be expanded.")]
        [XmlAttribute("item-from")]
        public string ItemFrom { get; set; }

        /// <summary>
        /// Overrides the default separator character (":" on POSIX and ";" on Windows).
        /// </summary>
        [Description("Overrides the default separator character (\":\" on POSIX and \";\" on Windows).")]
        [XmlAttribute("separator"), DefaultValue(""), CanBeNull]
        public string Separator { get; set; }

        private readonly List<Arg> _arguments = new List<Arg>();

        /// <summary>
        /// A list of command-line arguments to be passed to an executable. "${item}" will be substituted with each for-each value.
        /// </summary>
        [Browsable(false)]
        [XmlElement("arg"), NotNull]
        public List<Arg> Arguments => _arguments;

        #region Normalize
        /// <inheritdoc/>
        public override void Normalize()
        {
            EnsureNotNull(ItemFrom, xmlAttribute: "item-from", xmlTag: "for-each");
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the for-each instruction in the form "ItemFrom". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return ItemFrom ?? "(empty)";
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ForEachArgs"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ForEachArgs"/>.</returns>
        private ForEachArgs CloneForEachArgs()
        {
            var forEachArgs = new ForEachArgs {ItemFrom = ItemFrom, Separator = Separator};
            forEachArgs.Arguments.AddRange(Arguments.CloneElements());
            return forEachArgs;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="ForEachArgs"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ForEachArgs"/>.</returns>
        public override ArgBase Clone()
        {
            return CloneForEachArgs();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ForEachArgs other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.ItemFrom == ItemFrom && other.Separator == Separator &&
                   Arguments.SequencedEquals(other.Arguments);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is Arg && Equals((Arg)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ ItemFrom?.GetHashCode() ?? 0;
                result = (result * 397) ^ Separator?.GetHashCode() ?? 0;
                result = (result * 397) ^ Arguments.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
