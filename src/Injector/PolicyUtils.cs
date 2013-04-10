/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.IO;
using System.Net;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Helper methods to simplify using functionality provided by classes aggregated by <see cref="Policy"/>.
    /// </summary>
    public static class PolicyUtils
    {
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="policy">Provides class dependencies.</param>
        /// <param name="requirements">A set of requirements/restrictions imposed by the user on the implementation selection process.</param>
        /// <param name="staleFeeds">Returns <see langword="true"/> if one or more of the <see cref="Model.Feed"/>s used by the solver have passed <see cref="Config.Freshness"/>.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="requirements"/> is incomplete.</exception>
        /// <exception cref="IOException">Thrown if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        public static Selections Solve(this Policy policy, Requirements requirements, out bool staleFeeds)
        {
            return policy.Solver.Solve(requirements, policy, out staleFeeds);
        }

        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="policy">Provides class dependencies.</param>
        /// <param name="requirements">A set of requirements/restrictions imposed by the user on the implementation selection process.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="requirements"/> is incomplete.</exception>
        /// <exception cref="IOException">Thrown if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        public static Selections Solve(this Policy policy, Requirements requirements)
        {
            bool staleFeeds;
            return policy.Solver.Solve(requirements, policy, out staleFeeds);
        }

        /// <summary>
        /// Downloads a set of <see cref="Implementation"/>s to the <see cref="Store"/> and returns once this process is complete.
        /// </summary>
        /// <param name="policy">Provides class dependencies.</param>
        /// <param name="implementations">The <see cref="Model.Implementation"/>s to be downloaded.</param>
        /// <exception cref="OperationCanceledException">Thrown if a download or IO task was canceled from another thread.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">Thrown if a file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to <see cref="IFetcher.Store"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        public static void FetchImplementations(this Policy policy, IEnumerable<Implementation> implementations)
        {
            policy.Fetcher.FetchImplementations(implementations, policy.Handler);
        }
    }
}