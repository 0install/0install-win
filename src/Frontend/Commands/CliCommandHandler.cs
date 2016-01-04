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
using System.Diagnostics.CodeAnalysis;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Uses the stdin/stderr streams to allow users to interact with <see cref="CliCommand"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Diamond inheritance structure leads to false positive")]
    public sealed class CliCommandHandler : CliTaskHandler, ICommandHandler
    {
        /// <summary>
        /// Always returns <c>false</c>.
        /// </summary>
        public bool Background { get { return false; } set { } }

        /// <inheritdoc/>
        public void DisableUI()
        {
            // Console UI only, so nothing to do
        }

        /// <inheritdoc/>
        public void CloseUI()
        {
            // Console UI only, so nothing to do
        }

        /// <inheritdoc/>
        public void ShowSelections(Selections selections, IFeedManager feedManager)
        {
            // Stub to be overriden
        }

        /// <inheritdoc/>
        public void CustomizeSelections(Func<Selections> solveCallback)
        {
            throw new NeedGuiException(Resources.NoModifySelectionsInCli + (WindowsUtils.IsWindows ? "\n" + Resources.Try0installWin : ""));
        }

        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state)
        {
            throw new NeedGuiException(Resources.IntegrateAppUseGui);
        }

        /// <inheritdoc/>
        public void ShowFeedSearch(SearchQuery query)
        {
            #region Sanity checks
            if (query == null) throw new ArgumentNullException("query");
            #endregion

            Output(query.ToString(), query.Results);
        }

        /// <inheritdoc/>
        public void ShowConfig(Config config, ConfigTab configTab)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            Console.Write(config.ToString());
        }

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            throw new NeedGuiException();
        }
    }
}
