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
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Common.Controls;
using Common.Tasks;
using ZeroInstall.Central.Properties;
using ZeroInstall.Commands;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Central.WinForms.Wizards
{
    /// <summary>
    /// Base class for <see cref="Wizard"/> pages that need a <see cref="SyncApps"/>.
    /// </summary>
    internal partial class SyncPage : UserControl, IBackendHandler
    {
        protected readonly bool MachineWide;

        public SyncPage()
        {
            InitializeComponent();
            labelWorking.Text = Resources.Working;
        }

        public SyncPage(bool machineWide) : this()
        {
            MachineWide = machineWide;
        }

        #region Create sync
        /// <summary>
        /// Creates a new <see cref="SyncIntegrationManager"/> using the default configuration.
        /// </summary>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <returns>A new <see cref="SyncIntegrationManager"/> instance.</returns>
        protected SyncIntegrationManager CreateSync(bool machineWide)
        {
            var locator = new ServiceLocator(this);
            return new SyncIntegrationManager(locator.Config.ToSyncServer(), locator.Config.SyncCryptoKey, locator.FeedManager.GetFeedFresh, locator.Handler, machineWide);
        }

        /// <summary>
        /// Creates a new <see cref="SyncIntegrationManager"/> using a custom crypto key.
        /// </summary>
        /// <param name="cryptoKey">The crypto key to use; overrides <see cref="Config.SyncCryptoKey"/>.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <returns>A new <see cref="SyncIntegrationManager"/> instance.</returns>
        protected SyncIntegrationManager CreateSync(string cryptoKey, bool machineWide)
        {
            var locator = new ServiceLocator(this);
            return new SyncIntegrationManager(locator.Config.ToSyncServer(), cryptoKey, locator.FeedManager.GetFeedFresh, locator.Handler, machineWide);
        }

        /// <summary>
        /// Creates a new <see cref="SyncIntegrationManager"/> using a custom server and credentials.
        /// </summary>
        /// <param name="server">Access information for the sync server; overrides <see cref="Config"/>.</param>
        /// <param name="cryptoKey">The crypto key to use; overrides <see cref="Config.SyncCryptoKey"/>; overrides <see cref="Config.SyncCryptoKey"/>.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <returns>A new <see cref="SyncIntegrationManager"/> instance.</returns>
        protected SyncIntegrationManager CreateSync(SyncServer server, string cryptoKey, bool machineWide)
        {
            var locator = new ServiceLocator(this);
            return new SyncIntegrationManager(server, cryptoKey, locator.FeedManager.GetFeedFresh, locator.Handler, machineWide);
        }
        #endregion

        #region IBackendHandler
        private readonly CancellationToken _cancellationToken = new CancellationToken();

        /// <summary>
        /// Should never be signaled.
        /// </summary>
        public CancellationToken CancellationToken { get { return _cancellationToken; } }

        [MethodImpl(MethodImplOptions.Synchronized)] // Prevent multiple concurrent tasks
        public void RunTask(ITask task, object tag = null)
        {
            Invoke(new Action(() => TrackingDialog.Run(this, task)));
        }

        /// <summary>
        /// Always returns 1.
        /// </summary>
        public int Verbosity { get { return 1; } set { } }

        /// <summary>
        /// Always returns <see langword="true"/>.
        /// </summary>
        public bool Batch { get { return true; } set { } }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void SetGuiHints(Func<string> actionTitle, int delay)
        {}

        public void ShowProgressUI()
        {
            Invoke(new Action(() => labelWorking.Visible = true));
        }

        public void DisableProgressUI()
        {}

        public void CloseProgressUI()
        {
            Invoke(new Action(() => labelWorking.Visible = false));
        }

        public bool AskQuestion(string question, string batchInformation = null)
        {
            throw new NotImplementedException();
        }

        public void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            throw new NotImplementedException();
        }

        public void ModifySelections(Func<Selections> solveCallback)
        {
            throw new NotImplementedException();
        }

        public void Output(string title, string information)
        {
            throw new NotImplementedException();
        }

        public void ShowIntegrateApp(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            throw new NotImplementedException();
        }

        public bool ShowConfig(Config config)
        {
            throw new NotImplementedException();
        }

        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
