// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using JetBrains.Annotations;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.CliCommands
{
    partial class CatalogMan
    {
        private class Remove : CatalogSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "remove";

            public override string Description => Resources.DescriptionCatalogRemove;

            public override string Usage => "URI";

            protected override int AdditionalArgsMin => 1;

            protected override int AdditionalArgsMax => 1;

            public Remove([NotNull] ICommandHandler handler)
                : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var uri = new FeedUri(AdditionalArgs[0]);

                if (CatalogManager.RemoveSource(uri))
                    return ExitCode.OK;
                else
                {
                    Handler.OutputLow(Resources.CatalogSources, string.Format(Resources.CatalogNotRegistered, uri.ToStringRfc()));
                    return ExitCode.NoChanges;
                }
            }
        }
    }
}
