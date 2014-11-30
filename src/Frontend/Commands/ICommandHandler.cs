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

using System;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Callback methods to allow users to interact with <see cref="FrontendCommand"/>s.
    /// </summary>
    /// <remarks>The methods may be called from a background thread. Implementations apply appropriate thread-synchronization to update UI elements.</remarks>
    public interface ICommandHandler : ITaskHandler
    {
        /// <summary>
        /// Disables any persistent UI elements that were created but still leaves them visible.
        /// </summary>
        void DisableUI();

        /// <summary>
        /// Closes any persistent UI elements that were created.
        /// </summary>
        void CloseUI();

        /// <summary>
        /// Shows the user the <see cref="Selections"/> made by the solver.
        /// Returns immediately. Will be ignored by non-GUI intefaces.
        /// </summary>
        /// <param name="selections">The <see cref="Selections"/> as provided by the solver.</param>
        /// <param name="feedCache">The feed cache used to retrieve feeds for additional information about implementations.</param>
        void ShowSelections(Selections selections, IFeedCache feedCache);

        /// <summary>
        /// Allows the user to modify the interface preferences and rerun the solver if desired.
        /// Returns once the user is satisfied with her choice. Will be ignored by non-GUI intefaces.
        /// </summary>
        /// <param name="solveCallback">Called after interface preferences have been changed and the solver needs to be rerun.</param>
        void ModifySelections(Func<Selections> solveCallback);

        /// <summary>
        /// Displays application integration options to the user.
        /// </summary>
        /// <param name="state">A View-Model for modifying the current desktop integration state.</param>
        /// <exception cref="OperationCanceledException">The user does not want any changes to be applied.</exception>
        /// <remarks>The caller is responsible for saving any changes.</remarks>
        void ShowIntegrateApp(IntegrationState state);

        /// <summary>
        /// Displays the results of a feed search to the user.
        /// </summary>
        /// <param name="query">The search query that was performed.</param>
        void ShowFeedSearch(SearchQuery query);

        /// <summary>
        /// Displays the configuration settings to the user.
        /// </summary>
        /// <param name="config">The configuration to show.</param>
        /// <param name="configTab">Switch to a specific tab in the configuration GUI. Has no effect in text-mode.</param>
        /// <exception cref="OperationCanceledException">The user does not want any changes to be applied.</exception>
        /// <remarks>The caller is responsible for saving any changes.</remarks>
        void ShowConfig(Config config, ConfigTab configTab);

        /// <summary>
        /// Displays a user interface for managing <see cref="IStore"/>s.
        /// </summary>
        /// <param name="store">The <see cref="IStore"/> to manage.</param>
        /// <param name="feedCache">Information about implementations found in the <paramref name="store"/> are extracted from here.</param>
        void ManageStore(IStore store, IFeedCache feedCache);
    }
}
