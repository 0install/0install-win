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
using Common;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Management.Cli.Properties;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.Cli
{
    public static partial class Program
    {
        #region Manage
        private static ErrorLevel Add(IList<string> args, ITaskHandler handler)
        {
            if (args.Count < 3) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageAdd));
            var manifestDigest = new ManifestDigest(args[1]);
            string path = args[2];

            if (File.Exists(path))
            { // One or more archives (combined/overlayed)
                var archives = new ArchiveFileInfo[args.Count / 3];
                for (int i = 0; i < archives.Length; i++)
                {
                    archives[i] = new ArchiveFileInfo
                    {
                        Path = args[i * 3 + 2],
                        SubDir = (args.Count > i * 3 + 3) ? args[i * 3 + 3] : null,
                        MimeType = (args.Count > i * 3 + 4) ? args[i * 3 + 4] : null
                    };
                }
                _store.AddArchives(archives, manifestDigest, handler);
                return ErrorLevel.OK;
            }
            else if (Directory.Exists(path))
            { // A single directory
                if (args.Count > 3) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageAdd));
                _store.AddDirectory(path, manifestDigest, handler);
                return ErrorLevel.OK;
            }
            else
            {
                Log.Error(string.Format(Resources.NoSuchFileOrDirectory, path));
                return ErrorLevel.IOError;
            }
        }

        private static void Copy(IList<string> args, ITaskHandler handler)
        {
            if (args.Count < 2 || args.Count > 3) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageCopy));

            IStore store = (args.Count == 3) ? new DirectoryStore(args[2]) : _store;
            store.AddDirectory(args[1], new ManifestDigest(Path.GetFileName(args[1])), handler);
        }

        private static void Find(IList<string> args)
        {
            if (args.Count != 2) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageFind));

            string path = _store.GetPath(new ManifestDigest(args[1]));
            if (path == null) throw new ImplementationNotFoundException(new ManifestDigest(args[1]));
            Console.WriteLine(path);
        }

        private static void Remove(IList<string> args)
        {
            if (args.Count < 2) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageRemove));

            for (int i = 1; i < args.Count; i++)
            {
                _store.Remove(new ManifestDigest(args[i]));
                Log.Info(string.Format(Resources.SuccessfullyRemoved, args[i]));
            }
        }

        // ReSharper disable UnusedParameter.Local
        private static void List(IList<string> args)
        {
            if (args.Count != 1) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageList));

            foreach (ManifestDigest digest in _store.ListAll())
                Console.WriteLine(_store.GetPath(digest));
        }

        // ReSharper restore UnusedParameter.Local

        private static void Optimise(IList<string> args, ITaskHandler handler)
        {
            if (args.Count == 1) _store.Optimise(handler);
            else
            {
                for (int i = 1; i < args.Count; i++)
                    new DirectoryStore(args[i]).Optimise(handler);
            }
        }
        #endregion

        #region Verify
        private static void Verify(IList<string> args, ITaskHandler handler)
        {
            if (args.Count < 2) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageVerify));
            for (int i = 1; i < args.Count; i++)
            {
                if (Directory.Exists(args[i]))
                { // Verify an arbitrary directory
                    DirectoryStore.VerifyDirectory(args[i], new ManifestDigest(Path.GetFileName(args[i])), handler);
                }
                else
                { // Verify a directory inside the default store
                    _store.Verify(new ManifestDigest(args[i]), handler);
                }
                Console.WriteLine(Resources.StoreEntryOK);
            }
        }

        private static ErrorLevel Audit(IList<string> args, ITaskHandler handler)
        {
            if (args.Count == 1) return AuditStore(_store, handler);
            else
            {
                var result = ErrorLevel.OK;
                for (int i = 1; i < args.Count; i++)
                {
                    ErrorLevel tempResult = AuditStore(new DirectoryStore(args[i]), handler);
                    if (tempResult > result) result = tempResult; // Remember only the worst error level
                }
                return result;
            }
        }

        /// <summary>
        /// Recalculates the digests for all entries in the store and ensures they are correct. Prints any problems to the console.
        /// </summary>
        /// <param name="store">The store to be audited.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <returns>The error level to return when the process ends.</returns>
        /// <exception cref="IOException">Thrown if a directory in the store could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the store is not permitted.</exception>
        private static ErrorLevel AuditStore(IStore store, ITaskHandler handler)
        {
            var problems = store.Audit(handler);
            if (problems == null)
            {
                Log.Error(Resources.NoAuditSupport);
                return ErrorLevel.InvalidArguments;
            }

            bool hasProblems = false;
            foreach (var problem in problems)
            {
                Console.WriteLine(problem.Message);
                Console.WriteLine();
                hasProblems = true;
            }
            if (hasProblems)
            {
                Log.Warn(Resources.AuditErrors);
                return ErrorLevel.DigestMismatch;
            }

            Log.Info(Resources.AuditPass);
            return ErrorLevel.OK;
        }
        #endregion

        #region Manifest
        /// <summary>
        /// Prints the manifest for a directory (listing every file and directory in the tree) to the console.
        /// After the manifest, the last line gives the digest of the manifest itself. 
        /// </summary>
        /// <param name="args">The command-line arguments that were not parsed as options.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <exception cref="IOException">Thrown if a directory could not be processed.</exception>
        private static void GenerateManifest(IList<string> args, ITaskHandler handler)
        {
            if (args.Count < 2 || args.Count > 4) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageManifest));

            string path;
            try
            {
                path = Path.GetFullPath(args[1]);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            catch (NotSupportedException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion

            Manifest manifest;
            if (File.Exists(path))
            {
                // Determine manifest format
                var format = (args.Count == 4)
                    ? ManifestFormat.FromPrefix(args[3])
                    : ManifestFormat.Recommended.First();

                using (var tempDir = new TemporaryDirectory("0store"))
                {
                    // Extract archive to temp dir to generate manifest
                    using (var extractor = Extractor.CreateExtractor(null, path, 0, tempDir.Path))
                    {
                        if (args.Count >= 3) extractor.SubDir = args[2];
                        handler.RunTask(extractor, null);
                    }

                    manifest = Manifest.Generate(tempDir.Path, format, handler, path);
                }
            }
            else if (Directory.Exists(path))
            {
                // Determine manifest format
                ManifestFormat format;
                if (args.Count == 3) format = ManifestFormat.FromPrefix(args[2]);
                else
                {
                    try
                    {
                        // Try to extract the algorithm from the directory name
                        format = ManifestFormat.FromPrefix(Path.GetFileName(path).GetLeftPartAtFirstOccurrence('='));
                    }
                    catch (ArgumentException)
                    {
                        // Default to the best available algorithm
                        format = ManifestFormat.Recommended.First();
                    }
                }

                manifest = Manifest.Generate(path, format, handler, path);
            }
            else throw new DirectoryNotFoundException(string.Format(Resources.DirectoryNotFound, path));

            Console.Write(manifest);
            Console.WriteLine(manifest.CalculateDigest());
        }
        #endregion
    }
}
