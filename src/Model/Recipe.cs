/*
 * Copyright 2010-2011 Bastian Eicher
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
    /// A recipe is a list of <see cref="RecipeStep"/>s used to create an <see cref="Model.Implementation"/> directory.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("recipe", Namespace = Feed.XmlNamespace)]
    public sealed class Recipe : RetrievalMethod, IEquatable<Recipe>
    {
        #region Properties
        // Preserve order
        private readonly C5.ArrayList<RecipeStep> _steps = new C5.ArrayList<RecipeStep>();
        /// <summary>
        /// An ordered list of <see cref="RecipeStep"/>s to execute.
        /// </summary>
        [Description("An ordered list of archives to extract.")]
        [XmlElement("archive", typeof(Archive))] // Note: explicit naming of XML tag can be removed once other RecipeStep types have been added
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<RecipeStep> Steps { get { return _steps; } }

        /// <summary>
        /// Indicates whether this recipe contains steps of unknown type and therefore can not be processed.
        /// </summary>
        [Description("Indicates whether this recipe contains steps of unknown type and therefore can not be processed.")]
        [XmlIgnore]
        public bool ContainsUnknownSteps { get { return UnknownElements != null && UnknownElements.Length > 0; } }
        #endregion
        
        //--------------------//

        #region Simplify
        /// <summary>
        /// Call <see cref="ISimplifyable.Simplify"/> on all contained <see cref="RecipeStep"/>s.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Feed"/> again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            foreach (var step in Steps) step.Simplify();
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "Archive: Location (MimeType, Size + StartOffset) => Extract". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("Recipe: {0} Archives", Steps.Count);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Recipe"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Recipe"/>.</returns>
        public override RetrievalMethod CloneRetrievalMethod()
        {
            var recipe = new Recipe();
            foreach (var step in Steps)
                recipe.Steps.Add(step.CloneRecipeStep());

            return recipe;
        }
        #endregion
        
        #region Equality
        /// <inheritdoc/>
        public bool Equals(Recipe other)
        {
            if (other == null) return false;

            return Steps.SequencedEquals(other.Steps);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Recipe) && Equals((Recipe)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return Steps.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
