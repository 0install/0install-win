// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using JetBrains.Annotations;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.CliCommands
{
    partial class CatalogMan
    {
        private class Add : CatalogSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "add";

            public override string Description => Resources.DescriptionCatalogAdd;

            public override string Usage => "URI";

            protected override int AdditionalArgsMin => 1;

            protected override int AdditionalArgsMax => 1;
            #endregion

            #region State
            private bool _skipVerify;

            public Add([NotNull] ICommandHandler handler)
                : base(handler)
            {
                Options.Add("skip-verify", () => Resources.OptionCatalogAddSkipVerify, _ => _skipVerify = true);
            }
            #endregion

            public override ExitCode Execute()
            {
                var uri = new FeedUri(AdditionalArgs[0]);
                if (!_skipVerify) CatalogManager.DownloadCatalog(uri);

                if (CatalogManager.AddSource(uri))
                {
                    if (!_skipVerify) CatalogManager.GetOnlineSafe();
                    return ExitCode.OK;
                }
                else
                {
                    Handler.OutputLow(Resources.CatalogSources, string.Format(Resources.CatalogAlreadyRegistered, uri.ToStringRfc()));
                    return ExitCode.NoChanges;
                }
            }
        }
    }
}
