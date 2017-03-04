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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Retrieves an implementation by applying a list of <see cref="IRecipeStep"/>s, such as downloading and combining multiple archives.
    /// </summary>
    [Description("Retrieves an implementation by applying a list of recipe steps, such as downloading and combining multiple archives.")]
    [Serializable, XmlRoot("recipe", Namespace = Feed.XmlNamespace), XmlType("recipe", Namespace = Feed.XmlNamespace)]
    public sealed class Recipe : RetrievalMethod, IEquatable<Recipe>
    {
        /// <summary>
        /// An ordered list of <see cref="IRecipeStep"/>s to execute.
        /// </summary>
        [Description("An ordered list of archives to extract.")]
        [XmlIgnore, NotNull]
        public List<IRecipeStep> Steps { get; } = new List<IRecipeStep>();

        #region XML serialization
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Steps"/>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Used for XML serialization.")]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(typeof(Archive)), XmlElement(typeof(SingleFile)), XmlElement(typeof(RenameStep)), XmlElement(typeof(RemoveStep)), XmlElement(typeof(CopyFromStep))]
        public object[] StepsArray
        {
            get
            {
                // ReSharper disable CoVariantArrayConversion
                return Steps.ToArray();
                // ReSharper restore CoVariantArrayConversion
            }
            set
            {
                Steps.Clear();
                if (value != null && value.Length > 0) Steps.AddRange(value.OfType<IRecipeStep>());
            }
        }
        #endregion

        /// <summary>
        /// Indicates whether this recipe contains steps of unknown type and therefore can not be processed.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public bool ContainsUnknownSteps => (UnknownElements != null) && (UnknownElements.Length > 0);

        #region Normalize
        /// <summary>
        /// Call <see cref="RetrievalMethod.Normalize"/> on all contained <see cref="IRecipeStep"/>s.
        /// </summary>
        /// <param name="feedUri">The feed the data was originally loaded from.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing. Do not call it if you plan on serializing the feed again since it may loose some of its structure.</remarks>
        public override void Normalize(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException(nameof(feedUri));
            #endregion

            base.Normalize(feedUri);

            // Apply if-0install-version filter
            Steps.RemoveAll(FilterMismatch);

            foreach (var step in Steps)
                step.Normalize(feedUri);
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the recipe in the form "X steps". Not safe for parsing!
        /// </summary>
        public override string ToString() => $"{Steps.Count} steps";
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Recipe"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Recipe"/>.</returns>
        public override RetrievalMethod Clone()
        {
            var recipe = new Recipe {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements};
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
            return base.Equals(other) && Steps.SequencedEquals(other.Steps);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is Recipe && Equals((Recipe)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Steps.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
