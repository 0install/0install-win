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

using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Chooses a set of <see cref="Implementation"/>s to satisfy the requirements of a program and its user. 
    /// </summary>
    /// <remarks>Implementations of this interface should be immutable.</remarks>
    public interface ISolver
    {
        #region Solve
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to solve the dependencies for.</param>
        /// <param name="policy">The user settings controlling the solving process.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        // ToDo: Add more exceptions (e.g.e feed problems)
        Selections Solve(string feed, Policy policy);
        #endregion
    }
}
