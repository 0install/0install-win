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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    partial class StoreMan
    {
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
                        if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + AdditionalArgs.Skip(2).JoinEscapeArguments(), null);
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
                var manifestDigest = new ManifestDigest(Path.GetFileName(path));

                try
                {
                    store.AddDirectory(Path.GetFullPath(path), manifestDigest, Handler);
                    return ExitCode.OK;
                }
                catch (ImplementationAlreadyInStoreException ex)
                {
                    Log.Warn(ex);
                    return ExitCode.NoChanges;
                }
            }
        }

        internal class Export : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "export";

            protected override string Description { get { return Resources.DescriptionStoreExport; } }

            protected override string Usage { get { return "DIGEST OUTPUT-ARCHIVE [MIME-TYPE]"; } }

            protected override int AdditionalArgsMin { get { return 2; } }

            protected override int AdditionalArgsMax { get { return 3; } }

            public Export([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var manifestDigest = new ManifestDigest(AdditionalArgs[0]);

                string outputArchive = AdditionalArgs[1];
                Debug.Assert(outputArchive != null);

                string sourceDirectory = Store.GetPath(manifestDigest);
                if (sourceDirectory == null)
                    throw new ImplementationNotFoundException(manifestDigest);

                string mimeType = (AdditionalArgs.Count == 3) ? AdditionalArgs[3] : null;

                using (var generator = ArchiveGenerator.Create(sourceDirectory, outputArchive, mimeType))
                    Handler.RunTask(generator);
                return ExitCode.OK;
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
                var manifestDigest = new ManifestDigest(AdditionalArgs[0]);

                string path = Store.GetPath(manifestDigest);
                if (path == null) throw new ImplementationNotFoundException(manifestDigest);
                Handler.Output(string.Format(Resources.LocalPathOf, AdditionalArgs[0]), path);
                return ExitCode.OK;
            }
        }

        internal class Remove : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "remove";

            protected override string Description { get { return Resources.DescriptionStoreRemove; } }

            protected override string Usage { get { return "DIGEST+"; } }

            protected override int AdditionalArgsMin { get { return 1; } }

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
