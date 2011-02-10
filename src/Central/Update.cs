/*
 * Copyright 2010-2011 Bastian Eicher
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

using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;

namespace ZeroInstall.Central
{
    /// <summary>
    /// Helper methods for updating installed applications.
    /// </summary>
    public static class Update
    {
        /// <summary>
        /// Silently updates an application in the background.
        /// </summary>
        /// <param name="interfaceID">The interface to check for updates.</param>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="interfaceID"/> is not a valid interface ID.</exception>
        public static void BackgroundUpdate(string interfaceID)
        {
            var handler = new SilentHandler();
            var policy = Policy.CreateDefault();
            policy.FeedManager.Refresh = true;

            var selections = SolverProvider.Default.Solve(new Requirements { InterfaceID = interfaceID }, policy, handler);
            policy.Fetcher.RunSync(new FetchRequest(selections.ListUncachedImplementations(policy)), handler);
        }
    }
}
