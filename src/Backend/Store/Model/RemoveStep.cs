/*
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

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Removes or moves a file or directory. It is an error if the path is outside the implementation.
    /// </summary>
    [Description("Removes or moves a file or directory. It is an error if the path is outside the implementation.")]
    [Serializable]
    [XmlRoot("remove", Namespace = Feed.XmlNamespace), XmlType("remove", Namespace = Feed.XmlNamespace)]
    public sealed class RemoveStep : FeedElement, IRecipeStep, IEquatable<RemoveStep>, ICloneable
    {
        #region Properties
        /// <summary>
        /// The file or directory to be removed relative to the implementation root as a Unix-style path.
        /// </summary>
        [Description("The file or directory to be removed relative to the implementation root as a Unix-style path.")]
        [XmlAttribute("path"), DefaultValue("")]
        public string Path { get; set; }
        #endregion

        //--------------------//

        #region Normalize
        /// <inheritdoc/>
        public void Normalize(FeedUri feedUri = null)
        {}
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the remove step in the form "Path". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Path;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="RemoveStep"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="RemoveStep"/>.</returns>
        public IRecipeStep CloneRecipeStep()
        {
            return new RemoveStep {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Path = Path};
        }

        object ICloneable.Clone()
        {
            return CloneRecipeStep();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(RemoveStep other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Path == Path;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RemoveStep && Equals((RemoveStep)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Path ?? "").GetHashCode();
            }
        }
        #endregion
    }
}
