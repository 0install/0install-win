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

using Common;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Wraps two solvers always passing requests to the first one intially and falling back to second one should the first one fail.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public sealed class FallbackSolver : ISolver
    {
        /// <summary>
        /// The solver to run initially.
        /// </summary>
        private readonly ISolver _firstSolver;

        /// <summary>
        /// The solver to fall back to should <see cref="_firstSolver"/> fail.
        /// </summary>
        private readonly ISolver _secondSolver;

        /// <summary>
        /// Creates a new fallback solver.
        /// </summary>
        /// <param name="firstSolver">The solver to run initially.</param>
        /// <param name="secondSolver">The solver to fall back to should <paramref name="firstSolver"/> fail.</param>
        public FallbackSolver(ISolver firstSolver, ISolver secondSolver)
        {
            _firstSolver = firstSolver;
            _secondSolver = secondSolver;
        }

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements, out bool staleFeeds)
        {
            try
            {
                return _firstSolver.Solve(requirements, out staleFeeds);
            }
            catch (SolverException ex)
            {
				Log.Warn(string.Format("Falling back to secondary solver for {0} because: {1}", requirements, ex.Message));
                return _secondSolver.Solve(requirements, out staleFeeds);
            }
        }

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements)
        {
            try
            {
                return _firstSolver.Solve(requirements);
            }
            catch (SolverException)
            {
                return _secondSolver.Solve(requirements);
            }
        }
    }
}
