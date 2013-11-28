/*
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
using System.Collections.Generic;
using System.IO;
using Common.Info;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

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
        public SelfUpdate(IBackendHandler handler) : base(handler)
        {
            NoWait = true;
            FeedManager.Refresh = true;
            Config.AllowApiHooking = false;
            Requirements.Command = "update";

            //Options.Remove("no-wait");
            //Options.Remove("refresh");

            Options.Add("force", () => Resources.OptionForceSelfUpdate, unused => _force = true);
        }
        #endregion

        //--------------------//

        #region Parse
        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            if (Options.Parse(args).Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs.JoinEscapeArguments(), "");

            Requirements.InterfaceID = Config.SelfUpdateID;

            // Always pass in the installation directory to the updater as an argument
            AdditionalArgs.Add(Locations.InstallBase);
        }
        #endregion

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (File.Exists(Path.Combine(Locations.PortableBase, "_no_self_update_check"))) throw new NotSupportedException(Resources.NoSelfUpdateDisabled);
            if (StoreUtils.PathInAStore(Locations.InstallBase)) throw new NotSupportedException(Resources.NoSelfUpdateStore);

            Handler.ShowProgressUI();

            Solve();
            ShowSelections();

            // Make sure the update is actually a newer version
            var currentVersion = new ImplementationVersion(AppInfo.Current.Version);
            var newVersion = Selections.Implementations[0].Version;
            if (!_force && currentVersion >= newVersion)
            {
                Handler.OutputLow(Resources.ChangesFound, Resources.NoUpdatesFound);
                return 1;
            }

            DownloadUncachedImplementations();

            Handler.CancellationToken.ThrowIfCancellationRequested();
            return LaunchImplementation();
        }
        #endregion
    }
}
