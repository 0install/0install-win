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
using System.Xml.Serialization;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model
{

    #region Enumerations
    /// <summary>
    /// Describes how important a dependency is (i.e. whether ignoring it is an option).
    /// </summary>
    public enum Importance
    {
        /// <summary>A version of the <see cref="Dependency"/> must be selected.</summary>
        [XmlEnum("essential")]
        Essential,

        /// <summary>No version of the <see cref="Dependency"/> is also an option, although selecting a version is preferable to not selecting one.</summary>
        [XmlEnum("recommended")]
        Recommended
    }
    #endregion

    /// <summary>
    /// A reference to an interface that is required as dependency.
    /// </summary>
    [Description("A reference to an interface that is required as dependency.")]
    [Serializable, XmlRoot("requires", Namespace = Feed.XmlNamespace), XmlType("depedency", Namespace = Feed.XmlNamespace)]
    public class Dependency : Restriction, IInterfaceUriBindingContainer, IEquatable<Dependency>
    {
        #region Constants
        /// <summary>
        /// A <see cref="Use"/> value indicating that a depedency is only required for automatic test execution.
        /// </summary>
        public const string UseTesting = "testing";
        #endregion

        #region Properties
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
        [TypeConverter(typeof(UseConverter))]
        [XmlAttribute("use"), DefaultValue("")]
        public string Use { get; set; }

        private readonly List<Binding> _bindings = new List<Binding>();

        /// <summary>
        /// A list of <see cref="Binding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(GenericBinding)), XmlElement(typeof(EnvironmentBinding)), XmlElement(typeof(OverlayBinding)), XmlElement(typeof(ExecutableInVar)), XmlElement(typeof(ExecutableInPath))]
        public List<Binding> Bindings { get { return _bindings; } }
        #endregion

        //--------------------//

        #region Normalize
        /// <inheritdoc/>
        public override void Normalize()
        {
            base.Normalize();

            // Apply if-0install-version filter
            Bindings.RemoveAll(FilterMismatch);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the dependency in the form "Interface (Use)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = (InterfaceUri == null) ? "-" : InterfaceUri.ToStringRfc();
            if (!string.IsNullOrEmpty(Use)) result += " (" + Use + ")";
            return result;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Dependency"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Dependency"/>.</returns>
        public Dependency CloneDependency()
        {
            var dependency = new Dependency {InterfaceUri = InterfaceUri, OS = OS, Versions = Versions, Importance = Importance, Use = Use};
            dependency.Constraints.AddRange(Constraints.CloneElements());
            dependency.Distributions.AddRange(Distributions);
            dependency.Bindings.AddRange(Bindings.CloneElements());
            return dependency;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Dependency"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Dependency"/>.</returns>
        public override Restriction Clone()
        {
            return CloneDependency();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Dependency other)
        {
            return base.Equals(other) && Importance == other.Importance && Use == other.Use && Bindings.SequencedEquals(other.Bindings);
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
                int result = base.GetHashCode();
                result = (result * 397) ^ (int)Importance;
                result = (result * 397) ^ (Use ?? "").GetHashCode();
                result = (result * 397) ^ Bindings.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
