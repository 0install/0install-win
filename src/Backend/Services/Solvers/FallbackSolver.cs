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
using NanoByte.Common;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Wraps two solvers always passing requests to the primary one intially and falling back to secondary one should the primary one fail.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public sealed class FallbackSolver : ISolver
    {
        /// <summary>
        /// The solver to run initially.
        /// </summary>
        private readonly ISolver _primarySolver;

        /// <summary>
        /// The solver to fall back to should <see cref="_primarySolver"/> fail.
        /// </summary>
        private readonly ISolver _secondarySolver;

        /// <summary>
        /// Creates a new fallback solver.
        /// </summary>
        /// <param name="primarySolver">The solver to run initially.</param>
        /// <param name="secondarySolver">The solver to fall back to should <paramref name="primarySolver"/> fail.</param>
        public FallbackSolver(ISolver primarySolver, ISolver secondarySolver)
        {
            _primarySolver = primarySolver;
            _secondarySolver = secondarySolver;
        }

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements)
        {
            try
            {
                return _primarySolver.Solve(requirements);
            }
            catch (SolverException ex)
            {
                Log.Info(string.Format("Primary solver reported: " + ex.Message + Environment.NewLine + "Falling back to secondary solver.", requirements));
                Log.Debug(ex);

                return _secondarySolver.Solve(requirements);
            }
        }
    }
}
