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
using System.IO;
using Common;
using Common.Cli;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Uses the stderr stream to inform the user about the progress of tasks and ask the user questions.
    /// Provides hooks for specializition in derived implementations.
    /// </summary>
    public class CliHandler : CliTaskHandler, IHandler
    {
        /// <inheritdoc />
        public bool Batch { get; set; }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void SetGuiHints(string actionTitle, int delay)
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
                    CancellationToken.RequestCancellation();
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
        /// <inheritdoc />
        public bool AskQuestion(string question, string batchInformation)
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
        public virtual void AuditSelections(Func<Selections> solveCallback)
        {
            throw new NeedGuiException(Resources.NoAuditInCli + (WindowsUtils.IsWindows ? "\n" + Resources.Try0installWin : ""));
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
        /// <inheritdoc />
        public virtual void ShowIntegrateApp(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
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
        #endregion
    }
}
