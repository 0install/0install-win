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

using System;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// View or change <see cref="Config"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class SyncApps : IntegrationCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "sync";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionSync; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionSync; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public SyncApps(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);

            if (AdditionalArgs.Count > 0) throw new OptionException(Resources.TooManyArguments, "");

            Policy.Handler.ShowProgressUI(Cancel);

            var syncManager = new SyncIntegrationManager(SystemWide, Policy.Config.SyncServer, Policy.Config.SyncServerUsername, Policy.Config.SyncServerPassword, Policy.Config.SyncCryptoKey);
            Converter<string, Feed> feedRetreiver = delegate(string interfaceID)
            {
                CacheFeed(interfaceID);
                bool stale;
                return Policy.FeedManager.GetFeed(interfaceID, Policy, out stale);
            };
            syncManager.Sync(feedRetreiver, Policy.Handler);

            // Show a "sync complete" message (but not in batch mode, since it is too unimportant)
            if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.DesktopIntegration, Resources.DesktopIntegrationSyncDone);
            return 0;
        }
        #endregion
    }
}
