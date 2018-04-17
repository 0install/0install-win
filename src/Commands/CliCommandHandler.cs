// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Diagnostics.CodeAnalysis;
using NanoByte.Common;
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
        public bool Background { get => false; set {} }

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
        public void CustomizeSelections(Func<Selections> solveCallback) => throw new NeedGuiException(Resources.NoCustomizeSelectionsInCli);

        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state) => throw new NeedGuiException(Resources.IntegrateAppUseGui);

        /// <inheritdoc/>
        public void ShowFeedSearch(SearchQuery query)
        {
            #region Sanity checks
            if (query == null) throw new ArgumentNullException(nameof(query));
            #endregion

            Output(query.ToString(), query.Results);
        }

        /// <inheritdoc/>
        public void ShowConfig(Config config, ConfigTab configTab)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException(nameof(config));
            #endregion

            Console.Write(config.ToString());
        }

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache) => throw new NeedGuiException();
    }
}
