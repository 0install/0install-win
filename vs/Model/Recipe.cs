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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A recipe is a list of <see cref="RetrievalStep"/>s used to create an <see cref="Implementation"/> directory.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public sealed class Recipe : RetrievalMethod
    {
        #region Properties

        #region Steps
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Archive> _archives = new C5.HashedArrayList<Archive>();
        /// <summary>
        /// An ordered list of archives to extract.
        /// </summary>
        [Description("An ordered list of archives to extract.")]
        [XmlElement("archive")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Archive> Archives { get { return _archives; } }
        #endregion

        #endregion
        
        //--------------------//

        #region Simplify
        /// <summary>
        /// Call <see cref="ISimplifyable.Simplify"/> on all contained <see cref="RetrievalStep"/>s.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Interface"/> again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            foreach (var archive in Archives) archive.Simplify();
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Recipe"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Recipe"/>.</returns>
        public Recipe CloneRecipe()
        {
            var recipe = new Recipe();
            foreach (Archive archive in Archives)
                recipe.Archives.Add(archive.CloneArchive());

            return recipe;
        }

        public object Clone()
        {
            return CloneRecipe();
        }
        #endregion

        // ToDo: Implement ToString and Equals
    }
}
