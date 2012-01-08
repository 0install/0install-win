/*
 * Copyright 2010-2012 Bastian Eicher
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
using Common;
using Common.Collections;
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
            if (args.Count < 3 || args.Count > 4) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageAdd));
            var manifestDigest = new ManifestDigest(args[1]);
            string path = args[2];
            string subDir = (args.Count == 4) ? args[3] : null;

            if (Directory.Exists(path)) _store.AddDirectory(path, manifestDigest, handler);
            else if (File.Exists(path)) _store.AddArchives(new[] {new ArchiveFileInfo {Path = path, SubDir = subDir}}, manifestDigest, handler);
            else
            {
                Log.Error(string.Format(Resources.NoSuchFileOrDirectory, path));
                return ErrorLevel.IOError;
            }
            return ErrorLevel.OK;
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

            Console.WriteLine(_store.GetPath(new ManifestDigest(args[1])));
        }

        private static void Remove(IList<string> args, ITaskHandler handler)
        {
            if (args.Count < 2) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageRemove));

            for (int i = 1; i < args.Count; i++)
            {
                _store.Remove(new ManifestDigest(args[i]), handler);
                Log.Info(string.Format(Resources.SuccessfullyRemoved, args[i]));
            }
        }

        private static void List(IList<string> args)
        {
            if (args.Count != 1) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageList));

            foreach (ManifestDigest digest in _store.ListAll())
                Console.WriteLine(digest.BestDigest);
        }

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
            ErrorLevel result = ErrorLevel.OK;
            if (args.Count == 1) AuditStore(_store, handler);
            else
            {
                for (int i = 1; i < args.Count; i++)
                {
                    ErrorLevel tempResult = AuditStore(new DirectoryStore(args[i]), handler);
                    if (tempResult > result) result = tempResult;
                }
            }
            return result;
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
            if (args.Count < 2 || args.Count > 3) throw new ArgumentException(string.Format(Resources.WrongNoArguments, Resources.UsageManifest));

            // Determine manifest format
            ManifestFormat format;
            if (args.Count == 3) format = ManifestFormat.FromPrefix(args[2]);
            else
            {
                try
                {
                    // Try to extract the algorithm from the directory name
                    format = ManifestFormat.FromPrefix(StringUtils.GetLeftPartAtFirstOccurrence(Path.GetFileName(Path.GetFullPath(args[1])), '='));
                }
                catch (ArgumentException)
                {
                    // Default to the best available algorithm
                    format = EnumerableUtils.GetFirst(ManifestFormat.Recommended);
                }
            }

            string path = args[1];
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(string.Format(Resources.DirectoryNotFound, path));

            var manifest = Manifest.Generate(path, format, handler, path);
            Console.Write(manifest);
            Console.WriteLine(manifest.CalculateDigest());
        }
        #endregion
    }
}
