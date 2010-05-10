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
    /// A reference to an <see cref="Interface"/> that is required by an <see cref="Implementation"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public sealed class Dependency : IBindingContainer, ICloneable
    {
        #region Properties
        /// <summary>
        /// The URI used to locate the <see cref="Interface"/>.
        /// </summary>
        [Description("The URI used to locate the interface.")]
        [XmlIgnore]
        public Uri Uri { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false)]
        public String UriString
        {
            get { return (Uri == null ? null : Uri.ToString()); }
            set { Uri = (value == null ? null : new Uri(value)); }
        }

        /// <summary>
        /// This can be used to indicate that this dependency is only needed in some cases.
        /// </summary>
        [Description("This can be used to indicate that this dependency is only needed in some cases.")]
        [XmlAttribute("use")]
        public string Use { get; set; }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Constraint> _constraints = new C5.HashedArrayList<Constraint>();
        /// <summary>
        /// A list of version <see cref="Constraint"/>s that must be fulfilled.
        /// </summary>
        [Description("A list of version constraints that must be fulfilled.")]
        [XmlElement("version")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Constraint> Constraints { get { return _constraints; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<EnvironmentBinding> _environmentBindings = new C5.HashedArrayList<EnvironmentBinding>();
        /// <summary>
        /// A list of <see cref="EnvironmentBinding"/>s for <see cref="Implementation"/>s to locate this dependency.
        /// </summary>
        [Description("A list of bindings for environment implementations to locate this dependency.")]
        [XmlElement("environment")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<EnvironmentBinding> EnvironmentBindings { get { return _environmentBindings; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<OverlayBinding> _overlayBindings = new C5.HashedArrayList<OverlayBinding>();
        /// <summary>
        /// A list of <see cref="OverlayBinding"/>s for <see cref="Implementation"/>s to locate this dependency.
        /// </summary>
        [Description("A list of bindings for overlay implementations to locate this dependency.")]
        [XmlElement("overlay")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<OverlayBinding> OverlayBindings { get { return _overlayBindings; } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Dependency"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Dependency"/>.</returns>
        public Dependency CloneDependency()
        {
            var dependency = new Dependency {Uri = Uri, Use = Use};
            foreach (var binding in EnvironmentBindings) dependency.EnvironmentBindings.Add(binding.CloneBinding());
            foreach (var binding in OverlayBindings) dependency.OverlayBindings.Add(binding.CloneBinding());

            return dependency;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Dependency"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Dependency"/> casted to a generic <see cref="object"/>.</returns>
        public object Clone()
        {
            return CloneDependency();
        }
        #endregion

        // ToDo: Implement ToString and Equals
    }
}
