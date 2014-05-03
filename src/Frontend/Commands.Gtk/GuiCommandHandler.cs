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
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using CancellationToken = NanoByte.Common.Tasks.CancellationToken;
using CancellationTokenSource = NanoByte.Common.Tasks.CancellationTokenSource;

namespace ZeroInstall.Commands.Gtk
{
    /// <summary>
    /// Uses <see cref="Gtk"/> to allow users to interact with <see cref="FrontendCommand"/>s.
    /// </summary>
    /// <remarks>This class manages a GUI thread with an independent message queue. Invoking methods on the right thread is handled automatically.</remarks>
    public sealed class GuiCommandHandler : ICommandHandler
    {
        #region Properties
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return _cancellationTokenSource.Token; } }

        /// <inheritdoc/>
        public int Verbosity { get; set; }

        /// <inheritdoc/>
        public bool Batch { get; set; }

        private string _actionTitle;

        /// <inheritdoc/>
        public void SetGuiHints(LinqBridge::System.Func<string> actionTitle, int delay)
        {
            #region Sanity checks
            if (actionTitle == null) throw new ArgumentNullException("actionTitle");
            #endregion

            _actionTitle = actionTitle();
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new GUI handler with an external <see cref="CancellationTokenSource"/>.
        /// </summary>
        public GuiCommandHandler(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// Creates a new GUI handler with its own <see cref="CancellationTokenSource"/>.
        /// </summary>
        public GuiCommandHandler()
            : this(new CancellationTokenSource())
        {}
        #endregion

        #region Dispose
        /// <inheritdoc/>
        public void Dispose()
        {
            // TODO: Implement
        }
        #endregion

        //--------------------//

        #region Task tracking
        /// <summary>Synchronization object used to prevent multiple concurrent generic <see cref="ITask"/>s.</summary>
        private readonly object _genericTaskLock = new object();

        /// <inheritdoc/>
        public void RunTask(ITask task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            IProgress<TaskSnapshot> progress = null;
            if (task.Tag is ManifestDigest)
            {
                // TODO: Implement
            }
            else
            {
                lock (_genericTaskLock) // Prevent multiple concurrent generic tasks
                {
                    // TODO: Implement
                }
            }

            task.Run(CancellationToken, progress);
        }
        #endregion

        #region UI control
        /// <inheritdoc/>
        public void ShowProgressUI()
        {
            // TODO: Implement spawning GUI thread
        }

        /// <inheritdoc/>
        public void DisableProgressUI()
        {
            // TODO: Implement stopping GUI thread
        }

        /// <inheritdoc/>
        public void CloseProgressUI()
        {
            // TODO: Implement
        }
        #endregion

        #region Question
        /// <inheritdoc/>
        public bool AskQuestion(string question, string batchInformation = null)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(question)) throw new ArgumentNullException("question");
            #endregion

            // TODO: Implement
            return false;
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

        /// <summary>A wait handle used by <see cref="ModifySelections"/> to be signaled once the user is satisfied with the <see cref="Selections"/>.</summary>
        private readonly AutoResetEvent _modifySelectionsWaitHandle = new AutoResetEvent(false);

        /// <inheritdoc/>
        public void ModifySelections(LinqBridge::System.Func<Selections> solveCallback)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            #endregion

            // TODO: Implement

            _modifySelectionsWaitHandle.WaitOne();
        }
        #endregion

        #region Messages
        /// <inheritdoc/>
        public void Output(string title, string information)
        {
            DisableProgressUI();
            // TODO: Implement
        }
        #endregion

        #region Dialogs
        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state)
        {
            #region Sanity checks
            if (state == null) throw new ArgumentNullException("state");
            #endregion

            // TODO: Implement
        }

        /// <inheritdoc/>
        public bool ShowConfig(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            // TODO: Implement
            return false;
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
