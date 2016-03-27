/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.PackageManagers
{
    /// <summary>
    /// Base class for <see cref="IPackageManager"/> implementations using template methods.
    /// </summary>
    public abstract class PackageManagerBase : IPackageManager
    {
        /// <summary>
        /// The name of the <see cref="PackageImplementation.Distributions"/> this package manager provides packages for.
        /// </summary>
        [NotNull]
        protected abstract string DistributionName { get; }

        /// <inheritdoc/>
        public IEnumerable<ExternalImplementation> Query(PackageImplementation package, params string[] distributions)
        {
            #region Sanity checks
            if (package == null) throw new ArgumentNullException("package");
            if (distributions == null) throw new ArgumentNullException("distributions");
            #endregion

            if (!distributions.ContainsOrEmpty(DistributionName) || !package.Distributions.ContainsOrEmpty(DistributionName)) yield break;

            foreach (var implementation in GetImplementations(package.Package))
            {
                CopyValues(from: package, to: implementation);
                yield return implementation;
            }
        }

        /// <inheritdoc/>
        public ExternalImplementation Lookup(ImplementationSelection selection)
        {
            #region Sanity checks
            if (selection == null) throw new ArgumentNullException("selection");
            #endregion

            try
            {
                var referenceImpl = ExternalImplementation.FromID(selection.ID);

                // Reference implementation from ID does not contain all required information.
                // Therefore, find the original implementation.
                var implementation = GetImplementations(referenceImpl.Package)
                    .First(x => x.Version == referenceImpl.Version && x.Architecture == referenceImpl.Architecture);

                CopyValues(from: selection, to: implementation);
                return implementation;
            }
                #region Error handling
            catch (FormatException)
            {
                throw new ImplementationNotFoundException(string.Format(Resources.UnknownPackageID, selection.ID, DistributionName));
            }
            catch (InvalidOperationException)
            {
                throw new ImplementationNotFoundException(string.Format(Resources.UnknownPackageID, selection.ID, DistributionName));
            }
            #endregion
        }

        /// <summary>
        /// Retrieves a set of specific native implementations for a package name.
        /// </summary>
        /// <param name="packageName">The name of the package to look for.</param>
        [NotNull, ItemNotNull]
        protected abstract IEnumerable<ExternalImplementation> GetImplementations([NotNull] string packageName);

        private static void CopyValues(Element from, ExternalImplementation to)
        {
            to.Commands.AddRange(from.Commands);
            to.Bindings.AddRange(from.Bindings);
        }
    }
}
