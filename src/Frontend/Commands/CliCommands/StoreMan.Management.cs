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

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.CliCommands
{
    partial class StoreMan
    {
        private class Manage : StoreSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "manage";

            protected override string Description => Resources.DescriptionStoreManage;

            protected override string Usage => "";

            protected override int AdditionalArgsMax => 0;

            public Manage([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                Handler.ManageStore(Store, FeedCache);
                return ExitCode.OK;
            }
        }

        internal class ListImplementations : StoreSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "list-implementations";

            protected override string Description => Resources.DescriptionStoreListImplementations;

            protected override string Usage => "[FEED-URI]";

            protected override int AdditionalArgsMax => 1;

            public ListImplementations([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var nodeBuilder = new CacheNodeBuilder(Store, FeedCache);
                nodeBuilder.Run();

                if (AdditionalArgs.Count == 1)
                {
                    var uri = GetCanonicalUri(AdditionalArgs[0]);
                    if (uri.IsFile && !File.Exists(uri.LocalPath))
                        throw new FileNotFoundException(string.Format(Resources.FileOrDirNotFound, uri.LocalPath), uri.LocalPath);

                    var nodes = nodeBuilder.Nodes.OfType<OwnedImplementationNode>().Where(x => x.FeedUri == uri);
                    Handler.Output(Resources.CachedImplementations, nodes);
                }
                else
                {
                    var nodes = nodeBuilder.Nodes.OfType<ImplementationNode>();
                    Handler.Output(Resources.CachedImplementations, nodes);
                }

                return ExitCode.OK;
            }
        }

        internal class Audit : StoreSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "audit";

            protected override string Description => Resources.DescriptionStoreAudit;

            protected override string Usage => "[CACHE-DIR+]";

            public Audit([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var store = GetEffectiveStore();
                foreach (var manifestDigest in store.ListAll())
                    store.Verify(manifestDigest, Handler);
                return ExitCode.OK;
            }
        }

        internal class Optimise : StoreSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "optimise";

            public const string AltName = "optimize";

            protected override string Description => Resources.DescriptionStoreOptimise;

            protected override string Usage => "[CACHE-DIR+]";

            public Optimise([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                long savedBytes = GetEffectiveStore().Optimise(Handler);
                Handler.OutputLow(Resources.OptimiseComplete, string.Format(Resources.StorageReclaimed, savedBytes.FormatBytes(CultureInfo.CurrentCulture)));
                return ExitCode.OK;
            }
        }

        internal class Purge : StoreSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "purge";

            protected override string Description => Resources.DescriptionStorePurge;

            protected override string Usage => "[CACHE-DIR+]";

            public Purge([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                if (Handler.Ask(Resources.ConfirmPurge, defaultAnswer: true))
                {
                    GetEffectiveStore().Purge(Handler);
                    return ExitCode.OK;
                }
                else throw new OperationCanceledException();
            }
        }
    }
}
