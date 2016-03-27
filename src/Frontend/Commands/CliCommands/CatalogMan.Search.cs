/*
 * Copyright 2010-2016 Bastian Eicher
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

            protected override string Description { get { return Resources.DescriptionCatalogSearch; } }

            protected override string Usage { get { return "[QUERY]"; } }

            public Search([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var catalog = CatalogManager.GetCached() ?? CatalogManager.GetOnline();
                string query = AdditionalArgs.JoinEscapeArguments();

                Handler.Output(Resources.AppList, catalog.Search(query));
                return ExitCode.OK;
            }
        }
    }
}