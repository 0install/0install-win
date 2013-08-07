﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Expands an environment variable to multiple arguments.
    /// The variable specified in <see cref="ItemFrom"/> is split using <see cref="Separator"/> and the <see cref="Arguments"/> are added once for each item.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [Description("Expands an environment variable to multiple arguments.\nThe variable specified in ItemFrom is split using Separator and the arguments are added once for each item.")]
    [Serializable]
    [XmlRoot("for-each", Namespace = Feed.XmlNamespace), XmlType("for-each", Namespace = Feed.XmlNamespace)]
    public class ForEachArgs : ArgBase, IEquatable<ForEachArgs>
    {
        #region Properties
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
        [XmlAttribute("separator"), DefaultValue("")]
        public string Separator { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Arg> _arguments = new C5.ArrayList<Arg>();

        /// <summary>
        /// A list of command-line arguments to be passed to an executable. "${item}" will be substituted with each for-each value.
        /// </summary>
        [Browsable(false)]
        [XmlElement("arg")]
        public C5.ArrayList<Arg> Arguments { get { return _arguments; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the for-each instruction in the form "ItemFrom". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return ItemFrom;
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
            forEachArgs.Arguments.AddAll(Arguments.CloneElements());
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
                if (ItemFrom != null) result = (result * 397) ^ ItemFrom.GetHashCode();
                if (Separator != null) result = (result * 397) ^ Separator.GetHashCode();
                result = (result * 397) ^ Arguments.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
