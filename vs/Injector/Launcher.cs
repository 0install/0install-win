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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Executes a set of <see cref="Implementation"/>s as a program.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class Launcher
    {
        #region Variables
        // Preserve order, duplicate entries are not allowed
        /// <summary>The <see cref="Implementation"/> to be launched, followed by all its dependencies.</summary>
        private readonly C5.IIndexed<ImplementationSelection> _implementations = new C5.HashedArrayList<ImplementationSelection>();

        /// <summary>Used to locate the selected <see cref="_implementations"/>.</summary>
        private readonly IStore _provider;
        #endregion

        #region Properties
        /// <summary>
        /// An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="ImplementationBase.Main"/>.
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        /// Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.
        /// </summary>
        public string Wrapper { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new launcher from <see cref="Selections"/>.
        /// </summary>
        /// <param name="selections">The <see cref="Implementation"/> to be launched, followed by all its dependencies.</param>
        /// <param name="provider">Used to locate the selected <see cref="Implementation"/>s.</param>
        public Launcher(Selections selections, IStore provider)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (provider == null) throw new ArgumentNullException("provider");
            #endregion

            _implementations.AddAll(selections.Implementations);
            if (_implementations.IsEmpty) throw new ArgumentException(Resources.NoImplementationsPassed, "selections");

            _provider = provider;
        }
        #endregion

        //--------------------//

        #region Run
        /// <summary>
        /// Executes the first entry in <see cref="Implementation"/>s and injects the rest as dependencies.
        /// </summary>
        /// <param name="arguments">Arguments to pass to the launched applications.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Implementation"/>s is not cached yet.</exception>
        public void Execute(string arguments)
        {
            // ToDo: Implement properly

            string implDir = (!string.IsNullOrEmpty(_implementations[0].LocalPath) ? _implementations[0].LocalPath : _provider.GetPath(_implementations[0].ManifestDigest));
            
            Process.Start(Path.Combine(implDir, _implementations[0].Main), arguments);
        }
        #endregion
    }
}
