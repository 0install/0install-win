// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections;
using System.Collections.Generic;
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// A minimalistic <see cref="ICommandHandler"/> that allows you to pre-record answers and retrieve output.
    /// </summary>
    public class MockCommandHandler : TaskHandlerBase, ICommandHandler
    {
        /// <summary>
        /// Always returns <c>false</c>.
        /// </summary>
        public bool Background { get => false; set {} }

        /// <inheritdoc/>
        public void DisableUI()
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void CloseUI()
        {
            // No UI, so nothing to do
        }

        /// <summary>
        /// The prerecorded result for <see cref="Ask"/>.
        /// </summary>
        public bool AnswerQuestionWith { get; set; }

        /// <summary>
        /// Last question passed to <see cref="Ask"/>.
        /// </summary>
        public string LastQuestion { get; private set; }

        protected override void LogHandler(LogSeverity severity, string message) {}

        /// <summary>
        /// Fakes asking the user a question.
        /// </summary>
        /// <returns>The current value of <see cref="AnswerQuestionWith"/>.</returns>
        protected override bool Ask(string question, MsgSeverity severity)
        {
            LastQuestion = question;
            return AnswerQuestionWith;
        }

        /// <summary>
        /// Last <see cref="Selections"/> passed to <see cref="ShowSelections"/>.
        /// </summary>
        public Selections LastSelections { get; private set; }

        /// <summary>
        /// Fakes showing <see cref="Selections"/> to the user.
        /// </summary>
        public void ShowSelections(Selections selections, IFeedManager feedManager) => LastSelections = selections;

        /// <inheritdoc/>
        public void CustomizeSelections(Func<Selections> solveCallback)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state)
        {
            // No UI, so nothing to do
        }

        public void ShowFeedSearch(SearchQuery query)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ShowConfig(Config config, ConfigTab configTab)
        {
            // No UI, so nothing to do
        }

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            // No UI, so nothing to do
        }

        /// <summary>
        /// Last information string passed to <see cref="Output"/>.
        /// </summary>
        public string LastOutput { get; private set; }

        /// <summary>
        /// Fakes showing an information string output to the user.
        /// </summary>
        public override void Output(string title, string message) => LastOutput = message;

        /// <summary>
        /// Last data objects passed to <see cref="Output{T}"/>.
        /// </summary>
        public IEnumerable LastOutputObjects { get; private set; }

        /// <summary>
        /// Fakes showing tabular data to the user.
        /// </summary>
        public override void Output<T>(string title, IEnumerable<T> data) => LastOutputObjects = data;

        public override void Error(Exception exception) {}

        public override ICredentialProvider CredentialProvider => null;
    }
}
