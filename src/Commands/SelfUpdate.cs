/*
 * Copyright 2010-2012 Bastian Eicher
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
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Updates Zero Install itself to the most recent version.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class SelfUpdate : Run
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "self-update";
        #endregion

        #region Variables
        /// <summary>Perform the update even if the currently installed version is the same or newer.</summary>
        private bool _force;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionSelfUpdate; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionSelfUpdate; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public SelfUpdate(Policy policy) : base(policy)
        {
            NoWait = true;
            Policy.FeedManager.Refresh = true;
            Policy.Config.AllowApiHooking = false;
            Requirements.CommandName = "update";

            //Options.Remove("no-wait");
            //Options.Remove("refresh");

            Options.Add("force", Resources.OptionForceSelfUpdate, unused => _force = true);
        }
        #endregion

        //--------------------//

        #region Parse
        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            // Don't allow custom arguments
            if (Options.Parse(args).Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs, "");

            Requirements.InterfaceID = Policy.Config.SelfUpdateID;

            // Always pass in the installation directory to the updater as an argument
            AdditionalArgs.Add(Locations.InstallBase);

            IsParsed = true;
        }
        #endregion

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            #endregion

            Policy.Handler.ShowProgressUI();
            Solve();
            SelectionsUI();

            // Make sure the update is actually a newer version
            var currentVersion = new ImplementationVersion(AppInfo.Version);
            var newVersion = Selections.Implementations[0].Version;
            if (!_force && currentVersion >= newVersion)
            {
                // Show a "nothing changed" message (but not in batch mode, since it is too unimportant)
                if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.ChangesFound, Resources.NoUpdatesFound);
                return 1;
            }

            DownloadUncachedImplementations();

            Policy.Handler.CancellationToken.ThrowIfCancellationRequested();
            return LaunchImplementation();
        }
        #endregion
    }
}
