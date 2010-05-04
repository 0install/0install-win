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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Solver;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Controls the execution of an <see cref="Implementation"/> with its injected <see cref="Dependency"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class Launcher
    {
        #region Properties
        /// <summary>
        /// An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="ImplementationBase.Main"/>.
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        /// Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.
        /// </summary>
        public string Wrapper { get; set; }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.ISequenced<ImplementationSelection> _implementations = new C5.HashedArrayList<ImplementationSelection>();
        /// <summary>
        /// The <see cref="Implementation"/> to be launched, followed by all its dependencies.
        /// </summary>
        public IEnumerable<ImplementationSelection> Implementations { get { return _implementations; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new launcher from <see cref="Selections"/>.
        /// </summary>
        /// <param name="selections">The <see cref="Implementation"/> to be launched, followed by all its dependencies.</param>
        public Launcher(Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            _implementations.AddAll(selections.Implementations);
            if (_implementations.IsEmpty) throw new ArgumentException(Resources.NoImplementationsPassed, "selections");
        }
        #endregion

        //--------------------//

        #region Run
        /// <summary>
        /// Executes the first entry in <see cref="Implementations"/> and injects the rest as dependencies.
        /// </summary>
        public void Run()
        {
            // ToDo: Implement
            throw new NotImplementedException();
        }
        #endregion
    }
}
