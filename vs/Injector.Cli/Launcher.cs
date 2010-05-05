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
using ZeroInstall.DownloadBroker;
using ZeroInstall.Model;
using ZeroInstall.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.Cli
{
    /// <summary>
    /// Launches an <see cref="Implementation"/>
    /// </summary>
    public static class Launcher
    {
        /// <summary>
        /// Launches an application identified by a feed URI.
        /// </summary>
        public static void Run(Uri feed)
        {
            // Solve the dependencies
            var solver = SolverFactory.GetDefaultSolver();
            var selections = solver.Solve(feed);

            // Find out which implementations are missing and download them
            IImplementationProvider provider = new StoreSet();
            var launcher = new Injector.Launcher(selections, provider);
            new DownloadRequest(launcher.ListMissingImplementations(), provider).RunSync();

            // Runt the application
            launcher.Run();
        }
    }
}
