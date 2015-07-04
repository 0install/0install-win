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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Manages the <see cref="Catalog"/>s provided by the <see cref="ICatalogManager"/>.
    /// </summary>
    public sealed class CatalogMan : MultiCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "catalog";

        /// <inheritdoc/>
        public CatalogMan([NotNull] ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        protected override IEnumerable<string> SubCommandNames { get { return new[] {Search.Name, Refresh.Name, Add.Name, Remove.Name, Reset.Name, List.Name}; } }

        /// <inheritdoc/>
        protected override SubCommand GetCommand(string commandName)
        {
            #region Sanity checks
            if (commandName == null) throw new ArgumentNullException("commandName");
            #endregion

            switch (commandName)
            {
                case Search.Name:
                    return new Search(Handler);
                case Refresh.Name:
                    return new Refresh(Handler);
                case Add.Name:
                    return new Add(Handler);
                case Remove.Name:
                    return new Remove(Handler);
                case Reset.Name:
                    return new Reset(Handler);
                case List.Name:
                    return new List(Handler);
                default:
                    throw new OptionException(string.Format(Resources.UnknownCommand, commandName), commandName);
            }
        }

        private abstract class CatalogSubCommand : SubCommand
        {
            protected override string ParentName { get { return CatalogMan.Name; } }

            protected CatalogSubCommand([NotNull] ICommandHandler handler) : base(handler)
            {}
        }

        // ReSharper disable MemberHidesStaticFromOuterClass

        private class Search : CatalogSubCommand
        {
            #region Metadata
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

        private class Refresh : CatalogSubCommand
        {
            #region Metadata
            public new const string Name = "refresh";

            protected override string Description { get { return Resources.DescriptionCatalogRefresh; } }

            protected override string Usage { get { return ""; } }

            protected override int AdditionalArgsMax { get { return 0; } }

            public Refresh([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                CatalogManager.GetOnline();
                return ExitCode.OK;
            }
        }

        private class Add : CatalogSubCommand
        {
            #region Metadata
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

        private class Remove : CatalogSubCommand
        {
            #region Metadata
            public new const string Name = "remove";

            protected override string Description { get { return Resources.DescriptionCatalogRemove; } }

            protected override string Usage { get { return "URI"; } }

            protected override int AdditionalArgsMin { get { return 1; } }

            protected override int AdditionalArgsMax { get { return 1; } }

            public Remove([NotNull] ICommandHandler handler) : base(handler)
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

        private class Reset : CatalogSubCommand
        {
            #region Metadata
            public new const string Name = "reset";

            protected override string Description { get { return Resources.DescriptionCatalogReset; } }

            protected override string Usage { get { return ""; } }

            protected override int AdditionalArgsMax { get { return 0; } }

            public Reset([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                Services.Feeds.CatalogManager.SetSources(new[] {Services.Feeds.CatalogManager.DefaultSource});
                CatalogManager.GetOnlineSafe();
                return ExitCode.OK;
            }
        }

        private class List : CatalogSubCommand
        {
            #region Metadata
            public new const string Name = "list";

            protected override string Description { get { return Resources.DescriptionCatalogList; } }

            protected override string Usage { get { return ""; } }

            protected override int AdditionalArgsMax { get { return 0; } }

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
