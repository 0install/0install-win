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
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Info;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Updates Zero Install itself to the most recent version.
    /// </summary>
    public sealed class SelfUpdate : Run
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "self-update";

        /// <inheritdoc/>
        protected override string Description => Resources.DescriptionSelfUpdate;

        /// <inheritdoc/>
        protected override string Usage => "[OPTIONS]";
        #endregion

        #region State
        /// <summary>Perform the update even if the currently installed version is the same or newer.</summary>
        private bool _force;

        /// <summary>Restart the <see cref="Central"/> GUI after the update.</summary>
        private bool _restartCentral;

        /// <inheritdoc/>
        public SelfUpdate([NotNull] ICommandHandler handler) : base(handler)
        {
            NoWait = true;
            FeedManager.Refresh = true;
            Config.AllowApiHooking = false;

            //Options.Remove("no-wait");
            //Options.Remove("refresh");

            Options.Add("force", () => Resources.OptionForceSelfUpdate, _ => _force = true);
            Options.Add("restart-central", () => Resources.OptionRestartCentral, _ => _restartCentral = true);
        }
        #endregion

        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            // NOTE: Does not call base method

            AdditionalArgs.AddRange(Options.Parse(args));
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + AdditionalArgs.JoinEscapeArguments(), null);

            SetInterfaceUri(Config.SelfUpdateUri);
            if (ProgramUtils.GuiAssemblyName != null) Requirements.Command = Command.NameRunGui;

            // Instruct new version of Zero Install in the cache to deploy itself over the location of the current version
            AdditionalArgs.AddRange(new[] {MaintenanceMan.Name, MaintenanceMan.Deploy.Name, "--batch", Locations.InstallBase});

            if (_restartCentral) AdditionalArgs.Add("--restart-central");
        }

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            if (ProgramUtils.IsRunningFromCache) throw new NotSupportedException(Resources.SelfUpdateBlocked);

            try
            {
                Solve();
            }
                #region Error handling
            catch (WebException ex) when (Handler.Background)
            {
                Log.Info("Suppressed network-related error message due to background mode");
                Log.Info(ex);
                return ExitCode.WebError;
            }
            catch (SolverException ex) when (Handler.Background)
            {
                Log.Info("Suppressed Solver-related error message due to background mode");
                Log.Info(ex);
                return ExitCode.SolverError;
            }
            #endregion

            if (UpdateFound())
            {
                DownloadUncachedImplementations();

                Handler.CancellationToken.ThrowIfCancellationRequested();
                if (!Handler.Ask(string.Format(Resources.SelfUpdateAvailable, Selections.MainImplementation.Version), defaultAnswer: true))
                    throw new OperationCanceledException();

                LaunchImplementation();
                return ExitCode.OK;
            }
            else return ExitCode.OK;
        }

        private bool UpdateFound()
        {
            return _force || (Selections.MainImplementation.Version > new ImplementationVersion(AppInfo.Current.Version));
        }
    }
}
