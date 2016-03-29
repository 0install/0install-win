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
using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Renames or moves a file or directory. It is an error if the source or destination are outside the implementation.
    /// </summary>
    [Description("Renames or moves a file or directory. It is an error if the source or destination are outside the implementation.")]
    [Serializable, XmlRoot("rename", Namespace = Feed.XmlNamespace), XmlType("rename", Namespace = Feed.XmlNamespace)]
    public sealed class RenameStep : FeedElement, IRecipeStep, IEquatable<RenameStep>, ICloneable
    {
        /// <summary>
        /// The source file or directory relative to the implementation root as a Unix-style path.
        /// </summary>
        [Description("The source file or directory relative to the implementation root as a Unix-style path.")]
        [XmlAttribute("source"), DefaultValue(""), CanBeNull]
        public string Source { get; set; }

        /// <summary>
        /// The destination file or directory relative to the implementation root as a Unix-style path.
        /// </summary>
        [Description("The destination file or directory relative to the implementation root as a Unix-style path.")]
        [XmlAttribute("dest"), DefaultValue(""), CanBeNull]
        public string Destination { get; set; }

        #region Normalize
        /// <inheritdoc/>
        public void Normalize(FeedUri feedUri)
        {}
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the rename step in the form "Source => Destination". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} => {1}", Source, Destination);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="RenameStep"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="RenameStep"/>.</returns>
        public IRecipeStep CloneRecipeStep()
        {
            return new RenameStep {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Source = Source, Destination = Destination};
        }

        object ICloneable.Clone()
        {
            return CloneRecipeStep();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(RenameStep other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Source == Source && other.Destination == Destination;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is RenameStep && Equals((RenameStep)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (Source != null) result = (result * 397) ^ Source.GetHashCode();
                if (Destination != null) result = (result * 397) ^ Destination.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
