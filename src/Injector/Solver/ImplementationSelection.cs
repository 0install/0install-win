/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Serialization;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// An executable implementation of a <see cref="Feed"/> as a part of a <see cref="Selections"/>.
    /// </summary>
    /// <remarks>This class does not contain information on how to download the implementation in case it is not in cache. That must be obtained from a <see cref="Model.Implementation"/> instance.</remarks>
    /// <seealso cref="Selections.Implementations"/>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [XmlType("selection", Namespace = Feed.XmlNamespace)]
    public sealed class ImplementationSelection : ImplementationBase, IEquatable<ImplementationSelection>
    {
        #region Constants
        /// <summary>
        /// This is prepended to <see cref="FromFeed"/> if data is pulled from a native package manager.
        /// </summary>
        /// <seealso cref="Package"/>
        /// <seealso cref="PackageImplementation"/>
        public const string DistributionFeedPrefix = "distribution:";
        #endregion

        #region Properties
        /// <summary>
        /// The URI or local path of the interface this implementation is for.
        /// </summary>
        [Description("The URI or local path of the interface this implementation is for.")]
        [XmlAttribute("interface")]
        public string InterfaceID { get; set; }

        /// <summary>
        /// The URL or local path of the feed that contains this implementation.
        /// <see cref="DistributionFeedPrefix"/> is prepended if data is pulled from a native package manager.
        /// If <see langword="null"/> or <see cref="string.Empty"/> use <see cref="InterfaceID"/> instead.
        /// </summary>
        [Description("The URL or local path of the feed that contains this implementation. \"distribution:\" is prepended if data is pulled from a native package manager. If null or empty use InterfaceID instead.")]
        [XmlAttribute("from-feed")]
        public string FromFeed { get; set; }

        /// <summary>
        /// The name of the package in the distribution-specific package manager.
        /// Only set for <see cref="PackageImplementation"/>s; <see langword="null"/> if this comes from a real Zero Instal <see cref="Model.Implementation"/>.
        /// </summary>
        [Category("Identity"), Description("The name of the package in the distribution-specific package manager. Only set for PackageImplementation; null if this comes from a real Zero Instal implementation.")]
        [XmlAttribute("package")]
        public string Package { get; set; }

        // Order is always alphabetical, duplicate string entries are not allowed
        private readonly C5.TreeSet<string> _distributions = new C5.TreeSet<string>();

        /// <summary>
        /// A list of distribution names where <see cref="Package"/> applies.
        /// Only set for <see cref="PackageImplementation"/>s; <see langword="null"/> if this comes from a real Zero Instal <see cref="Model.Implementation"/>.
        /// </summary>
        [Category("Identity"), Description("A space-separated list of distribution names where the package name applies. Only set for PackageImplementation; null if this comes from a real Zero Instal implementation.")]
        [XmlIgnore]
        public ICollection<string> Distributions { get { return _distributions; } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("distributions"), DefaultValue(""), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DistributionsString
        {
            get
            {
                // Serialize list as string split by spaces
                var output = new StringBuilder();
                foreach (var distribution in _distributions) output.Append(distribution.Replace(' ', '_') + ' ');

                // Return without trailing space
                return output.ToString().TrimEnd();
            }
            set
            {
                _distributions.Clear();
                if (string.IsNullOrEmpty(value)) return;

                // Replace list by parsing input string split by spaces
                foreach (string distribution in value.Split(' ')) _distributions.Add(distribution);
            }
        }
        #endregion

        //--------------------//

        #region Store
        /// <summary>
        /// Tries to locate the implementation in an <see cref="IStore"/>.
        /// </summary>
        /// <param name="store">The store to search for the implementation storage location.</param>
        /// <returns>A fully qualified path to the directory containing the implementation, a native package name prefixed with <code>package:</code> or <see langword="null"/> if the implementation is not cached yet.</returns>
        public string GetPath(IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            if (ID.StartsWith("package:")) return "(" + ID + ")";
            else
            {
                try
                {
                    return store.GetPath(ManifestDigest);
                }
                catch (ImplementationNotFoundException)
                {
                    return null;
                }
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString() + " (" + InterfaceID + ")";
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ImplementationSelection"/>
        /// </summary>
        /// <returns>The cloned <see cref="ImplementationSelection"/>.</returns>
        public ImplementationSelection CloneImplementation()
        {
            var implementation = new ImplementationSelection {InterfaceID = InterfaceID, FromFeed = FromFeed, Package = Package};
            CloneFromTo(this, implementation);
            return implementation;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="ImplementationSelection"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ImplementationSelection"/>.</returns>
        public override Element Clone()
        {
            return CloneImplementation();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ImplementationSelection other)
        {
            if (other == null) return false;

            return base.Equals(other) && Equals(other.InterfaceID, InterfaceID) && Equals(other.FromFeed, FromFeed) && Equals(other.Package, Package);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ImplementationSelection) && Equals((ImplementationSelection)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (InterfaceID ?? "").GetHashCode();
                result = (result * 397) ^ (FromFeed ?? "").GetHashCode();
                result = (result * 397) ^ (Package ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
