/*
 * Copyright 2010 Bastian Eicher
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
using System.Reflection;
using Common;
using Common.Collections;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Management.Cli.Properties;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.Cli
{
    #region Enumerations
    /// <summary>
    /// An errorlevel is returned to the original caller after the application terminates, to indicate success or the reason for failure.
    /// </summary>
    public enum ErrorLevel
    {
        ///<summary>Everything is OK.</summary>
        OK = 0,

        /// <summary>The user cancelled the operation.</summary>
        UserCancelled = 1,

        /// <summary>The arguments passed on the command-line were not valid.</summary>
        InvalidArguments = 2,

        /// <summary>A not supported feature was requested.</summary>
        NotSupported = 3,

        /// <summary>An IO error occurred.</summary>
        IOError = 10,

        /// <summary>A requested implementation could not be found or could not be launched.</summary>
        ImplementationError = 15,

        /// <summary>A manifest digest for an implementation did not match the expected value.</summary>
        DigestMismatch = 20,
    }
    #endregion

    /// <summary>
    /// Launches a command-line tool for managing caches of Zero Install implementations.
    /// </summary>
    public static class Program
    {
        #region Text constants
        private const string UsageAdd = "0store add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT]))";
        private const string UsageAudit = "0store audit [CACHE+]";
        private const string UsageCopy = "0store copy DIRECTORY [CACHE]";
        private const string UsageFind = "0store find DIGEST";
        private const string UsageRemove = "0store remove DIGEST+";
        private const string UsageList = "0store list";
        private const string UsageManifest = "0store manifest DIRECTORY [ALGORITHM]";
        private const string UsageOptimize = "0store optimise [CACHE+]";
        private const string UsageVerify = "0store verify (DIGEST|DIRECTORY)+";
        #endregion

        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            // Automatically show help for missing args
            if (args.Length == 0) args = new[] {"--help"};

            IIOHandler handler = new CliHandler();

            IList<string> restArgs;
            try { restArgs = ParseArgs(args, handler); }
            #region Error handling
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            if (restArgs.Count == 0) return (int)ErrorLevel.OK;

            try { return (int)ExecuteArgs(restArgs, handler); }
            #region Error handling
            catch (UserCancelException)
            {
                return (int)ErrorLevel.UserCancelled;
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (ImplementationNotFoundException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.ImplementationError;
            }
            catch (ImplementationAlreadyInStoreException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.ImplementationError;
            }
            catch (DigestMismatchException ex)
            {
                // ToDo: Display manifest diff
                Log.Error(ex.Message);
                return (int)ErrorLevel.DigestMismatch;
            }
            #endregion
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <returns>Any unparsed commands left over.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        private static IList<string> ParseArgs(IEnumerable<string> args, IIOHandler handler)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            #region Define options
            var options = new OptionSet
            {
                // Special operations
                {"batch", Resources.OptionBatch, unused => handler.Batch = true},

                // Mode selection
                {"V|version", Resources.OptionVersion, unused => Console.WriteLine(@"Zero Install Store management CLI v{0}", Assembly.GetEntryAssembly().GetName().Version)}
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                const string usage = "Usage:\t{0}\n\t{1}\n\t{2}\n\t{3}\n\t{4}\n\t{5}\n\t{5}\n\t{6}\n\t{7}\n\t{8}\n";
                Console.WriteLine(usage, UsageAdd, UsageAudit, UsageCopy, UsageFind, UsageRemove, UsageList, UsageManifest, UsageOptimize, UsageVerify);
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            return options.Parse(args);
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments that were not parsed as options.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <returns>The error level to return when the process ends.</returns>
        /// <exception cref="UserCancelException">Thrown if an IO task was cancelled.</exception>
        /// <exception cref="ArgumentException">Thrown if the number of arguments passed in <paramref name="args"/> is incorrect.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if no implementation matching the <see cref="ManifestDigest"/> could be found in this store.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Model.Implementation"/> with the specified <see cref="ManifestDigest"/> in the store.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the archive/directory content doesn't match the <see cref="ManifestDigest"/>.</exception>
        private static ErrorLevel ExecuteArgs(IList<string> args, IIOHandler handler)
        {
            switch (args[0])
            {
                case "add":
                    return Add(args, handler);

                case "audit":
                    return Audit(args, handler);

                case "copy":
                    Copy(args, handler);
                    return ErrorLevel.OK;

                case "find":
                    Find(args);
                    return ErrorLevel.OK;

                case "remove":
                    Remove(args);
                    return ErrorLevel.OK;

                case "list":
                    List(args);
                    return ErrorLevel.OK;

                case "manifest":
                    GenerateManifest(args, handler);
                    return ErrorLevel.OK;

                case "optimise":
                    Optimise(args, handler);
                    return ErrorLevel.OK;

                case "verify":
                    Verify(args, handler);
                    return ErrorLevel.OK;

                default:
                    Log.Error("Unknown command");
                    return ErrorLevel.NotSupported;
            }
        }
        #endregion

        //--------------------//

        #region Execute helpers
        private static ErrorLevel Add(IList<string> args, IIOHandler handler)
        {
            if (args.Count < 3 || args.Count > 4) throw new ArgumentException(string.Format(Resources.WrongNoArguments, UsageAdd));
            var manifestDigest = new ManifestDigest(args[1]);
            string path = args[2];
            string subDir = (args.Count == 4) ? args[3] : null;

            if (Directory.Exists(path)) StoreProvider.Default.AddDirectory(path, manifestDigest, handler);
            else if (File.Exists(path)) StoreProvider.Default.AddArchive(new ArchiveFileInfo { Path = path, SubDir = subDir }, manifestDigest, handler);
            else
            {
                Log.Error("No such file or directory: " + path);
                return ErrorLevel.IOError;
            }
            return ErrorLevel.OK;
        }

        private static void Copy(IList<string> args, IIOHandler handler)
        {
            if (args.Count < 2 || args.Count > 3) throw new ArgumentException(string.Format(Resources.WrongNoArguments, UsageCopy));

            IStore store = (args.Count == 3) ? new DirectoryStore(args[2]) : StoreProvider.Default;
            store.AddDirectory(args[1], new ManifestDigest(Path.GetFileName(args[1])), handler);
        }

        private static void Find(IList<string> args)
        {
            if (args.Count != 2) throw new ArgumentException(string.Format(Resources.WrongNoArguments, UsageFind));

            Console.WriteLine(StoreProvider.Default.GetPath(new ManifestDigest(args[1])));
        }

        private static void Remove(IList<string> args)
        {
            if (args.Count < 2) throw new ArgumentException(string.Format(Resources.WrongNoArguments, UsageRemove));

            for (int i = 1; i < args.Count; i++)
            {
                StoreProvider.Default.Remove(new ManifestDigest(args[i]));
                Log.Info("Successfully removed " + args[i]);
            }
        }

        private static void List(IList<string> args)
        {
            if (args.Count != 1) throw new ArgumentException(string.Format(Resources.WrongNoArguments, UsageList));

            foreach (ManifestDigest digest in StoreProvider.Default.ListAll())
                Console.WriteLine(digest.BestDigest);
        }

        private static void Optimise(IList<string> args, IIOHandler handler)
        {
            if (args.Count == 1) StoreProvider.Default.Optimise(handler);
            else
            {
                for (int i = 1; i < args.Count; i++)
                    new DirectoryStore(args[i]).Optimise(handler);
            }
        }

        private static void Verify(IList<string> args, IIOHandler handler)
        {
            if (args.Count < 2) throw new ArgumentException(string.Format(Resources.WrongNoArguments, UsageVerify));
            for (int i = 1; i < args.Count; i++)
            {
                if (Directory.Exists(args[i]))
                { // Verify an arbitrary directory
                    DirectoryStore.VerifyDirectory(args[i], new ManifestDigest(Path.GetFileName(args[i])), handler);
                }
                else
                { // Verify a directory inside the default store
                    StoreProvider.Default.Verify(new ManifestDigest(args[i]), handler);
                }
                Console.WriteLine("OK");
            }
        }
        #endregion

        #region Audit
        private static ErrorLevel Audit(IList<string> args, IIOHandler handler)
        {
            ErrorLevel result = ErrorLevel.OK;
            if (args.Count == 1) AuditStore(StoreProvider.Default, handler);
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
        private static ErrorLevel AuditStore(IStore store, IIOHandler handler)
        {
            var problems = store.Audit(handler);
            if (problems == null)
            {
                Log.Error("This store does not support auditing.");
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
                Log.Warn("One or more problems were found in the store.");
                return ErrorLevel.DigestMismatch;
            }

            Log.Info("No problems were found in the store.");
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
        /// <exception cref="IOException">Thrown if the directory could not be processed.</exception>
        private static void GenerateManifest(IList<string> args, IIOHandler handler)
        {
            if (args.Count < 2 || args.Count > 3) throw new ArgumentException(string.Format(Resources.WrongNoArguments, UsageManifest));

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
                    format = EnumUtils.GetFirst(ManifestFormat.Recommended);
                }
            }

            string path = args[1];
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("Directory not found: " + path);

            var manifest = Manifest.Generate(path, format, handler);
            Console.Write(manifest);
            Console.WriteLine(manifest.CalculateDigest());
        }
        #endregion
    }
}