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
    /// All attributes of the group are inherited by any child groups and <see cref="Implementation"/>s as defaults, but can be overridden there.
    /// All <see cref="Dependency"/>s and <see cref="Binding"/>s are inherited (sub-groups may add more <see cref="Dependency"/>s and <see cref="Binding"/>s to the list, but cannot remove anything). 
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be dispoed.")]
    public sealed class Group : ImplementationBase, IGroupContainer
    {
        #region Properties
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Group> _groups = new C5.HashedArrayList<Group>();
        /// <summary>
        /// A list of sub-groups.
        /// </summary>
        [Category("Implementation"), Description("A list of sub-groups.")]
        [XmlElement("group")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Group> Groups { get { return _groups; } }

        #region Implementations
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Implementation> _implementation = new C5.HashedArrayList<Implementation>();
        /// <summary>
        /// A list of <see cref="Implementation"/>s contained within this group.
        /// </summary>
        [Category("Implementation"), Description("A list of implementations contained within this group.")]
        [XmlElement("implementation")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Implementation> Implementations { get { return _implementation; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<PackageImplementation> _packageImplementation = new C5.HashedArrayList<PackageImplementation>();
        /// <summary>
        /// A list of distribution-provided <see cref="PackageImplementation"/>s contained within this group.
        /// </summary>
        [Category("Implementation"), Description("A list of distribution-provided package implementations contained within this group.")]
        [XmlElement("package-implementation")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<PackageImplementation> PackageImplementations { get { return _packageImplementation; } }
        #endregion

        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Flattens the <see cref="Group"/> inheritance structure and sets missing default values in <see cref="Implementation"/>s.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Interface"/> again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            // Apply attribute inheritance to implementations and set missing default values
            foreach (var implementation in Implementations)
            {
                implementation.InheritFrom(this);
                implementation.Simplify();
            }
            foreach (var implementation in PackageImplementations)
            {
                implementation.InheritFrom(this);
                implementation.Simplify();
            }

            foreach (var group in Groups)
            {
                // Apply attribute inheritance to sub-groups
                group.InheritFrom(this);

                // Flatten structure in sub-groups and set missing default values in contained implementations
                group.Simplify();

                // Move implementations out of groups
                foreach (var implementation in group.Implementations) Implementations.Add(implementation);
                group.Implementations.Clear();
                foreach (var implementation in group.PackageImplementations) PackageImplementations.Add(implementation);
                group.Implementations.Clear();
            }

            // All sub-groups are now empty and unnecessary
            Groups.Clear();
        }
        #endregion

        //--------------------//

        // ToDo: Implement ToString and Equals
    }
}
