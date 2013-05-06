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
using System.Windows.Forms;
using Common.Controls;
using Common.Tasks;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Central.WinForms.SyncWizards
{
    /// <summary>
    /// Base class for <see cref="Wizard"/> pages that implemented a rudimentary <see cref="IHandler"/> without cancellation support.
    /// </summary>
    internal partial class HandlerPage : UserControl, IHandler
    {
        protected readonly bool MachineWide;

        public HandlerPage()
        {}

        public HandlerPage(bool machineWide)
        {
            MachineWide = machineWide;

            InitializeComponent();
            labelWorking.Text = Resources.Working;
        }

        #region ITaskHandler
        private readonly CancellationToken _cancellationToken = new CancellationToken();

        /// <summary>
        /// Should never be signaled.
        /// </summary>
        public CancellationToken CancellationToken { get { return _cancellationToken; } }

        private readonly object _taskLock = new object();

        public void RunTask(ITask task, object tag)
        {
            lock (_taskLock) // Prevent multiple concurrent tasks
            {
                Invoke(new Action(() => TrackingDialog.Run(this, task, null)));
            }
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
        public void SetGuiHints(string actionTitle, int delay)
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

        public bool AskQuestion(string question, string batchInformation)
        {
            throw new NotImplementedException();
        }

        public void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            throw new NotImplementedException();
        }

        public void AuditSelections(Func<Selections> solveCallback)
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
        #endregion
    }
}
