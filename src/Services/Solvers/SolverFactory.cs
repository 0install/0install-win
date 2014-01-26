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

using ZeroInstall.Model;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Creates <see cref="ISolver"/> instances.
    /// </summary>
    public static class SolverFactory
    {
        /// <summary>
        /// Creates an <see cref="ISolver"/> based on the <see cref="Config"/>.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedCache">Provides access to a cache of <see cref="Feed"/>s that were downloaded via HTTP(S).</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="store">Used to check which <see cref="Implementation"/>s are already cached.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public static ISolver CreateDefault(Config config, IFeedCache feedCache, IFeedManager feedManager, IStore store, IHandler handler)
        {
            ISolver
                backtrackingSolver = new BacktrackingSolver(config, feedManager, store, handler),
                externalSovler = new ExternalSolver(config, feedCache, feedManager, handler),
                fallbackSolver = new FallbackSolver(backtrackingSolver, externalSovler);

            return config.ExperimentalSolver ? fallbackSolver : externalSovler;
        }
    }
}
