/*
 * Copyright 2010-2011 Bastian Eicher
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
    #region Enumerations
    /// <seealso cref="Dependency.Importance"/>
    public enum Importance
    {
        /// <summary>A version of the <see cref="Dependency"/> must be selected.</summary>
        [XmlEnum("essential")]
        Essential,

        /// <summary>No version of the <see cref="Dependency"/> is also an option, although selecting a version is preferable to not selecting one.</summary>
        [XmlEnum("recommended")]
        Recommended,
    }
    #endregion

    /// <summary>
    /// A reference to an interface that is required as dependency.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("depedency", Namespace = Feed.XmlNamespace)]
    public class Dependency : XmlUnknown, IBindingContainer, ICloneable
    {
        #region Properties
        /// <summary>
        /// The URI or local path used to identify the interface.
        /// </summary>
        [Description("The URI or local path used to identify the interface.")]
        [XmlAttribute("interface")]
        public string Interface { get; set; }

        /// <summary>
        /// Controls how important this dependency is (i.e. whether ignoring it is an option).
        /// </summary>
        [Description("Controls how important this dependency is (i.e. whether ignoring it is an option).")]
        [XmlAttribute("importance"), DefaultValue(typeof(Importance), "Essential")]
        public Importance Importance { get; set; }

        /// <summary>
        /// This can be used to indicate that this dependency is only needed in some cases.
        /// </summary>
        [Description("This can be used to indicate that this dependency is only needed in some cases.")]
        [XmlAttribute("use"), DefaultValue("")]
        public string Use { get; set; }

        // Preserve order
        private readonly C5.LinkedList<Constraint> _constraints = new C5.LinkedList<Constraint>();
        /// <summary>
        /// A list of version <see cref="Constraint"/>s that must be fulfilled.
        /// </summary>
        [Description("A list of version constraints that must be fulfilled.")]
        [XmlElement("version")]
        public C5.LinkedList<Constraint> Constraints { get { return _constraints; } }

        // Preserve order
        private readonly C5.LinkedList<Binding> _bindings = new C5.LinkedList<Binding>();
        /// <summary>
        /// A list of <see cref="Binding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Description("A list of bindings for implementations to locate dependencies.")]
        [XmlElement(typeof(EnvironmentBinding)), XmlElement(typeof(OverlayBinding))]
        public C5.LinkedList<Binding> Bindings { get { return _bindings; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the dependency in the form "Dependency: Interface (Use)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = "Dependency: " + Interface;
            if (!string.IsNullOrEmpty(Use)) result += " (" + Use + ")";
            return result;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Dependency"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Dependency"/>.</returns>
        public virtual Dependency CloneDependency()
        {
            var dependency = new Dependency {Interface = Interface, Importance = Importance, Use = Use};
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
        /// <inheritdoc/>
        public bool Equals(Dependency other)
        {
            if (other == null) return false;

            if (Interface != other.Interface) return false;
            if (Importance != other.Importance) return false;
            if (Use != other.Use) return false;
            if (!Constraints.SequencedEquals(other.Constraints)) return false;
            if (!Bindings.SequencedEquals(other.Bindings)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Dependency) && Equals((Dependency)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Interface ?? "").GetHashCode();
                result = (result * 397) ^ Importance.GetHashCode();
                result = (result * 397) ^ (Use ?? "").GetHashCode();
                result = (result * 397) ^ Constraints.GetSequencedHashCode();
                result = (result * 397) ^ Bindings.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
