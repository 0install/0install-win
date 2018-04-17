// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using JetBrains.Annotations;
using NanoByte.Common.Cli;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Import a feed from a local file, as if it had been downloaded from the network.
    /// </summary>
    /// <remarks>This is useful when testing a feed file, to avoid uploading it to a remote server in order to download it again. The file must have a trusted digital signature, as when fetching from the network.</remarks>
    public sealed class Import : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "import";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionImport;

        /// <inheritdoc/>
        public override string Usage => "FEED-FILE [...]";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;
        #endregion

        /// <inheritdoc/>
        public Import([NotNull] ICommandHandler handler)
            : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            foreach (var file in ArgumentUtils.GetFiles(AdditionalArgs, "*.xml"))
                FeedManager.ImportFeed(file.FullName);

            return ExitCode.OK;
        }
    }
}
