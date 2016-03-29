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
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// An application category (e.g. Game or Office). Used for organizing application menus.
    /// </summary>
    [Description("An application category (e.g. Game or Office). Used for organizing application menus.")]
    [Serializable, XmlRoot("category", Namespace = Feed.XmlNamespace), XmlType("category", Namespace = Feed.XmlNamespace)]
    public sealed class Category : FeedElement, IEquatable<Category>, ICloneable
    {
        #region Constants
        /// <summary>
        /// Well-known values for <see cref="Name"/> if <see cref="TypeNamespace"/> is empty.
        /// </summary>
        public static readonly string[] WellKnownNames = {"AudioVideo", "Audio", "Video", "Development", "Education", "Game", "Graphics", "Network", "Office", "Science", "Settings", "System", "Utility"};
        #endregion

        /// <summary>
        /// The category name as specified by the <see cref="TypeNamespace"/>.
        /// </summary>
        [Description("The category name as specified by the TypeNamespace.")]
        [TypeConverter(typeof(CategoryNameConverter))]
        [XmlText]
        public string Name { get; set; }

        /// <summary>
        /// If no type is given, then the category is one of the 'Main' categories defined by the freedesktop.org menu specification (http://standards.freedesktop.org/menu-spec/latest/apa.html). Otherwise, it is a URI giving the namespace for the category.
        /// </summary>
        [Description("If no type is given, then the category is one of the 'Main' categories defined by the freedesktop.org menu specification. Otherwise, it is a URI giving the namespace for the category.")]
        [XmlAttribute("type"), DefaultValue(""), CanBeNull]
        public string TypeNamespace { get; set; }

        #region Conversion
        /// <summary>
        /// Convenience cast for turning strings into <see cref="Category"/>s.
        /// </summary>
        public static implicit operator Category(string value)
        {
            return new Category {Name = value};
        }

        /// <summary>
        /// Returns <see cref="Name"/> directly. Safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Name ?? "unset";
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Category other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Name == Name && other.TypeNamespace == TypeNamespace;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is Category && Equals((Category)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (Name != null) result = (result * 397) ^ Name.GetHashCode();
                if (TypeNamespace != null) result = (result * 397) ^ TypeNamespace.GetHashCode();
                return result;
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a plain copy of this category.
        /// </summary>
        /// <returns>The cloned category.</returns>
        public Category Clone()
        {
            return new Category {Name = Name, TypeNamespace = TypeNamespace};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
