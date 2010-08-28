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
    /// A reference to an <see cref="Feed"/> that is required by an <see cref="Implementation"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public sealed class Dependency : IBindingContainer, ICloneable
    {
        #region Properties
        /// <summary>
        /// The URI or local path used to identify the <see cref="Feed"/>.
        /// </summary>
        [Description("The URI or local path used to identify the interface.")]
        [XmlAttribute("interface")] 
        public string Interface { get; set; }

        /// <summary>
        /// This can be used to indicate that this dependency is only needed in some cases.
        /// </summary>
        [Description("This can be used to indicate that this dependency is only needed in some cases.")]
        [XmlAttribute("use")]
        public string Use { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Constraint> _constraints = new C5.ArrayList<Constraint>();
        /// <summary>
        /// A list of version <see cref="Constraint"/>s that must be fulfilled.
        /// </summary>
        [Description("A list of version constraints that must be fulfilled.")]
        [XmlElement("version")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<Constraint> Constraints { get { return _constraints; } }

        // Preserve order
        private readonly C5.ArrayList<Binding> _bindings = new C5.ArrayList<Binding>();
        /// <summary>
        /// A list of <see cref="Binding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Description("A list of bindings for implementations to locate dependencies.")]
        [XmlElement(Type = typeof(EnvironmentBinding), ElementName = "environment"),
        XmlElement(Type = typeof(OverlayBinding), ElementName = "overlay")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<Binding> Bindings { get { return _bindings; } }
        #endregion

        //--------------------//

        // ToDo: Implement ToString

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Dependency"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Dependency"/>.</returns>
        public Dependency CloneDependency()
        {
            var dependency = new Dependency {Interface = Interface, Use = Use};
            foreach (var binding in Bindings) dependency.Bindings.Add(binding.CloneBinding());
            foreach (var constraint in Constraints) dependency.Constraints.Add(constraint.CloneConstraint());

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

        #region Equality
        public bool Equals(Dependency other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (Interface != other.Interface) return false;
            if (Use != other.Use) return false;
            if (!Constraints.SequencedEquals(other.Constraints)) return false;
            if (!Bindings.SequencedEquals(other.Bindings)) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Dependency) && Equals((Dependency)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Interface ?? "").GetHashCode();
                result = (result * 397) ^ (Use ?? "").GetHashCode();
                result = (result * 397) ^ Constraints.GetSequencedHashCode();
                result = (result * 397) ^ Bindings.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
