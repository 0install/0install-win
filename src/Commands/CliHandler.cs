/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using Common;
using Common.Cli;
using Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Uses the stderr stream to inform the user about the progress of tasks and ask the user questions.
    /// </summary>
    public class CliHandler : CliTaskHandler, IHandler
    {
        #region Task tracking
        /// <summary>The currently running generic task (if any).</summary>
        private volatile ITask _genericTask;

        /// <inheritdoc/>
        public override void RunTask(ITask task, object tag)
        {
            // Track generic tasks when they start running
            if (tag == null) _genericTask = task;

            base.RunTask(task, tag);

            // Since the CliTaskHandler only runs one task at a time, this is safe
            _genericTask = null;
        }

        /// <inheritdoc />
        public bool Batch { get; set; }
        #endregion

        #region UI control
        /// <inheritdoc/>
        public void ShowProgressUI(SimpleEventHandler cancelCallback)
        {
            // Handle Ctrl+C
            try
            {
                Console.TreatControlCAsInput = false;
                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
                {
                    // Cancel generic tasks
                    var genericTask = _genericTask;
                    if (genericTask != null && genericTask.CanCancel) genericTask.Cancel();

                    cancelCallback();

                    e.Cancel = true;
                };
            }
            catch(IOException)
            {
                // Ignore failures caused by non-standard terminal emulators
            }
        }

        /// <inheritdoc/>
        public void DisableProgressUI()
        {
            // Console UI only, so nothing to do
        }

        /// <inheritdoc/>
        public void CloseProgressUI()
        {
            // Console UI only, so nothing to do
        }
        #endregion

        #region Key control
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            if (Batch) return false;

            Log.Info(information);

            // Loop until the user has made a valid choice
            while (true)
            {
                switch ((CliUtils.ReadString("Trust [Y/N] ") ?? "n").ToLower())
                {
                    case "y":
                    case "yes":
                        return true;
                    case "n":
                    case "no":
                        return false;
                }
            }
        }
        #endregion

        #region Selections UI
        /// <inheritdoc/>
        public void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            // Console UI only, so nothing to do
        }

        /// <inheritdoc/>
        public void AuditSelections(SimpleResult<Selections> solveCallback)
        {
            // Console UI only, so nothing to do
        }
        #endregion

        #region Messages
        /// <inheritdoc />
        public void Output(string title, string information)
        {
            Console.WriteLine(information);
        }
        #endregion

        #region Dialogs
        public void ShowIntegrateApp(IIntegrationManager integrationManager, InterfaceFeed target)
        {
            // ToDo: Implement text-based UI
            Output(Resources.DesktopIntegration, Resources.IntegrateAppUseGui);
        }

        /// <inheritdoc/>
        public bool ShowConfig(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            Console.Write(config.ToString());
            return false;
        }
        #endregion
    }
}
