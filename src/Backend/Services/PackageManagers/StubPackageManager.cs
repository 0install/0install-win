/*
 * Copyright 2010-2014 Bastian Eicher
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

using System.Collections.Generic;
using System.Linq;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.PackageManagers
{
    /// <summary>
    /// Stub implementation of <see cref="IPackageManager"/>, used when there is no native package manager support for the current platform.
    /// </summary>
    public class StubPackageManager : IPackageManager
    {
        /// <summary>
        /// Always returns an empty list.
        /// </summary>
        public IEnumerable<ExternalImplementation> Query(PackageImplementation package, params string[] distributions)
        {
            return Enumerable.Empty<ExternalImplementation>();
        }

        /// <summary>
        /// Always throws <see cref="ImplementationNotFoundException"/>.
        /// </summary>
        public ExternalImplementation Lookup(ImplementationSelection selection)
        {
            throw new ImplementationNotFoundException(Resources.NoPackageManagerSupport);
        }
    }
}
