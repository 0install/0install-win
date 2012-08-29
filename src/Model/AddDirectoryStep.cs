/*
 * Copyright 2010-2012 Bastian Eicher
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
    /// Adds a directory. It is an error if the path is outside the implementation.
    /// </summary>
    [Serializable]
    [XmlType("add-directory", Namespace = Feed.XmlNamespace)]
    public sealed class AddDirectoryStep : XmlUnknown, IRecipeStep, IEquatable<AddDirectoryStep>
    {
        #region Properties
        /// <summary>
        /// The directory to be added relative to the implementation root as a Unix-style path.
        /// </summary>
        [Description("The directory to be added relative to the implementation root as a Unix-style path.")]
        [XmlAttribute("path"), DefaultValue("")]
        public string Path { get; set; }
        #endregion

        //--------------------//

        #region Normalize
        /// <inheritdoc/>
        public void Normalize()
        {}
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the add-directory step in the form "AddDirectory: Path". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AddDirectory: {0}", Path);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AddDirectoryStep"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AddDirectoryStep"/>.</returns>
        public IRecipeStep CloneRecipeStep()
        {
            return new AddDirectoryStep {Path = Path};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AddDirectoryStep other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Path == Path;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is AddDirectoryStep && Equals((AddDirectoryStep)obj);
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
