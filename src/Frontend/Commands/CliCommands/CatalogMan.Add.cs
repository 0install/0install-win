/*
 * Copyright 2010-2015 Bastian Eicher
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

using JetBrains.Annotations;
using NanoByte.Common.Tasks;
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

            protected override string Description { get { return Resources.DescriptionCatalogAdd; } }

            protected override string Usage { get { return "URI"; } }

            protected override int AdditionalArgsMin { get { return 1; } }

            protected override int AdditionalArgsMax { get { return 1; } }
            #endregion

            #region State
            private bool _skipVerify;

            public Add([NotNull] ICommandHandler handler) : base(handler)
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