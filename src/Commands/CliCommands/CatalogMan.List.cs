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
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.CliCommands
{
    partial class CatalogMan
    {
        private class List : CatalogSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "list";

            public override string Description => Resources.DescriptionCatalogList;

            public override string Usage => "";

            protected override int AdditionalArgsMax => 0;

            public List([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                Handler.Output(Resources.CatalogSources, Services.Feeds.CatalogManager.GetSources());
                return ExitCode.OK;
            }
        }
    }
}