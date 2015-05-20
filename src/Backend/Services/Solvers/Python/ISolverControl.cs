/*
 * Copyright 2010-2015 Bastian Eicher
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

using System.IO;

namespace ZeroInstall.Services.Solvers.Python
{
    /// <summary>
    /// Interacts with the external process used by <see cref="PythonSolver"/>.
    /// </summary>
    internal interface ISolverControl
    {
        /// <summary>
        /// Runs the external solver, processes its output and waits until it has terminated.
        /// </summary>
        /// <param name="arguments">Command-line arguments to launch the solver with.</param>
        /// <returns>The solver's complete output to the stdout-stream.</returns>
        /// <exception cref="IOException">The external solver could not be launched.</exception>
        string Execute(params string[] arguments);
    }
}
