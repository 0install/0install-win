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
    /// Moves everything into a new top-level directory. It is an error if the path is outside the implementation.
    /// </summary>
    [Serializable]
    [XmlType("add-toplevel", Namespace = Feed.XmlNamespace)]
    public sealed class AddToplevelStep : XmlUnknown, IRecipeStep, IEquatable<AddToplevelStep>
    {
        #region Properties
        /// <summary>
        /// The name of the directory to be added.
        /// </summary>
        [Description("The name of the directory to be added.")]
        [XmlAttribute("dir"), DefaultValue("")]
        public string Directory { get; set; }
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
        /// Returns the add-toplevel step in the form "AddToplevel: Directory". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AddToplevel: {0}", Directory);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AddToplevelStep"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AddToplevelStep"/>.</returns>
        public IRecipeStep CloneRecipeStep()
        {
            return new AddToplevelStep {Directory = Directory};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AddToplevelStep other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Directory == Directory;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is AddToplevelStep && Equals((AddToplevelStep)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Directory ?? "").GetHashCode();
            }
        }
        #endregion
    }
}
