/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Tasks;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Management;

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
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "store";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionStore; } }

        /// <inheritdoc/>
        protected override string Usage
        {
            get
            {
                var usages = new[] {"", Resources.UsageStoreAdd, Resources.UsageStoreAudit, Resources.UsageStoreCopy, Resources.UsageStoreFind, Resources.UsageStoreList, Resources.UsageStoreOptimize, Resources.UsageStorePurge, Resources.UsageStoreRemove, Resources.UsageStoreVerify};
                return "SUBCOMMAND" + Environment.NewLine + string.Join(Environment.NewLine + "\t0install store ", usages) + Environment.NewLine;
            }
        }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public StoreMan(Resolver resolver)
            : base(resolver)
        {
            Options.Add("man", () => Resources.OptionMan, unused =>
            {
                Resolver.Handler.Output("0install store sub-commands",
                    @"ADD" + Environment.NewLine + Environment.NewLine + Resources.DetailsStoreAdd + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    @"AUDIT" + Environment.NewLine + Environment.NewLine + Resources.DetailsStoreAudit + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    @"COPY" + Environment.NewLine + Environment.NewLine + Resources.DetailsStoreCopy + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    @"FIND" + Environment.NewLine + Environment.NewLine + Resources.DetailsStoreFind + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    @"LIST" + Environment.NewLine + Environment.NewLine + Resources.DetailsStoreList + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    @"MANAGE" + Environment.NewLine + Environment.NewLine + Resources.DetailsStoreManage + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    @"PURGE" + Environment.NewLine + Environment.NewLine + Resources.DetailsStorePurge + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    @"REMOVE" + Environment.NewLine + Environment.NewLine + Resources.DetailsStoreRemove + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    @"VERIFY" + Environment.NewLine + Environment.NewLine + Resources.DetailsStoreVerify);

                throw new OperationCanceledException(); // Don't handle any of the other arguments
            });
        }
        #endregion

        //--------------------//

        #region Execute
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
                    ProcessUtils.LaunchAssembly( /*MonoUtils.IsUnix ? "0store-gtk" :*/ "0store-win", AdditionalArgs.JoinEscapeArguments());
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
        #endregion

        #region Subcommands
        private void Add()
        {
            if (AdditionalArgs.Count < 3) throw new ArgumentException(Resources.MissingArguments + Environment.NewLine + Resources.UsageStoreAdd);

            Resolver.Handler.ShowProgressUI();

            var manifestDigest = new ManifestDigest(AdditionalArgs[1]);
            string path = AdditionalArgs[2];
            if (File.Exists(path))
            { // One or more archives (combined/overlayed)
                Resolver.Store.AddArchives(GetArchiveFileInfos(), manifestDigest, Resolver.Handler);
            }
            else if (Directory.Exists(path))
            { // A single directory
                if (AdditionalArgs.Count > 3) throw new ArgumentException(Resources.TooManyArguments + Environment.NewLine + Resources.UsageStoreAdd);
                Resolver.Store.AddDirectory(path, manifestDigest, Resolver.Handler);
            }
            else throw new FileNotFoundException(string.Format(Resources.NoSuchFileOrDirectory, path), path);
        }

        private StoreErrorLevel Audit()
        {
            Resolver.Handler.ShowProgressUI();

            var problems = GetStore().Audit(Resolver.Handler);
            if (problems == null) throw new NotSupportedException(Resources.NoAuditSupport);

            if (problems.Any())
            {
                Resolver.Handler.Output(Resources.StoreAudit, Resources.AuditErrors);
                return StoreErrorLevel.DigestMismatch;
            }
            else
            {
                Resolver.Handler.Output(Resources.StoreAudit, Resources.AuditPass);
                return StoreErrorLevel.OK;
            }
        }

        private void Copy()
        {
            if (AdditionalArgs.Count < 2) throw new ArgumentException(Resources.MissingArguments + Environment.NewLine + Resources.UsageStoreCopy);
            if (AdditionalArgs.Count > 3) throw new ArgumentException(Resources.TooManyArguments + Environment.NewLine + Resources.UsageStoreCopy);

            Resolver.Handler.ShowProgressUI();

            var store = (AdditionalArgs.Count == 3) ? new DirectoryStore(AdditionalArgs[2]) : Resolver.Store;
            store.AddDirectory(AdditionalArgs[1], new ManifestDigest(Path.GetFileName(AdditionalArgs[1])), Resolver.Handler);
        }

        private void Find()
        {
            if (AdditionalArgs.Count < 2) throw new ArgumentException(Resources.MissingArguments + Environment.NewLine + Resources.UsageStoreFind);
            if (AdditionalArgs.Count > 2) throw new ArgumentException(Resources.TooManyArguments + Environment.NewLine + Resources.UsageStoreFind);

            string path = Resolver.Store.GetPath(new ManifestDigest(AdditionalArgs[1]));
            if (path == null) throw new ImplementationNotFoundException(new ManifestDigest(AdditionalArgs[1]));
            Resolver.Handler.Output(string.Format(Resources.LocalPathOf, AdditionalArgs[1]), path);
        }

        private void List()
        {
            if (AdditionalArgs.Count > 2) throw new ArgumentException(Resources.TooManyArguments + Environment.NewLine + Resources.UsageStoreList);

            Resolver.Handler.Output(Resources.CachedInterfaces, Environment.NewLine.Join(Resolver.Store.ListAll().Select(Resolver.Store.GetPath)));
        }

        private void Optimise()
        {
            Resolver.Handler.ShowProgressUI();

            GetStore().Optimise(Resolver.Handler);
        }

        private void Purge()
        {
            Resolver.Handler.ShowProgressUI();

            GetStore().Purge(Resolver.Handler);
        }

        private void Remove()
        {
            if (AdditionalArgs.Count < 2) throw new ArgumentException(Resources.MissingArguments + Environment.NewLine + Resources.UsageStoreRemove);

            Resolver.Handler.ShowProgressUI();

            var digests = AdditionalArgs.Skip(1).Select(x => new ManifestDigest(x));
            Resolver.Handler.RunTask(new ForEachTask<ManifestDigest>(Resources.RemovingImplementations, digests, Resolver.Store.Remove));
        }

        private StoreErrorLevel Verify()
        {
            Resolver.Handler.ShowProgressUI();

            try
            {
                foreach (string arg in AdditionalArgs.Skip(1))
                {
                    if (Directory.Exists(arg))
                    { // Verify an arbitrary directory
                        string path = arg;
                        var digest = new ManifestDigest(Path.GetFileName(arg));
                        DirectoryStore.VerifyDirectory(path, digest, Resolver.Handler);
                    }
                    else
                    { // Verify a directory inside the default store
                        Resolver.Store.Verify(new ManifestDigest(arg), Resolver.Handler);
                    }
                }
            }
                #region Error handling
            catch (DigestMismatchException ex)
            {
                Resolver.Handler.Output(Resources.VerifyImplementation, ex.Message);
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
                    MimeType = (AdditionalArgs.Count > i * 3 + 4) ? AdditionalArgs[i * 3 + 4] : null
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
                ? Resolver.Store
                : new CompositeStore(AdditionalArgs.Skip(1).Select(arg => (IStore)new DirectoryStore(arg)));
        }
        #endregion
    }
}
