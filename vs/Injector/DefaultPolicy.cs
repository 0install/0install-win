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

using ZeroInstall.DownloadBroker;
using ZeroInstall.Model;
using ZeroInstall.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// A policy that uses default settings for everything unless user settings override something.
    /// </summary>
    public class DefaultPolicy : Policy
    {
        #region Properties
        /// <summary>
        /// The location to search for cached <see cref="Implementation"/>s.
        /// </summary>
        /// <remarks>If <see cref="AdditionalStore"/> is <see langword="null"/>, this is the same as <see cref="DownloadBroker.Fetcher.Store"/>.</remarks>
        public override IStore Store
        {
            get  { return ((AdditionalStore == null) ? base.Store : new StoreSet(new[] { AdditionalStore, base.Store })); }
        }

        /// <summary>
        /// A location to search for cached <see cref="Implementation"/>s in addition to <see cref="StoreProvider.GetDefaultStore"/>.
        /// </summary>
        /// <remarks>This location will not be used by <see cref="Policy.Fetcher"/>.</remarks>
        public IStore AdditionalStore { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new policy with default settings for everything.
        /// </summary>
        public DefaultPolicy() : base(SolverFactory.CreateDefaultSolver(), Fetcher.Default)
        {}
        #endregion
    }
}
