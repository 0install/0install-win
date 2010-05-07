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

using ZeroInstall.Store.Interface;

namespace ZeroInstall.Solver
{
    /// <summary>
    /// Provides access to <see cref="ISolver"/> implementations.
    /// </summary>
    public static class SolverFactory
    {
        /// <summary>
        /// Instantiates the default implementation of <see cref="ISolver"/>.
        /// </summary>
        public static ISolver DefaultSolver()
        {
            // ToDo: Make more flexible
            return new PythonSolver(new InterfaceProvider());
        }
    }
}
