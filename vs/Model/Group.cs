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
    /// All attributes of the group are inherited by any child groups and <see cref="Implementation"/>s as defaults, but can be overridden there.
    /// All <see cref="Dependency"/>s and <see cref="Binding"/>s are inherited (sub-groups may add more <see cref="Dependency"/>s and <see cref="Binding"/>s to the list, but cannot remove anything). 
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("group", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    public sealed class Group : Element, IElementContainer, IEquatable<Group>
    {
        #region Properties
        // Preserve order
        private readonly C5.ArrayList<Element> _elements = new C5.ArrayList<Element>();
        /// <summary>
        /// A list of <see cref="Group"/>s and <see cref="Implementation"/>s contained within this group.
        /// </summary>
        [Category("Implementation"), Description("A list of groups and implementations contained within this group.")]
        [XmlElement(typeof(Implementation)), XmlElement(typeof(PackageImplementation)), XmlElement(typeof(Group))]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<Element> Elements { get { return _elements; } }
        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Flattens the <see cref="Group"/> inheritance structure and sets missing default values in <see cref="Implementation"/>s.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Feed"/> again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            var collapsedElements = new C5.LinkedList<Element>();

            foreach (var element in Elements)
            {
                element.InheritFrom(this);

                // Flatten structure in sub-groups, set missing default values in implementations
                element.Simplify();

                var group = element as Group;
                if (group != null)
                {
                    // Move implementations out of sub-groups
                    foreach (var groupElement in group.Elements)
                        collapsedElements.Add(groupElement);
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
                group.Elements.Add(element.CloneElement());

            return group;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Group"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Group"/>.</returns>
        public override Element CloneElement()
        {
            return CloneGroup();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the group in the form "Group: Comma-seperated names of the set values". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = "Group: ";
            if (!Languages.IsEmpty) result += "Languages, ";
            if (Architecture != default(Architecture)) result += "Architecture, ";
            if (Version != null) result += "Version, ";
            if (Released != default(DateTime)) result += "Released, ";
            if (Stability != default(Stability)) result += "Stability, ";
            if (!string.IsNullOrEmpty(License)) result += "License, ";
            if (!string.IsNullOrEmpty(Main)) result += "Main, ";
            if (!string.IsNullOrEmpty(SelfTest)) result += "SelfTest, ";
            if (!string.IsNullOrEmpty(DocDir)) result += "DocDir, ";

            // Remove last two characters
            return result.Substring(0, result.Length - 2);
        }
        #endregion

        #region Equality
        public bool Equals(Group other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (!base.Equals(other)) return false;
            if (!Elements.SequencedEquals(other.Elements)) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Group) && Equals((Group)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Elements.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
