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
using System.IO;
using Common;
using ZeroInstall.Model;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Launcher.Solver
{
    /// <summary>
    /// Chooses a set of <see cref="Implementation"/>s to satisfy the requirements of a program and its user. 
    /// </summary>
    /// <remarks>This is an application of the strategy pattern. Implementations of this interface should be immutable.</remarks>
    public interface ISolver
    {
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="interfaceID">The URI or local path (must be absolute) to the interface to solve the dependencies for.</param>
        /// <param name="policy">The user settings controlling the solving process.</param>
        /// <param name="handler">A callback object used if the the user needs to be asked any questions (such as whether to trust a certain GPG key).</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        /// <exception cref="UserCancelException">Thrown if the user clicked the "Cancel" button.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="interfaceID"/> is not a valid URI or absolute local path.</exception>
        /// <exception cref="IOException">Thrown if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        Selections Solve(string interfaceID, Policy policy, IFeedHandler handler);
    }
}
