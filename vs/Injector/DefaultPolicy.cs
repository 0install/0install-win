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
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Interface;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// A policy that uses the default <see cref="SolverProvider"/>, <see cref="InterfaceProvider"/> and <see cref="Fetcher"/>.
    /// </summary>
    public class DefaultPolicy : Policy
    {
        #region Constructor
        /// <summary>
        /// Creates a new policy for a specific feed.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to solve the dependencies for.</param>
        public DefaultPolicy(string feed) : base(feed, SolverProvider.Default, new InterfaceProvider(), Fetcher.Default)
        {}
        #endregion
    }
}
