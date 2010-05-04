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
using ZeroInstall.Model;

namespace ZeroInstall.Solver
{
    /// <summary>
    /// Chooses a set of <see cref="Implementation"/>s to satisfy the requirements of a program and its user. 
    /// </summary>
    public interface ISolver
    {
        #region Properties
        /// <summary>
        /// Try to avoid network usage.
        /// </summary>
        bool Offline { get; set; }

        /// <summary>
        /// Fetch a fresh copy of all used interfaces.
        /// </summary>
        bool Refresh { get; set; }

        /// <summary>
        /// An additional directory to search for cached <see cref="Interface"/>s.
        /// </summary>
        string AdditionalStore { get; set; }
        #endregion

        //--------------------//

        #region Solve
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="feed">The feed to solve the dependencies for.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Interface files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        // ToDo: Add exceptions
        Selections Solve(Uri feed);

        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="feed">The feed to solve the dependencies for.</param>
        /// <param name="notBefore">The minimum version of the main <see cref="Implementation"/> to choose.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Interface files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        // ToDo: Add exceptions
        Selections Solve(Uri feed, ImplementationVersion notBefore);
        #endregion
    }
}
