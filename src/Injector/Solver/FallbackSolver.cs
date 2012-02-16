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

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Wraps to solvers always passing requests to the first one intially and falling back to second one should the first one fail.
    /// </summary>
    public class FallbackSolver : ISolver
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
        public Selections Solve(Requirements requirements, Policy policy, out bool staleFeeds)
        {
            try
            {
                return _firstSolver.Solve(requirements, policy, out staleFeeds);
            }
            catch (SolverException)
            {
                return _secondSolver.Solve(requirements, policy, out staleFeeds);
            }
        }
    }
}
