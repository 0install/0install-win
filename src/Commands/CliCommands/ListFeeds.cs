// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Linq;
using JetBrains.Annotations;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// List all known feed URIs for a specific interface.
    /// </summary>
    public sealed class ListFeeds : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "list-feeds";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionListFeeds;

        /// <inheritdoc/>
        public override string Usage => "[OPTIONS] URI";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 1;
        #endregion

        /// <inheritdoc/>
        public ListFeeds([NotNull] ICommandHandler handler)
            : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            var interfaceUri = GetCanonicalUri(AdditionalArgs[0]);
            var preferences = InterfacePreferences.LoadFor(interfaceUri);

            Handler.Output(
                string.Format(Resources.FeedsRegistered, interfaceUri),
                preferences.Feeds.Select(x => x.Source));
            return ExitCode.OK;
        }
    }
}
