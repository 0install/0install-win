/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Globalization;
using System.Xml.Serialization;
using NanoByte.Common;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// All attributes of a group are inherited by any child <see cref="Group"/>s and <see cref="Implementation"/>s as defaults, but can be overridden there.
    /// All <see cref="Dependency"/>s and <see cref="Binding"/>s are inherited (sub-groups may add more <see cref="Dependency"/>s and <see cref="Binding"/>s to the list, but cannot remove any).
    /// </summary>
    /// <seealso cref="Feed.Elements"/>
    [Description("All attributes of a group are inherited by any child Groups and Implementations as defaults, but can be overridden there.\r\nAll Dependencies and Bindings are inherited (sub-groups may add more Dependencies and Bindings to the list, but cannot remove any).")]
    [Serializable, XmlRoot("group", Namespace = Feed.XmlNamespace), XmlType("group", Namespace = Feed.XmlNamespace)]
    public sealed class Group : Element, IElementContainer, IEquatable<Group>
    {
        private readonly List<Element> _elements = new List<Element>();

        /// <summary>
        /// A list of <see cref="Group"/>s and <see cref="Implementation"/>s contained within this group.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Implementation)), XmlElement(typeof(PackageImplementation)), XmlElement(typeof(Group))]
        public List<Element> Elements { get { return _elements; } }

        #region Normalize
        /// <summary>
        /// Flattens the <see cref="Group"/> inheritance structure and sets missing default values in <see cref="Implementation"/>s.
        /// </summary>
        /// <param name="feedUri">The feed the data was originally loaded from.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing. Do not call it if you plan on serializing the feed again since it may loose some of its structure.</remarks>
        public override void Normalize(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            // Apply if-0install-version filter
            Elements.RemoveAll(FilterMismatch);

            var collapsedElements = new List<Element>();

            foreach (var element in Elements)
            {
                element.InheritFrom(this);

                // Flatten structure in sub-groups, set missing default values in implementations
                element.Normalize(feedUri);

                var group = element as Group;
                if (group != null)
                {
                    // Move implementations out of sub-groups
                    collapsedElements.AddRange(group.Elements);
                }
                else collapsedElements.Add(element);
            }
            Elements.Clear();
            Elements.AddRange(collapsedElements);
        }
        #endregion

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
        /// Returns the group in the form "Comma-seperated list of set values". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            var parts = new List<string>();
            if (Architecture != default(Architecture)) parts.Add(Architecture.ToString());
            if (Version != null) parts.Add(Version.ToString());
            if (Released != default(DateTime)) parts.Add(Released.ToString("d", CultureInfo.InvariantCulture));
            if (ReleasedVerbatim != null) parts.Add(ReleasedVerbatim);
            if (Stability != default(Stability)) parts.Add(Stability.ToString());
            if (!string.IsNullOrEmpty(License)) parts.Add(License);
            if (!string.IsNullOrEmpty(Main)) parts.Add(Main);
            return StringUtils.Join(", ", parts);
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
