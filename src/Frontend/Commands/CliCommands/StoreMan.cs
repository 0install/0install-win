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
using System.Text;
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
    [CLSCompliant(false)]
    public sealed class StoreMan : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "store";

        /// <inheritdoc/>
        protected override string Description
        {
            get
            {
                string[,] subcommands =
                {
                    {"add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))", Resources.DescriptionStoreAdd},
                    {"audit [CACHE+]", Resources.DescriptionStoreAudit},
                    {"copy DIRECTORY [CACHE]", Resources.DescriptionStoreCopy},
                    {"find DIGEST", Resources.DescriptionStoreFind},
                    {"list", Resources.DescriptionStoreList},
                    {"list-implementations", Resources.DescriptionStoreListImplementations},
                    {"manage", Resources.DescriptionStoreManage},
                    {"optimise [CACHE+]", Resources.DescriptionStoreOptimise},
                    {"purge [CACHE+]", Resources.DescriptionStorePurge},
                    {"remove DIGEST+", Resources.DescriptionStoreRemove},
                    {"verify [DIRECTORY] DIGEST", Resources.DescriptionStoreVerify}
                };

                var builder = new StringBuilder();
                for (int i = 0; i < subcommands.GetLength(0); i++)
                {
                    builder.AppendLine();
                    builder.Append("0install store ");
                    builder.AppendLine(subcommands[i, 0]);
                    builder.AppendLine(subcommands[i, 1]);
                }

                return builder.ToString();
            }
        }

        /// <inheritdoc/>
        protected override string Usage { get { return "SUBCOMMAND"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        public StoreMan([NotNull] ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            string mode = AdditionalArgs[0];
            switch (mode)
            {
                case "add":
                    return Add();

                case "audit":
                    return Audit();

                case "copy":
                    return Copy();

                case "find":
                    Find();
                    return ExitCode.OK;

                case "list":
                    List();
                    return ExitCode.OK;

                case "list-implementations":
                    ListImplementations();
                    return ExitCode.OK;

                case "manage":
                    Handler.ManageStore(Store, FeedCache);
                    return ExitCode.OK;

                case "manifest":
                    throw new NotSupportedException(string.Format(Resources.UseInstead, "0install digest --manifest"));

                case "optimise":
                case "optimize":
                    Optimise();
                    return ExitCode.OK;

                case "purge":
                    Purge();
                    return ExitCode.OK;

                case "remove":
                    Remove();
                    return ExitCode.OK;

                case "verify":
                    return Verify();

                default:
                    throw new OptionException(Resources.UnknownMode, mode);
            }
        }

        #region Subcommands
        private ExitCode Add()
        {
            if (AdditionalArgs.Count < 3) throw new OptionException(Resources.MissingArguments + Environment.NewLine + "add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))", null);

            var manifestDigest = new ManifestDigest(AdditionalArgs[1]);
            string path = AdditionalArgs[2];
            try
            {
                if (File.Exists(path))
                { // One or more archives (combined/overlayed)
                    Store.AddArchives(GetArchiveFileInfos(), manifestDigest, Handler);
                    return ExitCode.OK;
                }
                else if (Directory.Exists(path))
                { // A single directory
                    if (AdditionalArgs.Count > 3) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))", AdditionalArgs[3]);
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

        private ExitCode Audit()
        {
            var store = GetStore();
            foreach (var manifestDigest in store.ListAll())
                store.Verify(manifestDigest, Handler);
            return ExitCode.OK;
        }

        private ExitCode Copy()
        {
            if (AdditionalArgs.Count < 2) throw new OptionException(Resources.MissingArguments + Environment.NewLine + "copy DIRECTORY [CACHE]", null);
            if (AdditionalArgs.Count > 3) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "copy DIRECTORY [CACHE]", AdditionalArgs[3]);

            var store = (AdditionalArgs.Count == 3) ? new DirectoryStore(AdditionalArgs[2]) : Store;

            string path = AdditionalArgs[1];
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

        private void Find()
        {
            if (AdditionalArgs.Count < 2) throw new OptionException(Resources.MissingArguments + Environment.NewLine + "find DIGEST", null);
            if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "find DIGEST", AdditionalArgs[2]);

            string path = Store.GetPath(new ManifestDigest(AdditionalArgs[1]));
            if (path == null) throw new ImplementationNotFoundException(new ManifestDigest(AdditionalArgs[1]));
            Handler.Output(string.Format(Resources.LocalPathOf, AdditionalArgs[1]), path);
        }

        private void List()
        {
            if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments, AdditionalArgs[1]);

            var composite = Store as CompositeStore;
            Handler.Output(Resources.CachedInterfaces, (composite == null) ? new[] {Store} : composite.Stores);
        }

        private void ListImplementations()
        {
            if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments, AdditionalArgs[2]);

            var nodeBuilder = new CacheNodeBuilder(Store, FeedCache);
            nodeBuilder.Run();

            if (AdditionalArgs.Count == 2)
            {
                var uri = GetCanonicalUri(AdditionalArgs[1]);
                var nodes = nodeBuilder.Nodes.OfType<OwnedImplementationNode>().Where(x => x.FeedUri == uri);
                Handler.Output(Resources.CachedImplementations, nodes);
            }
            else
            {
                var nodes = nodeBuilder.Nodes.OfType<ImplementationNode>();
                Handler.Output(Resources.CachedImplementations, nodes);
            }
        }

        private void Optimise()
        {
            long savedBytes = GetStore().Optimise(Handler);
            Handler.OutputLow(Resources.OptimiseComplete, string.Format(Resources.StorageReclaimed, savedBytes.FormatBytes(CultureInfo.CurrentCulture)));
        }

        private void Purge()
        {
            if (Handler.Ask(Resources.ConfirmPurge, defaultAnswer: true)) GetStore().Purge(Handler);
            else throw new OperationCanceledException();
        }

        private void Remove()
        {
            if (AdditionalArgs.Count < 2) throw new OptionException(Resources.MissingArguments + Environment.NewLine + "remove DIGEST+", null);

            foreach (var digest in AdditionalArgs.Skip(1).Select(x => new ManifestDigest(x)))
            {
                if (!Store.Remove(digest, Handler))
                    throw new ImplementationNotFoundException(digest);
            }
        }

        private ExitCode Verify()
        {
            try
            {
                switch (AdditionalArgs.Count)
                {
                    case 1:
                        throw new OptionException(Resources.MissingArguments + Environment.NewLine + "verify [DIRECTORY] DIGEST" + Environment.NewLine + Resources.StoreVerfiyTryAuditInstead, null);

                    case 2:
                        // Verify a directory inside the store
                        Store.Verify(new ManifestDigest(AdditionalArgs[1]), Handler);
                        break;

                    case 3:
                        // Verify an arbitrary directory
                        DirectoryStore.VerifyDirectory(AdditionalArgs[1], new ManifestDigest(AdditionalArgs[2]), Handler);
                        break;

                    default:
                        throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "verify [DIRECTORY] DIGEST", "");
                }
            }
            catch (DigestMismatchException ex)
            {
                Handler.Output(Resources.VerifyImplementation, ex.Message);
                return ExitCode.DigestMismatch;
            }

            return ExitCode.OK;
        }
        #endregion

        #region Helpers
        private IEnumerable<ArchiveFileInfo> GetArchiveFileInfos()
        {
            var archives = new ArchiveFileInfo[AdditionalArgs.Count / 3];
            for (int i = 0; i < archives.Length; i++)
            {
                archives[i] = new ArchiveFileInfo
                {
                    Path = Path.GetFullPath(AdditionalArgs[i * 3 + 2]),
                    SubDir = (AdditionalArgs.Count > i * 3 + 3) ? AdditionalArgs[i * 3 + 3] : null,
                    MimeType = (AdditionalArgs.Count > i * 3 + 4) ? AdditionalArgs[i * 3 + 4] : Archive.GuessMimeType(AdditionalArgs[i * 3 + 2])
                };
            }
            return archives;
        }

        /// <summary>
        /// Returns the default <see cref="IStore"/> or a <see cref="CompositeStore"/> as specifief by the <see cref="CliCommand.AdditionalArgs"/>.
        /// </summary>
        private IStore GetStore()
        {
            return (AdditionalArgs.Count == 1)
                ? Store
                : new CompositeStore(AdditionalArgs.Skip(1).Select(arg => (IStore)new DirectoryStore(arg)));
        }
        #endregion
    }
}
