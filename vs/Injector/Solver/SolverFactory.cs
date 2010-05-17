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
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Interface;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Creates a new <see cref="ISolver"/> instance on demand.
    /// </summary>
    public abstract class SolverFactory
    {
        #region Singleton properties
        private static readonly SolverFactory _default = new DefaultSolverFactory();
        /// <summary>
        /// A <see cref="SolverFactory"/> that creates instances of the default <see cref="ISolver"/>.
        /// </summary>
        public static SolverFactory Default { get { return _default; } }
        #endregion

        #region Abstract methods
        /// <summary>
        /// Creates a new <see cref="ISolver"/> instance.
        /// </summary>
        /// <param name="interfaceCache">The source used to request <see cref="Interface"/>s.</param>
        /// <param name="store">The location to search for cached <see cref="Implementation"/>s.</param>
        public abstract ISolver CreateSolver(InterfaceCache interfaceCache, IStore store);
        #endregion

        #region Inner classes
        private sealed class DefaultSolverFactory : SolverFactory
        {
            public override ISolver CreateSolver(InterfaceCache interfaceCache, IStore store)
            {
                return new PythonSolver(interfaceCache, store);
            }
        }
        #endregion
    }
}
