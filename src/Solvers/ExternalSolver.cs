﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.IO;
using Common;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Solvers.External;
using ZeroInstall.Solvers.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Uses an external process to solve requirements.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public sealed class ExternalSolver : ISolver
    {
        #region Dependencies
        private readonly Config _config;
        private readonly IFeedCache _feedCache;
        private readonly IFeedManager _feedManager;
        private readonly IHandler _handler;

        /// <summary>
        /// Creates a new external solver.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedCache">Provides access to a cache of <see cref="Feed"/>s that were downloaded via HTTP(S).</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public ExternalSolver(Config config, IFeedCache feedCache, IFeedManager feedManager, IHandler handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _config = config;

            _feedCache = feedCache;
            _feedManager = feedManager;
            _handler = handler;
        }
        #endregion

        /// <inheritdoc />
        public Selections Solve(Requirements requirements, out bool staleFeeds)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (string.IsNullOrEmpty(requirements.InterfaceID)) throw new ArgumentException(Resources.MissingInterfaceID, "requirements");
            #endregion

            // Execute the external solver
            ISolverControl control;
            if (WindowsUtils.IsWindows) control = new SolverControlBundled(_handler); // Use bundled Python on Windows
            else control = new SolverControlNative(_handler); // Use native Python everywhere else
            string arguments = GetSolverArguments(requirements);
            string result = control.ExecuteSolver(arguments);

            // Flush in-memory cache in case external solver updated something on-disk
            _feedCache.Flush();

            // Detect when feeds get out-of-date
            staleFeeds = result.Contains("<!-- STALE_FEEDS -->");

            // Parse StandardOutput data as XML
            _handler.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var selections = XmlStorage.FromXmlString<Selections>(result);
                selections.Normalize();
                return selections;
            }
                #region Error handling
            catch (InvalidDataException ex)
            {
                Log.Warn("Solver result:\n" + result);
                throw new SolverException(Resources.ExternalSolverOutputErrror, ex);
            }
            #endregion
        }

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements)
        {
            bool temp;
            return Solve(requirements, out temp);
        }

        /// <summary>
        /// Generates a list of arguments to be passed on to the solver script.
        /// </summary>
        /// <param name="requirements">A set of requirements/restrictions imposed by the user on the implementation selection process.</param>
        /// <returns>A list of arguments terminated by a space.</returns>
        private string GetSolverArguments(Requirements requirements)
        {
            string arguments = "";

            for (int i = 0; i < _handler.Verbosity; i++)
                arguments += "--verbose ";
            if (_config.EffectiveNetworkUse == NetworkLevel.Offline) arguments += "--offline ";
            if (_feedManager.Refresh) arguments += "--refresh ";
            //if (additionalStore != null) arguments += "--store=" + additionalStore.DirectoryPath + " ";s
            arguments += requirements.ToCommandLineArgs();

            return arguments;
        }
    }
}
