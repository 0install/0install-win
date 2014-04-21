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
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services
{
    /// <summary>
    /// Ignores progress reports and silently answer all questions with "No".
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Diamond inheritance structure leads to false positive.")]
    public class SilentHandler : SilentTaskHandler, ICommandHandler
    {
        /// <summary>
        /// Always returns <see langword="true"/>.
        /// </summary>
        public virtual bool Batch { get { return true; } set { } }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void SetGuiHints(Func<string> actionTitle, int delay)
        {}

        /// <inheritdoc/>
        public void ShowProgressUI()
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void DisableProgressUI()
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void CloseProgressUI()
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public virtual bool AskQuestion(string question, string batchInformation = null)
        {
            return false;
        }

        /// <inheritdoc/>
        public virtual void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ModifySelections(Func<Selections> solveCallback)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public virtual void Output(string title, string information)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public bool ShowConfig(Config config)
        {
            // No UI, so nothing to do
            return false;
        }

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            // No UI, so nothing to do
        }
    }
}
