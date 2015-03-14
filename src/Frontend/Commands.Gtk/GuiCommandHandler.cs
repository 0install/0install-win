/*
 * Copyright 2010-2014 Bastian Eicher
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

extern alias LinqBridge;
using System;
using System.Threading;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.Gtk
{
    /// <summary>
    /// Uses <see cref="Gtk"/> to allow users to interact with <see cref="CliCommand"/>s.
    /// </summary>
    /// <remarks>This class manages a GUI thread with an independent message queue. Invoking methods on the right thread is handled automatically.</remarks>
    public sealed class GuiCommandHandler : GuiTaskHandler, ICommandHandler
    {
        /// <inheritdoc/>
        public bool Background { get; set; }

        #region UI control
        /// <inheritdoc/>
        public void DisableUI()
        {
            // TODO: Implement stopping GUI thread
        }

        /// <inheritdoc/>
        public void CloseUI()
        {
            // TODO: Implement
        }
        #endregion

        #region Selections UI
        /// <inheritdoc/>
        public void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            // TODO: Implement
        }

        /// <summary>A wait handle used by <see cref="CustomizeSelections"/> to be signaled once the user is satisfied with the <see cref="Selections"/>.</summary>
        private readonly AutoResetEvent _modifySelectionsWaitHandle = new AutoResetEvent(false);

        /// <inheritdoc/>
        public void CustomizeSelections(LinqBridge::System.Func<Selections> solveCallback)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            #endregion

            // TODO: Implement

            _modifySelectionsWaitHandle.WaitOne();
        }
        #endregion

        #region Dialogs
        /// <inheritdoc/>
        public void ShowFeedSearch(SearchQuery query)
        {
            #region Sanity checks
            if (query == null) throw new ArgumentNullException("query");
            #endregion

            Output(query.ToString(), query.Results);
        }

        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state)
        {
            #region Sanity checks
            if (state == null) throw new ArgumentNullException("state");
            #endregion

            // TODO: Implement
        }

        /// <inheritdoc/>
        public void ShowConfig(Config config, ConfigTab configTab)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            // TODO: Implement
        }

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            // TODO: Implement
        }
        #endregion
    }
}
