// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.CliCommands
{
    partial class CatalogMan
    {
        private class Search : CatalogSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "search";

            public override string Description => Resources.DescriptionCatalogSearch;

            public override string Usage => "[QUERY]";

            public Search([NotNull] ICommandHandler handler)
                : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var catalog = GetCatalog();
                string query = AdditionalArgs.JoinEscapeArguments();

                Handler.Output(Resources.AppList, catalog.Search(query));
                return ExitCode.OK;
            }
        }
    }
}
