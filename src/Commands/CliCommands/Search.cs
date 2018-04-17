// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Searches for feeds indexed by the mirror server.
    /// </summary>
    public class Search : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "search";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionSearch;

        /// <inheritdoc/>
        public override string Usage => "QUERY";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => (Handler is CliCommandHandler) ? 1 : 0;
        #endregion

        /// <inheritdoc/>
        public Search([NotNull] ICommandHandler handler)
            : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            string keywords = StringUtils.Join(" ", AdditionalArgs);
            Handler.ShowFeedSearch(SearchQuery.Perform(Config, keywords));
            return ExitCode.OK;
        }
    }
}
