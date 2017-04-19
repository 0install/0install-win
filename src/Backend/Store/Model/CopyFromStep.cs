/*
 * Copyright 2010-2017 Bastian Eicher
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
    /// Copies files or directories from another implementation specified elsewhere in the same feed.
    /// </summary>
    [Description("Copies files or directories from another implementation specified elsewhere in the same feed.")]
    [Serializable, XmlRoot("copy-from", Namespace = Feed.XmlNamespace), XmlType("copy-from", Namespace = Feed.XmlNamespace)]
    public sealed class CopyFromStep : FeedElement, IRecipeStep, IEquatable<CopyFromStep>, ICloneable
    {
        /// <summary>
        /// The <see cref="ImplementationBase.ID"/> of the <see cref="Implementation"/> to copy from.
        /// </summary>
        [Description("The ID of the implementation to copy from.")]
        [XmlAttribute("id"), DefaultValue(""), CanBeNull]
        public string ID { get; set; }

        /// <summary>
        /// The source file or directory relative to the source implementation root as a Unix-style path. Leave <c>null</c> to copy the entire implementation.
        /// </summary>
        [Description("The source file or directory relative to the source implementation root as a Unix-style path. Leave unset to copy the entire implementation.")]
        [XmlAttribute("source"), DefaultValue(""), CanBeNull]
        public string Source { get; set; }

        /// <summary>
        /// The destination file or directory relative to the destination implementation root as a Unix-style path. Leave <c>null</c> to copy to the root of the new implementation. Must be set if <see cref="Source"/> points to a file rather than a directory.
        /// </summary>
        [Description("The destination file or directory relative to the destination implementation root as a Unix-style path. Leave unset to copy to the root of the new implementation. Must be set if Source points to a file rather than a directory.")]
        [XmlAttribute("dest"), DefaultValue(""), CanBeNull]
        public string Destination { get; set; }

        /// <summary>
        /// Used to hold the <see cref="Implementation"/> the <see cref="ID"/> references after <see cref="Feed.Normalize"/> has been executed.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public Implementation Implementation { get; set; }

        #region Normalize
        /// <inheritdoc/>
        public void Normalize(FeedUri feedUri)
        {}
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the copy-from step in the form "Copy from ID (Source => Destination)". Not safe for parsing!
        /// </summary>
        public override string ToString() => $"Copy from {ID} ({Source} => {Destination})";
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="CopyFromStep"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="CopyFromStep"/>.</returns>
        public IRecipeStep CloneRecipeStep() => new CopyFromStep {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, ID = ID, Source = Source, Destination = Destination};

        object ICloneable.Clone() => CloneRecipeStep();
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(CopyFromStep other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.ID == ID && other.Source == Source && other.Destination == Destination;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            var step = obj as CopyFromStep;
            return step != null && Equals(step);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ ID?.GetHashCode() ?? 0;
                result = (result * 397) ^ Source?.GetHashCode() ?? 0;
                result = (result * 397) ^ Destination?.GetHashCode() ?? 0;
                return result;
            }
        }
        #endregion
    }
}
