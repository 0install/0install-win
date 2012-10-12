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

using Common;
using Common.Tasks;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Ignores progress reports and silently answer all questions with "No".
    /// </summary>
    public class SilentHandler : SilentTaskHandler, IHandler
    {
        /// <summary>
        /// Always returns <see langword="true"/>.
        /// </summary>
        public bool Batch { get { return true; } set { } }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void SetGuiHints(string actionTitle, int delay)
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

        /// <inheritdoc />
        public virtual bool AskQuestion(string question, string batchInformation)
        {
            return false;
        }

        /// <inheritdoc/>
        public virtual void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void AuditSelections(SimpleResult<Selections> solveCallback)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc />
        public virtual void Output(string title, string information)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ShowIntegrateApp(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public bool ShowConfig(Config config)
        {
            // No UI, so nothing to do
            return false;
        }
    }
}
