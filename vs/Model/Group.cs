using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// All attributes of the group are inherited by any child groups and <see cref="Implementation"/>s as defaults, but can be overridden there.
    /// All <see cref="Dependency"/>s and <see cref="Binding"/>s are inherited (sub-groups may add more <see cref="Dependency"/>s and <see cref="Binding"/>s to the list, but cannot remove anything). 
    /// </summary>
    public sealed class Group : ImplementationBase, IGroupContainer
    {
        #region Properties
        // ToDo: Prevent double entries
        private readonly Collection<Group> _groups = new Collection<Group>();
        /// <summary>
        /// A list of sub-groups.
        /// </summary>
        [Category("Implementation"), Description("A list of sub-groups.")]
        [XmlElement("group")]
        public Collection<Group> Groups { get { return _groups; } }

        #region Implementations
        private readonly Collection<Implementation> _implementation = new Collection<Implementation>();
        /// <summary>
        /// A list of <see cref="Implementation"/>s contained within this group.
        /// </summary>
        [Category("Implementation"), Description("A list of implementations contained within this group.")]
        [XmlElement("implementation")]
        public Collection<Implementation> Implementations { get { return _implementation; } }

        private readonly Collection<PackageImplementation> _packageImplementation = new Collection<PackageImplementation>();
        /// <summary>
        /// A list of distribution-provided <see cref="PackageImplementation"/>s contained within this group.
        /// </summary>
        [Category("Implementation"), Description("A list of distribution-provided package implementations contained within this group.")]
        [XmlElement("package-implementation")]
        public Collection<PackageImplementation> PackageImplementations { get { return _packageImplementation; } }
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
