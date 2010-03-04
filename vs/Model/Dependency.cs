using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A reference to an <see cref="Interface"/> that is required by an <see cref="Implementation"/>.
    /// </summary>
    public sealed class Dependency : IBindingContainer
    {
        #region Properties
        /// <summary>
        /// The URI used to locate the <see cref="Interface"/>.
        /// </summary>
        [Description("The URI used to locate the interfcae.")]
        [XmlIgnore]
        public Uri Uri { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false)]
        public String UriString
        {
            get { return (Uri == null ? null : Uri.ToString()); }
            set { Uri = new Uri(value); }
        }

        /// <summary>
        /// This can be used to indicate that this dependency is only needed in some cases.
        /// </summary>
        [Description("This can be used to indicate that this dependency is only needed in some cases.")]
        [XmlAttribute("use")]
        public string Use { get; set; }

        private readonly Collection<Constraint> _constraints = new Collection<Constraint>();
        /// <summary>
        /// A list of version <see cref="Constraint"/>s that must be fullfilled.
        /// </summary>
        [Description("A list of version constraints that must be fullfilled.")]
        [XmlElement("version")]
        public Collection<Constraint> Constraints { get { return _constraints; } }

        private readonly Collection<EnvironmentBinding> _environmentBindings = new Collection<EnvironmentBinding>();
        /// <summary>
        /// A list of <see cref="EnvironmentBinding"/>s for <see cref="Implementation"/>s to locate this dependency.
        /// </summary>
        [Description("A list of bindings for environment implementatiosn to locate this dependency.")]
        [XmlElement("environment")]
        public Collection<EnvironmentBinding> EnvironmentBindings { get { return _environmentBindings; } }

        private readonly Collection<OverlayBinding> _overlayBindings = new Collection<OverlayBinding>();
        /// <summary>
        /// A list of <see cref="OverlayBinding"/>s for <see cref="Implementation"/>s to locate this dependency.
        /// </summary>
        [Description("A list of bindings for overlay implementatiosn to locate this dependency.")]
        [XmlElement("overlay")]
        public Collection<OverlayBinding> OverlayBindings { get { return _overlayBindings; } }
        #endregion
    }
}
