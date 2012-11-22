/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Threading;
using Common.Tasks;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Wraps a <see cref="GuiHandler"/> and displays it only after a certain delay (or immediately when it is required).
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposal is handled sufficiently by GC in this case")]
    public class DelayedGuiHandler : MarshalByRefObject, IHandler
    {
        #region Variables
        /// <summary>The actual GUI to show with a delay.</summary>
        private GuiHandler _target;

        /// <summary>The number of milliseconds by which to delay the initial display of the GUI.</summary>
        private int _delay;

        /// <summary>Synchronization object used to prevent concurrent access to <see cref="_target"/>.</summary>
        private readonly object _targetLock = new object();

        /// <summary>A wait handle used to signal that <see cref="InitTarget"/> no longer needs to be called.</summary>
        private readonly AutoResetEvent _uiDone = new AutoResetEvent(false);

        /// <summary>Queues defered actions to be executed as soon as the <see cref="_target"/> is created.</summary>
        private Action<GuiHandler> _onTargetCreate;
        #endregion

        #region Properties
        private readonly CancellationToken _cancellationToken = new CancellationToken();

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return _cancellationToken; } }
        #endregion

        //--------------------//

        #region Target access
        /// <summary>
        /// Initializes the <see cref="_target"/> if it is missing (thread-safe) and returns it.
        /// </summary>
        private GuiHandler InitTarget()
        {
            // Thread-safe singleton with double-check
            if (_target != null) return _target;
            lock (_targetLock)
            {
                if (_target != null) return _target;
                _uiDone.Set();

                // Create target but keep it hidden until all defered actions are complete (ensures correct order)
                var newTarget = new GuiHandler(_cancellationToken);
                if (_onTargetCreate != null) _onTargetCreate(newTarget);
                return _target = newTarget;
            }
        }

        /// <summary>
        /// Applies an action to the <see cref="_target"/> as soon as it is created
        /// </summary>
        private void ApplyToTarget(Action<GuiHandler> action)
        {
            // Thread-safe singleton with double-check
            if (_target != null) action(_target);
            lock (_targetLock)
            {
                if (_target != null) action(_target);
                else _onTargetCreate += action;
            }
        }
        #endregion

        #region UI control
        /// <inheritdoc/>
        public void ShowProgressUI()
        {
            _onTargetCreate += target => target.ShowProgressUI();

            if (_delay == 0) InitTarget();
            else
            {
                new Thread(() =>
                {
                    if (!_uiDone.WaitOne(_delay))
                        InitTarget();
                }).Start();
            }
        }

        /// <inheritdoc/>
        public void CloseProgressUI()
        {
            _uiDone.Set();
            ApplyToTarget(target => target.CloseProgressUI());
        }
        #endregion

        #region Pass-through
        // Keep local read cache, assume no inner changes
        private bool _batch;

        /// <inheritdoc />
        public bool Batch
        {
            get { return _batch; }
            set
            {
                _batch = value;
                ApplyToTarget(target => target.Batch = value);
            }
        }

        /// <inheritdoc />
        public void SetGuiHints(string actionTitle, int delay)
        {
            _delay = delay;
            ApplyToTarget(target => target.SetGuiHints(actionTitle, delay));
        }

        /// <inheritdoc />
        public void RunTask(ITask task, object tag)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            InitTarget().RunTask(task, tag);
        }

        /// <inheritdoc/>
        public void DisableProgressUI()
        {
            ApplyToTarget(target => target.DisableProgressUI());
        }

        /// <inheritdoc />
        public bool AskQuestion(string question, string batchInformation)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(question)) throw new ArgumentNullException("question");
            #endregion

            return InitTarget().AskQuestion(question, batchInformation);
        }

        /// <inheritdoc/>
        public void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            ApplyToTarget(target => target.ShowSelections(selections, feedCache));
        }

        /// <inheritdoc/>
        public void AuditSelections(Func<Selections> solveCallback)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            #endregion

            InitTarget().AuditSelections(solveCallback);
        }

        /// <inheritdoc />
        public void Output(string title, string information)
        {
            InitTarget().Output(title, information);
        }

        /// <inheritdoc/>
        public void ShowIntegrateApp(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            InitTarget().ShowIntegrateApp(integrationManager, appEntry, feed);
        }

        /// <inheritdoc/>
        public bool ShowConfig(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            return InitTarget().ShowConfig(config);
        }
        #endregion
    }
}
