/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// All attributes of the group are inherited by any child groups and <see cref="Implementation"/>s as defaults, but can be overridden there.
    /// All <see cref="Dependency"/>s and <see cref="Binding"/>s are inherited (sub-groups may add more <see cref="Dependency"/>s and <see cref="Binding"/>s to the list, but cannot remove anything).
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("group", Namespace = Feed.XmlNamespace)]
    public sealed class Group : Element, IElementContainer, IEquatable<Group>
    {
        #region Properties
        // Preserve order
        private readonly C5.ArrayList<Element> _elements = new C5.ArrayList<Element>();

        /// <summary>
        /// A list of <see cref="Group"/>s and <see cref="Implementation"/>s contained within this group.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Implementation)), XmlElement(typeof(PackageImplementation)), XmlElement(typeof(Group))]
        public C5.ArrayList<Element> Elements { get { return _elements; } }
        #endregion

        //--------------------//

        #region Normalize
        /// <summary>
        /// Flattens the <see cref="Group"/> inheritance structure and sets missing default values in <see cref="Implementation"/>s.
        /// </summary>
        /// <param name="feedID">The feed the data was originally loaded from.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing.
        /// It should not be called if you plan on serializing the feed again since it will may loose some of its structure.</remarks>
        public override void Normalize(string feedID)
        {
            var collapsedElements = new List<Element>();

            foreach (var element in Elements)
            {
                element.InheritFrom(this);

                // Flatten structure in sub-groups, set missing default values in implementations
                element.Normalize(feedID);

                var group = element as Group;
                if (group != null)
                {
                    // Move implementations out of sub-groups
                    collapsedElements.AddRange(group.Elements);
                }
                else collapsedElements.Add(element);
            }

            // Replace original elements list with the collapsed version
            Elements.Clear();
            Elements.AddAll(collapsedElements);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Group"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Group"/>.</returns>
        public Group CloneGroup()
        {
            var group = new Group();
            CloneFromTo(this, group);
            foreach (var element in Elements)
                group.Elements.Add(element.Clone());

            return group;
        }

        /// <inheritdoc/>
        public override Element Clone()
        {
            return CloneGroup();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the group in the form "Group: Comma-seperated list of set values". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = "Group: ";
            if (Architecture != default(Architecture)) result += Architecture + ", ";
            if (Version != null) result += Version + ", ";
            if (Released != default(DateTime)) result += Released.ToShortDateString() + ", ";
            if (Stability != default(Stability)) result += Stability + ", ";
            if (!string.IsNullOrEmpty(License)) result += License +", ";
            if (Main != null) result += Main + ", ";

            // Remove last two characters
            return result.Substring(0, result.Length - 2);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Group other)
        {
            if (other == null) return false;
            return base.Equals(other) && Elements.SequencedEquals(other.Elements);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Group && Equals((Group)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Elements.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
