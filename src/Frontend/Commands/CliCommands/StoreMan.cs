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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Manages the contents of the <see cref="IStore"/>s.
    /// </summary>
    public sealed class StoreMan : MultiCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "store";

        /// <inheritdoc/>
        public StoreMan([NotNull] ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        protected override IEnumerable<string> SubCommandNames { get { return new[] {Add.Name, Audit.Name, Copy.Name, Find.Name, List.Name, ListImplementations.Name, Manage.Name, Optimise.Name, Purge.Name, Remove.Name, Verify.Name}; } }

        /// <inheritdoc/>
        protected override SubCommand GetCommand(string commandName)
        {
            #region Sanity checks
            if (commandName == null) throw new ArgumentNullException("commandName");
            #endregion

            switch (commandName)
            {
                case Add.Name:
                    return new Add(Handler);
                case Audit.Name:
                    return new Audit(Handler);
                case Copy.Name:
                    return new Copy(Handler);
                case Find.Name:
                    return new Find(Handler);
                case List.Name:
                    return new List(Handler);
                case ListImplementations.Name:
                    return new ListImplementations(Handler);
                case Manage.Name:
                    return new Manage(Handler);
                case "manifest":
                    throw new NotSupportedException(string.Format(Resources.UseInstead, "0install digest --manifest"));
                case Optimise.Name:
                case Optimise.AltName:
                    return new Optimise(Handler);
                case Purge.Name:
                    return new Purge(Handler);
                case Remove.Name:
                    return new Remove(Handler);
                case Verify.Name:
                    return new Verify(Handler);
                default:
                    throw new OptionException(Resources.UnknownOption, commandName);
            }
        }

        internal abstract class StoreSubCommand : SubCommand
        {
            protected override string ParentName { get { return StoreMan.Name; } }

            protected StoreSubCommand([NotNull] ICommandHandler handler) : base(handler)
            {}

            /// <summary>
            /// Returns the default <see cref="IStore"/> or a <see cref="CompositeStore"/> as specifief by the <see cref="CliCommand.AdditionalArgs"/>.
            /// </summary>
            protected IStore GetEffectiveStore()
            {
                return (AdditionalArgs.Count == 0)
                    ? Store
                    : new CompositeStore(AdditionalArgs.Select(arg => (IStore)new DirectoryStore(arg)));
            }
        }

        // ReSharper disable MemberHidesStaticFromOuterClass

        internal class Add : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "add";

            protected override string Description { get { return Resources.DescriptionStoreAdd; } }

            protected override string Usage { get { return "DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))"; } }

            protected override int AdditionalArgsMin { get { return 2; } }

            public Add([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var manifestDigest = new ManifestDigest(AdditionalArgs[0]);
                string path = AdditionalArgs[1];
                try
                {
                    if (File.Exists(path))
                    { // One or more archives (combined/overlayed)
                        Store.AddArchives(GetArchiveFileInfos(), manifestDigest, Handler);
                        return ExitCode.OK;
                    }
                    else if (Directory.Exists(path))
                    { // A single directory
                        if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))", AdditionalArgs[3]);
                        Store.AddDirectory(Path.GetFullPath(path), manifestDigest, Handler);
                        return ExitCode.OK;
                    }
                    else throw new FileNotFoundException(string.Format(Resources.FileOrDirNotFound, path), path);
                }
                catch (ImplementationAlreadyInStoreException ex)
                {
                    Log.Warn(ex);
                    return ExitCode.NoChanges;
                }
            }

            private IEnumerable<ArchiveFileInfo> GetArchiveFileInfos()
            {
                var archives = new ArchiveFileInfo[(AdditionalArgs.Count + 1) / 3];
                for (int i = 0; i < archives.Length; i++)
                {
                    archives[i] = new ArchiveFileInfo
                    {
                        Path = Path.GetFullPath(AdditionalArgs[i * 3 + 1]),
                        SubDir = (AdditionalArgs.Count > i * 3 + 2) ? AdditionalArgs[i * 3 + 2] : null,
                        MimeType = (AdditionalArgs.Count > i * 3 + 3) ? AdditionalArgs[i * 3 + 3] : Archive.GuessMimeType(AdditionalArgs[i * 3 + 1])
                    };
                }
                return archives;
            }
        }

        internal class Audit : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "audit";

            protected override string Description { get { return Resources.DescriptionStoreAudit; } }

            protected override string Usage { get { return "[CACHE-DIR+]"; } }

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

        internal class Copy : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "copy";

            protected override string Description { get { return Resources.DescriptionStoreCopy; } }

            protected override string Usage { get { return "DIRECTORY [CACHE]"; } }

            protected override int AdditionalArgsMin { get { return 1; } }

            protected override int AdditionalArgsMax { get { return 2; } }

            public Copy([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var store = (AdditionalArgs.Count == 2) ? new DirectoryStore(AdditionalArgs[1]) : Store;

                string path = AdditionalArgs[0];
                Debug.Assert(path != null);
                try
                {
                    store.AddDirectory(Path.GetFullPath(path), new ManifestDigest(Path.GetFileName(path)), Handler);
                    return ExitCode.OK;
                }
                catch (ImplementationAlreadyInStoreException ex)
                {
                    Log.Warn(ex);
                    return ExitCode.NoChanges;
                }
            }
        }

        internal class Find : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "find";

            protected override string Description { get { return Resources.DescriptionStoreFind; } }

            protected override string Usage { get { return "DIGEST"; } }

            protected override int AdditionalArgsMin { get { return 1; } }

            protected override int AdditionalArgsMax { get { return 1; } }

            public Find([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                string path = Store.GetPath(new ManifestDigest(AdditionalArgs[0]));
                if (path == null) throw new ImplementationNotFoundException(new ManifestDigest(AdditionalArgs[0]));
                Handler.Output(string.Format(Resources.LocalPathOf, AdditionalArgs[0]), path);
                return ExitCode.OK;
            }
        }

        internal class List : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "list";

            protected override string Description { get { return Resources.DescriptionStoreList; } }

            protected override string Usage { get { return ""; } }

            protected override int AdditionalArgsMax { get { return 0; } }

            public List([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var composite = Store as CompositeStore;
                Handler.Output(Resources.CachedInterfaces, (composite == null) ? new[] {Store} : composite.Stores);
                return ExitCode.OK;
            }
        }

        internal class ListImplementations : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "list-implementations";

            protected override string Description { get { return Resources.DescriptionStoreListImplementations; } }

            protected override string Usage { get { return "[FEED-URI]"; } }

            protected override int AdditionalArgsMax { get { return 1; } }

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

        private class Manage : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "manage";

            protected override string Description { get { return Resources.DescriptionStoreManage; } }

            protected override string Usage { get { return ""; } }

            protected override int AdditionalArgsMax { get { return 0; } }

            public Manage([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                Handler.ManageStore(Store, FeedCache);
                return ExitCode.OK;
            }
        }

        internal class Optimise : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "optimise";

            public const string AltName = "optimize";

            protected override string Description { get { return Resources.DescriptionStoreOptimise; } }

            protected override string Usage { get { return "[CACHE-DIR+]"; } }

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
            public new const string Name = "purge";

            protected override string Description { get { return Resources.DescriptionStorePurge; } }

            protected override string Usage { get { return "[CACHE-DIR+]"; } }

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

        internal class Remove : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "remove";

            protected override string Description { get { return Resources.DescriptionStoreRemove; } }

            protected override string Usage { get { return "DIGEST+"; } }

            protected override int AdditionalArgsMax { get { return 1; } }

            public Remove([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                foreach (var digest in AdditionalArgs.Select(x => new ManifestDigest(x)))
                {
                    if (!Store.Remove(digest, Handler))
                        throw new ImplementationNotFoundException(digest);
                }
                return ExitCode.OK;
            }
        }

        internal class Verify : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "verify";

            protected override string Description { get { return Resources.DescriptionStoreVerify; } }

            protected override string Usage { get { return "[DIRECTORY] DIGEST"; } }

            protected override int AdditionalArgsMin { get { return 1; } }

            protected override int AdditionalArgsMax { get { return 2; } }

            public Verify([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                try
                {
                    switch (AdditionalArgs.Count)
                    {
                        case 1:
                            // Verify a directory inside the store
                            Store.Verify(new ManifestDigest(AdditionalArgs[0]), Handler);
                            break;

                        case 2:
                            // Verify an arbitrary directory
                            DirectoryStore.VerifyDirectory(AdditionalArgs[0], new ManifestDigest(AdditionalArgs[1]), Handler);
                            break;
                    }
                }
                catch (DigestMismatchException ex)
                {
                    Handler.Output(Resources.VerifyImplementation, ex.Message);
                    return ExitCode.DigestMismatch;
                }

                return ExitCode.OK;
            }
        }
    }
}
