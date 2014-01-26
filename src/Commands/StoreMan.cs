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
using System.IO;
using System.Linq;
using System.Text;
using Common.Tasks;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Management;
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

        /// <summary>A manifest digest for an implementation did not match the expected value.</summary>
        DigestMismatch = 20,
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
                    {"manage", Resources.DescriptionStoreManage},
                    {"optimise [CACHE+]", Resources.DescriptionStoreOptimise},
                    {"purge [CACHE+]", Resources.DescriptionStorePurge},
                    {"remove DIGEST+", Resources.DescriptionStoreRemove},
                    {"verify (DIGEST|DIRECTORY)+", Resources.DescriptionStoreVerify}
                };

                var builder = new StringBuilder();
                for (int i = 0; i < subcommands.GetLength(0); i++)
                    builder.Append(Environment.NewLine + "0install store " + subcommands[i, 0] + Environment.NewLine + subcommands[i, 1] + Environment.NewLine);

                return builder.ToString();
            }
        }

        /// <inheritdoc/>
        protected override string Usage { get { return "SUBCOMMAND"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        public StoreMan(IBackendHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            switch (AdditionalArgs[0])
            {
                case "add":
                    Add();
                    return (int)StoreErrorLevel.OK;

                case "audit":
                    return (int)Audit();

                case "copy":
                    Copy();
                    return (int)StoreErrorLevel.OK;

                case "find":
                    Find();
                    return (int)StoreErrorLevel.OK;

                case "list":
                    List();
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
        private void Add()
        {
            if (AdditionalArgs.Count < 3) throw new ArgumentException(Resources.MissingArguments + Environment.NewLine + "add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))");

            Handler.ShowProgressUI();

            var manifestDigest = new ManifestDigest(AdditionalArgs[1]);
            string path = AdditionalArgs[2];
            if (File.Exists(path))
            { // One or more archives (combined/overlayed)
                Store.AddArchives(GetArchiveFileInfos(), manifestDigest, Handler);
            }
            else if (Directory.Exists(path))
            { // A single directory
                if (AdditionalArgs.Count > 3) throw new ArgumentException(Resources.TooManyArguments + Environment.NewLine + "add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT [MIME-TYPE [...]]))");
                Store.AddDirectory(path, manifestDigest, Handler);
            }
            else throw new FileNotFoundException(string.Format(Resources.NoSuchFileOrDirectory, path), path);
        }

        private StoreErrorLevel Audit()
        {
            Handler.ShowProgressUI();

            var problems = GetStore().Audit(Handler);
            if (problems == null) throw new NotSupportedException(Resources.NoAuditSupport);

            if (problems.Any())
            {
                Handler.Output(Resources.StoreAudit, Resources.AuditErrors);
                return StoreErrorLevel.DigestMismatch;
            }
            else
            {
                Handler.Output(Resources.StoreAudit, Resources.AuditPass);
                return StoreErrorLevel.OK;
            }
        }

        private void Copy()
        {
            if (AdditionalArgs.Count < 2) throw new ArgumentException(Resources.MissingArguments + Environment.NewLine + "copy DIRECTORY [CACHE]");
            if (AdditionalArgs.Count > 3) throw new ArgumentException(Resources.TooManyArguments + Environment.NewLine + "copy DIRECTORY [CACHE]");

            Handler.ShowProgressUI();

            var store = (AdditionalArgs.Count == 3) ? new DirectoryStore(AdditionalArgs[2]) : Store;
            store.AddDirectory(AdditionalArgs[1], new ManifestDigest(Path.GetFileName(AdditionalArgs[1])), Handler);
        }

        private void Find()
        {
            if (AdditionalArgs.Count < 2) throw new ArgumentException(Resources.MissingArguments + Environment.NewLine + "find DIGEST");
            if (AdditionalArgs.Count > 2) throw new ArgumentException(Resources.TooManyArguments + Environment.NewLine + "find DIGEST");

            string path = Store.GetPath(new ManifestDigest(AdditionalArgs[1]));
            if (path == null) throw new ImplementationNotFoundException(new ManifestDigest(AdditionalArgs[1]));
            Handler.Output(string.Format(Resources.LocalPathOf, AdditionalArgs[1]), path);
        }

        private void List()
        {
            if (AdditionalArgs.Count > 2) throw new ArgumentException(Resources.TooManyArguments + Environment.NewLine + "list");

            Handler.Output(Resources.CachedInterfaces, StringUtils.Join(Environment.NewLine, Store.ListAll().Select(Store.GetPath)));
        }

        private void Optimise()
        {
            Handler.ShowProgressUI();

            GetStore().Optimise(Handler);
        }

        private void Purge()
        {
            Handler.ShowProgressUI();
            if (Handler.Batch || Handler.AskQuestion(Resources.ConfirmPurge)) GetStore().Purge(Handler);
            else throw new OperationCanceledException();
        }

        private void Remove()
        {
            if (AdditionalArgs.Count < 2) throw new ArgumentException(Resources.MissingArguments + Environment.NewLine + "remove DIGEST+");

            Handler.ShowProgressUI();

            var digests = AdditionalArgs.Skip(1).Select(x => new ManifestDigest(x));
            Handler.RunTask(new ForEachTask<ManifestDigest>(Resources.RemovingImplementations, digests, Store.Remove));
        }

        private StoreErrorLevel Verify()
        {
            Handler.ShowProgressUI();

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
                    Path = AdditionalArgs[i * 3 + 2],
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
