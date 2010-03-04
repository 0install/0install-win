using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An object that contains <see cref="Binding"/>s.
    /// </summary>
    interface IBindingContainer
    {
        /// <summary>
        /// A list of <see cref="EnvironmentBinding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Description("A list of bindings for implementatiosn to locate dependencies.")]
        [XmlElement("environment")]
        Collection<EnvironmentBinding> EnvironmentBindings { get; }

        /// <summary>
        /// A list of <see cref="OverlayBinding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Description("A list of bindings for implementatiosn to locate dependencies.")]
        [XmlElement("overlay")]
        Collection<OverlayBinding> OverlayBindings { get; }
    }
}
