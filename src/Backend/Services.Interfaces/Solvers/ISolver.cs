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

using System;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Chooses a set of <see cref="Store.Model.Implementation"/>s to satisfy the requirements of a program and its user.
    /// </summary>
    /// <remarks>This is an application of the strategy pattern. Implementations of this interface are immutable and thread-safe.</remarks>
    public interface ISolver
    {
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="requirements">A set of requirements/restrictions imposed by the user on the implementation selection process.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        /// <exception cref="OperationCanceledException">The user canceled the process.</exception>
        /// <exception cref="ArgumentException"><paramref name="requirements"/> is incomplete.</exception>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">A problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">The signature data of a remote feed file could not be verified.</exception>
        /// <exception cref="SolverException">The dependencies could not be solved.</exception>
        [NotNull]
        Selections Solve([NotNull] Requirements requirements);
    }
}
