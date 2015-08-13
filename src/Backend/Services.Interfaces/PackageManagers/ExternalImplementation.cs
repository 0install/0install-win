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
using JetBrains.Annotations;
using NanoByte.Common.Values;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.PackageManagers
{
    /// <summary>
    /// An implementation provided by an external package manager.
    /// </summary>
    /// <seealso cref="IPackageManager"/>
    public sealed class ExternalImplementation : Implementation, IEquatable<ExternalImplementation>
    {
        #region Constants
        /// <summary>
        /// This is prepended to <see cref="ImplementationBase.ID"/> for all <see cref="ExternalImplementation"/>.
        /// </summary>
        /// <remarks>Also used to mark regular <see cref="Implementation"/>s that act as proxies for <see cref="ExternalImplementation"/>s.</remarks>
        public const string PackagePrefix = "package:";
        #endregion

        /// <summary>
        /// The name of the distribution (e.g. Debian, RPM) where this implementation comes from.
        /// </summary>
        [NotNull]
        public string Distribution { get; set; }

        /// <summary>
        /// The name of the package in the <see cref="Distribution"/>.
        /// </summary>
        [NotNull]
        public string Package { get; set; }

        /// <summary>
        /// Indicates whether this implementation is currently installed.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// A file which, if present, indicates that this implementation <see cref="IsInstalled"/>.
        /// </summary>
        /// <remarks>This makes it possible to avoid <see cref="IPackageManager.Lookup"/> calls for better performance.</remarks>
        /// <seealso cref="ImplementationSelection.QuickTestFile"/>
        [CanBeNull]
        public string QuickTestFile { get; set; }

        /// <summary>
        /// Creates a new external implementation.
        /// </summary>
        /// <param name="distribution">The name of the distribution (e.g. Debian, RPM) where this implementation comes from.</param>
        /// <param name="package">The name of the package in the <paramref name="distribution"/>.</param>
        /// <param name="version">The version number of the implementation.</param>
        /// <param name="cpu">For platform-specific binaries, the CPU architecture for which the implementation was compiled.</param>
        public ExternalImplementation([NotNull] string distribution, [NotNull] string package, [NotNull] ImplementationVersion version, Cpu cpu = Cpu.All)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(distribution)) throw new ArgumentNullException("distribution");
            if (string.IsNullOrEmpty(package)) throw new ArgumentNullException("package");
            if (version == null) throw new ArgumentNullException("version");
            #endregion

            ID = PackagePrefix + distribution.ToLowerInvariant() + ":" + package + ":" + version;

            Version = version;
            Stability = Stability.Packaged;
            Distribution = distribution;
            Package = package;

            if (cpu != Cpu.All)
            {
                ID += ":" + cpu.ConvertToString();
                Architecture = new Architecture(OS.All, cpu);
            }
        }

        /// <summary>
        /// Creates a new external implementation from an <see cref="ImplementationBase.ID"/>.
        /// </summary>
        /// <param name="id">The ID to parse.</param>
        /// <exception cref="FormatException"><paramref name="id"/> is not a standard <see cref="ExternalImplementation"/> ID.</exception>
        public static ExternalImplementation FromID([NotNull] string id)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            var parts = id.Split(':');
            if (parts.Length < 4 || parts[0] + ":" != PackagePrefix)
                throw new FormatException();

            var implementation = new ExternalImplementation(distribution: parts[1], package: parts[2], version: new ImplementationVersion(parts[3])) {ID = id};
            if (parts.Length >= 5) implementation.Architecture = new Architecture(OS.All, parts[4].ConvertFromString<Cpu>());

            return implementation;
        }

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ExternalImplementation other)
        {
            if (other == null) return false;
            return base.Equals(other) && Distribution == other.Distribution && Package == other.Package && IsInstalled == other.IsInstalled && QuickTestFile == other.QuickTestFile;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ExternalImplementation)) return false;
            return Equals((ExternalImplementation)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Distribution.GetHashCode();
                result = (result * 397) ^ Package.GetHashCode();
                if (QuickTestFile != null) result = (result * 397) ^ QuickTestFile.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
