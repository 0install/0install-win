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
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

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
        public new const string Name = "sync";
        #endregion

        #region Variables
        private SyncResetMode _syncResetMode = SyncResetMode.None;
        private SyncIntegrationManager _syncManager;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionSync; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 0; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionSync; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public SyncApps(IBackendHandler handler) : base(handler)
        {
            Options.Add("reset=", () => Resources.OptionSyncReset, (SyncResetMode mode) => _syncResetMode = mode);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            Handler.ShowProgressUI();

            using (_syncManager = new SyncIntegrationManager(Config.ToSyncServer(), Config.SyncCryptoKey, feedID => FeedManager.GetFeed(feedID), Handler, MachineWide))
                Sync();

            return 0;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Performs the synchronization.
        /// </summary>
        private void Sync()
        {
            try
            {
                _syncManager.Sync(_syncResetMode);
            }
                #region Error handling
            catch
            {
                // Suppress any left-over errors if the user canceled anyway
                Handler.CancellationToken.ThrowIfCancellationRequested();
                throw;
            }
            #endregion
        }
        #endregion
    }
}
