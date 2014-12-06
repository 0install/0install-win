/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands
{

    #region Enumerations
    /// <summary>
    /// An StoreErrorLevel is returned to the original caller after the application terminates, to indicate success or the reason for failure.
    /// </summary>
    public enum StoreErrorLevel
    {
        ///<summary>Everything is OK.</summary>
        OK = 0,

        /// <summary>An implementation to be added is already in the store.</summary>
        ImplementationAlreadyInStore = 10,

        /// <summary>A manifest digest for an implementation did not match the expected value.</summary>
        DigestMismatch = 20
    }
    #endregion

    /// <summary>
    /// Manages the contents of the <see cref="IStore"/>s.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class StoreMan : FrontendCommand
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
                    {"verify (DIGEST|DIRECTORY)+", Resources.DescriptionStoreVerify}
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
        public StoreMan(ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            switch (AdditionalArgs[0])
            {
                case "add":
                    return (int)Add();

                case "audit":
                    return (int)Audit();

                case "copy":
                    return (int)Copy();

                case "find":
                    Find();
                    return (int)StoreErrorLevel.OK;

                case "list":
                    List();
                    return (int)StoreErrorLevel.OK;

                case "list-implementations":
                    ListImplementations();
                    return (int)StoreErrorLevel.OK;

                case "manage":
                    Handler.ManageStore(Store, FeedCache);
                    return (int)StoreErrorLevel.OK;

                case "manifest":
                    throw new NotSupportedException(string.Format(Resources.UseInstead, "0install digest --manifest"));

                case "optimise":
                case "optimize":
                    Optimise();
                    return (int)StoreErrorLevel.OK;

                case "purge":
                    Purge();
                    return (int)StoreErrorLevel.OK;

                case "remove":
                    Remove();
                    return (int)StoreErrorLevel.OK;

                case "verify":
                    return (int)Verify();

                default:
                    throw new OptionException(Resources.UnknownMode, "");
            }
        }

        #region Subcommands
        private StoreErrorLevel Add()
        {
            if (AdditionalArgs.Count < 3) throw new OptionException(Resources.MissingArguments + Environment.NewLine + "add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))", "");

            var manifestDigest = new ManifestDigest(AdditionalArgs[1]);
            string path = AdditionalArgs[2];
            try
            {
                if (File.Exists(path))
                { // One or more archives (combined/overlayed)
                    Store.AddArchives(GetArchiveFileInfos(), manifestDigest, Handler);
                    return StoreErrorLevel.OK;
                }
                else if (Directory.Exists(path))
                { // A single directory
                    if (AdditionalArgs.Count > 3) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))", "");
                    Store.AddDirectory(Path.GetFullPath(path), manifestDigest, Handler);
                    return StoreErrorLevel.OK;
                }
                else throw new FileNotFoundException(string.Format(Resources.NoSuchFileOrDirectory, path), path);
            }
            catch (ImplementationAlreadyInStoreException ex)
            {
                Log.Warn(ex);
                return StoreErrorLevel.ImplementationAlreadyInStore;
            }
        }

        private StoreErrorLevel Audit()
        {
            var store = GetStore();
            foreach (var manifestDigest in store.ListAll())
                store.Verify(manifestDigest, Handler);
            return StoreErrorLevel.OK;
        }

        private StoreErrorLevel Copy()
        {
            if (AdditionalArgs.Count < 2) throw new OptionException(Resources.MissingArguments + Environment.NewLine + "copy DIRECTORY [CACHE]", "");
            if (AdditionalArgs.Count > 3) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "copy DIRECTORY [CACHE]", "");

            var store = (AdditionalArgs.Count == 3) ? new DirectoryStore(AdditionalArgs[2]) : Store;

            string path = AdditionalArgs[1];
            try
            {
                store.AddDirectory(Path.GetFullPath(path), new ManifestDigest(Path.GetFileName(path)), Handler);
                return StoreErrorLevel.OK;
            }
            catch (ImplementationAlreadyInStoreException ex)
            {
                Log.Warn(ex);
                return StoreErrorLevel.ImplementationAlreadyInStore;
            }
        }

        private void Find()
        {
            if (AdditionalArgs.Count < 2) throw new OptionException(Resources.MissingArguments + Environment.NewLine + "find DIGEST", "");
            if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "find DIGEST", "");

            string path = Store.GetPath(new ManifestDigest(AdditionalArgs[1]));
            if (path == null) throw new ImplementationNotFoundException(new ManifestDigest(AdditionalArgs[1]));
            Handler.Output(string.Format(Resources.LocalPathOf, AdditionalArgs[1]), path);
        }

        private void List()
        {
            if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "list", "");

            var composite = Store as CompositeStore;
            Handler.Output(Resources.CachedInterfaces, (composite == null) ? new[] { Store } : composite.Stores);
        }

        private void ListImplementations()
        {
            if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + "list", "");

            Handler.Output(Resources.CachedInterfaces, Store.ListAll());
        }

        private void Optimise()
        {
            long savedBytes = GetStore().Optimise(Handler);
            Handler.OutputLow(Resources.OptimiseComplete, string.Format(Resources.StorageReclaimed, savedBytes.FormatBytes(CultureInfo.CurrentCulture)));
        }

        private void Purge()
        {
            if (Handler.Batch || Handler.AskQuestion(Resources.ConfirmPurge)) GetStore().Purge(Handler);
            else throw new OperationCanceledException();
        }

        private void Remove()
        {
            if (AdditionalArgs.Count < 2) throw new OptionException(Resources.MissingArguments + Environment.NewLine + "remove DIGEST+", "");

            var digests = AdditionalArgs.Skip(1).Select(x => new ManifestDigest(x));
            Handler.RunTask(new ForEachTask<ManifestDigest>(Resources.RemovingImplementations, digests, digest =>
            {
                if (!Store.Remove(digest))
                    throw new ImplementationNotFoundException(digest);
            }));
        }

        private StoreErrorLevel Verify()
        {
            try
            {
                foreach (string arg in AdditionalArgs.Skip(1))
                {
                    if (Directory.Exists(arg))
                    { // Verify an arbitrary directory
                        string path = arg;
                        var digest = new ManifestDigest(Path.GetFileName(arg));
                        DirectoryStore.VerifyDirectory(path, digest, Handler);
                    }
                    else
                    { // Verify a directory inside the default store
                        Store.Verify(new ManifestDigest(arg), Handler);
                    }
                }
            }
                #region Error handling
            catch (DigestMismatchException ex)
            {
                Handler.Output(Resources.VerifyImplementation, ex.Message);
                return StoreErrorLevel.DigestMismatch;
            }
            #endregion

            return StoreErrorLevel.OK;
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
        /// Returns the default <see cref="IStore"/> or a <see cref="CompositeStore"/> as specifief by the <see cref="FrontendCommand.AdditionalArgs"/>.
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
