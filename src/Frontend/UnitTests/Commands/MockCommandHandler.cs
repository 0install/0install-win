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

extern alias LinqBridge;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// A minimalistic <see cref="ICommandHandler"/> that allows you to pre-record answers and retrieve output.
    /// </summary>
    public class MockCommandHandler : MockTaskHandler, ICommandHandler
    {
        /// <summary>
        /// Always returns <c>false</c>.
        /// </summary>
        public bool Background { get { return false; } set { } }

        /// <inheritdoc/>
        public void DisableUI()
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void CloseUI()
        {
            // No UI, so nothing to do
        }

        /// <summary>
        /// Last <see cref="Selections"/> passed to <see cref="ShowSelections"/>.
        /// </summary>
        public Selections LastSelections { get; private set; }

        /// <summary>
        /// Fakes showing <see cref="Selections"/> to the user.
        /// </summary>
        public void ShowSelections(Selections selections, IFeedManager feedManager)
        {
            LastSelections = selections;
        }

        /// <inheritdoc/>
        public void CustomizeSelections(LinqBridge::System.Func<Selections> solveCallback)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state)
        {
            // No UI, so nothing to do
        }

        public void ShowFeedSearch(SearchQuery query)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ShowConfig(Config config, ConfigTab configTab)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            // No UI, so nothing to do
        }
    }
}
