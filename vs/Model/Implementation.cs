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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A downloadable implementation of an <see cref="Interface"/>.
    /// </summary>
    /// <remarks>An implementation is a specific version of an application, e.g. Fire fox 3.6 for Windows.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class Implementation : IDImplementation
    {
        #region Properties
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Archive> _archives = new C5.HashedArrayList<Archive>();
        /// <summary>
        /// A list of <see cref="Archive"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of archives as retrieval methods.")]
        [XmlElement("archive")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Archive> Archives { get { return _archives; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Recipe> _recipes = new C5.HashedArrayList<Recipe>();
        /// <summary>
        /// A list of <see cref="Recipe"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of recipes as retrieval methods.")]
        [XmlElement("recipe")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Recipe> Recipes { get { return _recipes; } }
        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Sets missing default values.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the interface again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            base.Simplify();

            // Simplify retrieval methods
            foreach (var archive in Archives) archive.Simplify();
            foreach (var recipe in Recipes) recipe.Simplify();
        }
        #endregion

        //--------------------//

        // ToDo: Implement ToString, Equals and Clone
    }
}
