using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Backend.Model
{
    /// <summary>
    /// An object that contains <see cref="Group"/>s and <see cref="Feed"/>s. Supports a composite pattern.
    /// </summary>
    interface IGroupContainer
    {
        /// <summary>
        /// A list of <see cref="Group"/>s contained within this element.
        /// </summary>
        [Category("Implementation"), Description("A list of groups contained within this element.")]
        [XmlElement("group")]
        Collection<Group> Groups { get; }

        #region Implementations
        /// <summary>
        /// A list of <see cref="Implementation"/>s contained within this element.
        /// </summary>
        [Category("Implementation"), Description("A list of implementations contained within this element.")]
        [XmlElement("implementation")]
        Collection<Implementation> Implementations { get; }

        /// <summary>
        /// A list of distribution-provided <see cref="PackageImplementation"/>s contained within this element.
        /// </summary>
        [Category("Implementation"), Description("A list of distribution-provided package implementations contained within this element.")]
        [XmlElement("implementation")]
        Collection<PackageImplementation> PackageImplementations { get; }
        #endregion

        /// <summary>
        /// Calls <see cref="ImplementationBase.InheritFrom"/> for all <see cref="Groups"/>, <see cref="Implementations"/> and <see cref="PackageImplementations"/>
        /// and moves the entries of <see cref="Groups"/> upwards.
        /// </summary>
        void FlattenInheritance();
    }
}
