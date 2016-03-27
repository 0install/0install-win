/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Fetchers;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Uses an external process controlled via a IPC to solve requirements. The external process is itself provided by another <see cref="ISolver"/>.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public partial class ExternalSolver : ISolver
    {
        private const string ApiVersion = "2.7";

        #region Dependencies
        private readonly ISolver _backingSolver;
        private readonly ISelectionsManager _selectionsManager;
        private readonly IFetcher _fetcher;
        private readonly IExecutor _executor;
        private readonly IFeedManager _feedManager;
        private readonly ITaskHandler _handler;
        private readonly Requirements _solverRequirements;

        /// <summary>
        /// Creates a new external JSON solver.
        /// </summary>
        /// <param name="backingSolver">An internal solver used to find an implementation of the external solver.</param>
        /// <param name="selectionsManager">Used to check whether the external solver is already in the cache.</param>
        /// <param name="fetcher">Used to download implementations of the external solver.</param>
        /// <param name="executor">Used to launch the external solver.</param>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public ExternalSolver([NotNull] ISolver backingSolver, [NotNull] ISelectionsManager selectionsManager, [NotNull] IFetcher fetcher, [NotNull] IExecutor executor, [NotNull] Config config, [NotNull] IFeedManager feedManager, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (backingSolver == null) throw new ArgumentNullException("backingSolver");
            if (selectionsManager == null) throw new ArgumentNullException("selectionsManager");
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            if (executor == null) throw new ArgumentNullException("executor");
            if (config == null) throw new ArgumentNullException("config");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _backingSolver = backingSolver;
            _selectionsManager = selectionsManager;
            _fetcher = fetcher;
            _executor = executor;
            _feedManager = feedManager;
            _handler = handler;

            _solverRequirements = new Requirements(config.ExternalSolverUri);
        }
        #endregion

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (requirements.InterfaceUri == null) throw new ArgumentException(Resources.MissingInterfaceUri, "requirements");
            #endregion

            Selections selections = null;
            _handler.RunTask(new SimpleTask(Resources.ExternalSolverRunning, () =>
            {
                using (var control = new JsonControl(GetStartInfo())
                {
                    {"confirm", args => DoConfirm((string)args[0])},
                    {"confirm-keys", args => DoConfirmKeys(new FeedUri((string)args[0]), args[1].ReparseAsJson<Dictionary<string, string[][]>>())},
                    {"update-key-info", args => null}
                })
                {
                    control.Invoke(args =>
                    {
                        if ((string)args[0] == "ok")
                        {
                            _feedManager.Stale = args[1].ReparseAsJson(new {stale = false}).stale;
                            selections = XmlStorage.FromXmlString<Selections>((string)args[2]);
                        }
                        else throw new SolverException(((string)args[1]).Replace("\n", Environment.NewLine));
                    }, "select", GetEffectiveRequirements(requirements), _feedManager.Refresh);
                    while (selections == null)
                    {
                        control.HandleStderr();
                        control.HandleNextChunk();
                    }
                    control.HandleStderr();
                }
            }));

            return selections;
        }

        private Selections _solverSelections;

        private ProcessStartInfo GetStartInfo()
        {
            if (_solverSelections == null)
                _solverSelections = _backingSolver.Solve(_solverRequirements);

            var missing = _selectionsManager.GetUncachedImplementations(_solverSelections);
            _fetcher.Fetch(missing);

            var arguments = new[] {"--console", "slave", ApiVersion};
            for (int i = 0; i < (int)_handler.Verbosity; i++)
                arguments = arguments.Append("--verbose");
            var startInfo = _executor.GetStartInfo(_solverSelections, arguments);

            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            if (Locations.IsPortable)
                startInfo.EnvironmentVariables["ZEROINSTALL_PORTABLE_BASE"] = Locations.PortableBase;

            return startInfo;
        }

        private static Requirements GetEffectiveRequirements(Requirements requirements)
        {
            var effectiveRequirements = requirements.Clone();
            effectiveRequirements.Command = requirements.Command ?? (requirements.Architecture.Cpu == Cpu.Source ? Command.NameCompile : Command.NameRun);
            return effectiveRequirements;
        }

        private string DoConfirm(string message)
        {
            return _handler.Ask(message) ? "ok" : "cancel";
        }

        private string DoConfirmKeys(FeedUri feedUri, Dictionary<string, string[][]> keys)
        {
            var key = keys.First();
            var hint = key.Value[0];

            string message = string.Format(Resources.AskKeyTrust, feedUri.ToStringRfc(), key.Key, hint[1], feedUri.Host);
            return _handler.Ask(message) ? "ok" : "cancel";
        }
    }
}
