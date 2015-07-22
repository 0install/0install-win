/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Properties;
using ZeroInstall.Services.Solvers.Python;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Uses a Python process to solve requirements.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public sealed class PythonSolver : ISolver
    {
        #region Dependencies
        private readonly Config _config;
        private readonly IFeedManager _feedManager;
        private readonly IFeedCache _feedCache;
        private readonly ITaskHandler _handler;

        /// <summary>
        /// Creates a new Python solver.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedCache">The underlying cache backing the <paramref name="feedManager"/>.</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public PythonSolver([NotNull] Config config, [NotNull] IFeedManager feedManager, [NotNull] IFeedCache feedCache, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _config = config;
            _feedManager = feedManager;
            _feedCache = feedCache;
            _handler = handler;
        }
        #endregion

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (requirements.InterfaceUri == null) throw new ArgumentException(Resources.MissingInterfaceUri, "requirements");
            #endregion

            Log.Info("Running Python Solver for: " + requirements);

            // Execute the external solver
            ISolverControl control;
            if (WindowsUtils.IsWindows) control = new SolverControlBundled(_handler); // Use bundled Python on Windows
            else control = new SolverControlNative(_handler); // Use native Python everywhere else
            var arguments = GetSolverArguments(requirements);

            string result = null;
            _handler.RunTask(new SimpleTask(Resources.ExternalSolverRunning, () => { result = control.Execute(arguments); }));

            // Flush in-memory cache in case external solver updated something on-disk
            _feedCache.Flush();

            // Detect when feeds get out-of-date
            _feedManager.Stale = result.Contains("<!-- STALE_FEEDS -->");

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
                Log.Warn("Solver result:" + Environment.NewLine + result);
                throw new SolverException(Resources.ExternalSolverOutputErrror, ex);
            }
            #endregion
        }

        /// <summary>
        /// Generates a list of arguments to be passed on to the solver script.
        /// </summary>
        /// <param name="requirements">A set of requirements/restrictions imposed by the user on the implementation selection process.</param>
        /// <returns>A list of arguments terminated by a space.</returns>
        private string[] GetSolverArguments(Requirements requirements)
        {
            var arguments = requirements.ToCommandLineArgs();

            for (int i = 0; i < (int)_handler.Verbosity; i++)
                arguments = arguments.Prepend("--verbose");
            if (_config.NetworkUse == NetworkLevel.Offline) arguments = arguments.Prepend("--offline");
            if (_feedManager.Refresh) arguments = arguments.Prepend("--refresh");

            return arguments;
        }
    }
}
