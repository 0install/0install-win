// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Downloads a set of <see cref="Implementation"/>s piped in as XML via stdin (for programmatic use). Use <see cref="Feed"/> format with no inner linebreaks and terminated by a single linebreak.
    /// </summary>
    public class Fetch : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "fetch";

        /// <inheritdoc/>
        public override string Description => "Downloads a set of implementations piped in as XML via stdin (for programmatic use). Use Feed format with no inner linebreaks and terminated by a single linebreak.";

        /// <inheritdoc/>
        public override string Usage => "";

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 0;
        #endregion

        /// <inheritdoc/>
        public Fetch([NotNull] ICommandHandler handler)
            : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) return ExitCode.InvalidData;

            var feedFragment = XmlStorage.FromXmlString<Feed>(input);
            Fetcher.Fetch(feedFragment.Implementations);

            return ExitCode.OK;
        }
    }
}
