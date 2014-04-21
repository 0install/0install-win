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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Cli;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services
{
    /// <summary>
    /// Uses the stderr stream to inform the user about the progress of tasks and ask the user questions.
    /// Provides hooks for specializition in derived implementations.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Diamond inheritance structure leads to false positive.")]
    public class CliHandler : CliTaskHandler, ICommandHandler
    {
        /// <inheritdoc/>
        public bool Batch { get; set; }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void SetGuiHints(Func<string> actionTitle, int delay)
        {}

        #region UI control
        /// <inheritdoc/>
        public void ShowProgressUI()
        {
            // Handle Ctrl+C
            try
            {
                Console.TreatControlCAsInput = false;
                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
                {
                    CancellationTokenSource.Cancel();
                    e.Cancel = true;
                };
            }
            catch (IOException)
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

        #region Question
        /// <inheritdoc/>
        public bool AskQuestion(string question, string batchInformation = null)
        {
            if (Batch)
            {
                if (!string.IsNullOrEmpty(batchInformation)) Log.Warn(batchInformation);
                return false;
            }

            Log.Info(question);

            // Loop until the user has made a valid choice
            while (true)
            {
                string input = CliUtils.ReadString("[Y/N]");
                if (input == null) throw new OperationCanceledException();
                switch (input.ToLower())
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
        public virtual void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            // Stub to be overriden
        }

        /// <inheritdoc/>
        public virtual void ModifySelections(Func<Selections> solveCallback)
        {
            throw new NeedGuiException(Resources.NoModifySelectionsInCli + (WindowsUtils.IsWindows ? "\n" + Resources.Try0installWin : ""));
        }
        #endregion

        #region Messages
        /// <inheritdoc/>
        public void Output(string title, string information)
        {
            Console.WriteLine(information);
        }
        #endregion

        #region Dialogs
        /// <inheritdoc/>
        public virtual void ShowIntegrateApp(IntegrationState state)
        {
            throw new NeedGuiException(Resources.IntegrateAppUseGui);
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

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            throw new NeedGuiException();
        }
        #endregion
    }
}
