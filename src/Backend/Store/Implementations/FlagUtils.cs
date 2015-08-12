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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Some file flags (executable, symlink, etc.) cannot be stored directly as filesystem attributes on some platforms (e.g. Windows). They can be kept track of in external "flag files" instead.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
    public static class FlagUtils
    {
        /// <summary>
        /// The well-known file name used for a flag file to indicate that a directory resides on a non-Unix filesystem.
        /// </summary>
        public const string NoUnixFSFile = ".no-unix-fs";

        /// <summary>
        /// The well-known file name used to store executable flags in directories.
        /// </summary>
        public const string XbitFile = ".xbit";

        /// <summary>
        /// The well-known file name used to store symlink flags in directories.
        /// </summary>
        public const string SymlinkFile = ".symlink";

        #region Read
        /// <summary>
        /// Determines whether a directory resides on a non-Unix filesystem.
        /// </summary>
        /// <param name="target">The full path to the directory.</param>
        /// <remarks>The flag file is searched for instead of specifiying it directly to allow handling of special cases like creating manifests of subdirectories of extracted archives.</remarks>
        /// <seealso cref="NoUnixFSFile"/>
        /// <seealso cref="FileUtils.IsUnixFS"/>
        public static bool IsUnixFS([NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            #endregion

            // Move up one level to avoid write-protection within implementation directories
            if (StoreUtils.PathInAStore(target)) target = Path.Combine(target, "..");

            try
            {
                if (FindRootDir(NoUnixFSFile, target) != null) return false;
                else return FileUtils.IsUnixFS(target);
            }
                #region Error handling
            catch (IOException)
            {
                // Just assume the target is a Unix FS if the check fails on a Unixoid OS
                return UnixUtils.IsUnix;
            }
            catch (UnauthorizedAccessException)
            {
                // Just assume the target is a Unix FS if the check fails on a Unixoid OS
                return UnixUtils.IsUnix;
            }
            #endregion
        }

        /// <summary>
        /// Retrieves a list of files for which an external flag is set.
        /// </summary>
        /// <param name="flagName">The name of the flag type to search for (<see cref="FlagUtils.XbitFile"/> or <see cref="FlagUtils.SymlinkFile"/>).</param>
        /// <param name="target">The target directory to start the search from (will go upwards through directory levels one-by-one, thus may deliver "too many" results).</param>
        /// <returns>A list of fully qualified paths of files that are named in an external flag file.</returns>
        /// <exception cref="IOException">There was an error reading the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to read the flag file.</exception>
        /// <remarks>The flag file is searched for instead of specifiying it directly to allow handling of special cases like creating manifests of subdirectories of extracted archives.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flag")]
        [NotNull, ItemNotNull]
        public static ICollection<string> GetFiles([NotNull] string flagName, [NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(flagName)) throw new ArgumentNullException(nameof(flagName));
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            #endregion

            string flagDir = FindRootDir(flagName, target);
            if (flagDir == null) return new string[0];

            var externalFlags = new List<string>();
            using (StreamReader flagFile = File.OpenText(Path.Combine(flagDir, flagName)))
            {
                // Each line in the file signals a flagged file
                while (!flagFile.EndOfStream)
                {
                    string line = flagFile.ReadLine();
                    if (line != null && line.StartsWith("/"))
                    {
                        // Trim away the first slash and then replace Unix-style slashes
                        string relativePath = FileUtils.UnifySlashes(line.Substring(1));
                        externalFlags.Add(Path.Combine(flagDir, relativePath));
                    }
                }
            }
            return externalFlags;
        }

        /// <summary>
        /// Searches for a flag file starting in the <paramref name="target"/> directory and moving upwards until it finds it or until it reaches the root directory.
        /// </summary>
        /// <param name="flagName">The name of the flag type to search for (<see cref="FlagUtils.XbitFile"/> or <see cref="FlagUtils.SymlinkFile"/>).</param>
        /// <param name="target">The target directory to start the search from.</param>
        /// <returns>The full path to the closest flag file that was found; <c>null</c> if none was found.</returns>
        [CanBeNull]
        private static string FindRootDir([NotNull] string flagName, [NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(flagName)) throw new ArgumentNullException(nameof(flagName));
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            #endregion

            // Start searching for the flag file in the target directory and then move upwards
            string flagDir = Path.GetFullPath(target);
            while (!File.Exists(Path.Combine(flagDir, flagName)))
            {
                // Go up one level in the directory hierachy
                flagDir = Path.GetDirectoryName(flagDir);

                // Cancel once the root dir has been reached
                if (flagDir == null) break;
            }

            return flagDir;
        }
        #endregion

        #region Write
        /// <summary>
        /// Sets a flag for a directory indicating that it resides on a non-Unix filesystem. This makes future calls to <see cref="IsUnixFS"/> run faster and more reliable.
        /// </summary>
        /// <param name="target">The full path to the directory.</param>
        /// <exception cref="IOException">There was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to write the flag file.</exception>
        /// <seealso cref="NoUnixFSFile"/>
        public static void MarkAsNoUnixFS([NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            #endregion

            FileUtils.Touch(Path.Combine(target, NoUnixFSFile));
        }

        /// <summary>
        /// Sets a flag for a file in an external flag file.
        /// </summary>
        /// <param name="path">The full path to the flag file, named <see cref="FlagUtils.XbitFile"/> or <see cref="FlagUtils.SymlinkFile"/>.</param>
        /// <param name="relativePath">The path of the file to set relative to <paramref name="path"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="relativePath"/> is not a relative path.</exception>
        /// <exception cref="IOException">There was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to write the flag file.</exception>
        public static void Set([NotNull] string path, [NotNull] string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException(nameof(relativePath));
            if (Path.IsPathRooted(relativePath)) throw new ArgumentException(Resources.PathNotRelative, nameof(relativePath));
            #endregion

            // Convert path to rooted Unix-style
            string unixPath = "/" + relativePath.Replace(Path.DirectorySeparatorChar, '/');

            using (var flagFile = new StreamWriter(path, append: true, encoding: FeedUtils.Encoding) {NewLine = "\n"})
                flagFile.WriteLine(unixPath);
        }

        /// <summary>
        /// Removes one or more flags for a file or directory in an external flag file.
        /// </summary>
        /// <param name="path">The full path to the flag file, named <see cref="FlagUtils.XbitFile"/> or <see cref="FlagUtils.SymlinkFile"/>.</param>
        /// <param name="relativePath">The path of the file or directory to remove relative to <paramref name="path"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="relativePath"/> is not a relative path.</exception>
        /// <exception cref="IOException">There was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to write the flag file.</exception>
        public static void Remove([NotNull] string path, [NotNull] string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException(nameof(relativePath));
            if (Path.IsPathRooted(relativePath)) throw new ArgumentException(Resources.PathNotRelative, nameof(relativePath));
            #endregion

            if (!File.Exists(path)) return;

            // Convert path to rooted Unix-style
            string unixPath = "/" + relativePath.Replace(Path.DirectorySeparatorChar, '/');

            using (var atomic = new AtomicWrite(path))
            using (var newFlagFile = new StreamWriter(atomic.WritePath, append: false, encoding: FeedUtils.Encoding) {NewLine = "\n"})
            using (var oldFlagFile = File.OpenText(path))
            {
                // Each line in the file signals a flagged file
                while (!oldFlagFile.EndOfStream)
                {
                    string line = oldFlagFile.ReadLine();
                    if (line != null && line.StartsWith("/"))
                    {
                        if (line == unixPath || line.StartsWith(unixPath + "/")) continue; // Filter out removed files

                        newFlagFile.WriteLine(line);
                    }
                }
                atomic.Commit();
            }
        }

        /// <summary>
        /// Adds a directory prefix to all entries in an external flag file.
        /// </summary>
        /// <param name="path">The full path to the flag file, named <see cref="FlagUtils.XbitFile"/> or <see cref="FlagUtils.SymlinkFile"/>.</param>
        /// <param name="source">The old path of the renamed file or directory relative to <paramref name="path"/>.</param>
        /// <param name="destination">The new path of the renamed file or directory relative to <paramref name="path"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="source"/> or <paramref name="destination"/> is not a relative path.</exception>
        /// <exception cref="IOException">There was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to write the flag file.</exception>
        public static void Rename([NotNull] string path, [NotNull] string source, [NotNull] string destination)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
            if (Path.IsPathRooted(source)) throw new ArgumentException(Resources.PathNotRelative, nameof(source));
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException(nameof(destination));
            if (Path.IsPathRooted(destination)) throw new ArgumentException(Resources.PathNotRelative, nameof(destination));
            #endregion

            if (!File.Exists(path)) return;

            // Convert paths to rooted Unix-style
            source = "/" + source.Replace(Path.DirectorySeparatorChar, '/');
            destination = "/" + destination.Replace(Path.DirectorySeparatorChar, '/');

            using (var atomic = new AtomicWrite(path))
            using (var newFlagFile = new StreamWriter(atomic.WritePath, append: false, encoding: FeedUtils.Encoding) {NewLine = "\n"})
            using (var oldFlagFile = File.OpenText(path))
            {
                // Each line in the file signals a flagged file
                while (!oldFlagFile.EndOfStream)
                {
                    string line = oldFlagFile.ReadLine();
                    if (line != null && line.StartsWith("/"))
                    {
                        if (line == source || line.StartsWith(source + "/"))
                            newFlagFile.WriteLine(destination + line.Substring(source.Length));
                        else newFlagFile.WriteLine(line);
                    }
                }
                atomic.Commit();
            }
        }
        #endregion

        #region Convert
        /// <summary>
        /// Converts all flag files in a directory into real filesystem attributes (executable bits and symlinks).
        /// </summary>
        /// <param name="path">The path to the directory to convert.</param>
        public static void ConvertToFS([NotNull] string path)
        {
            string xbitFile = Path.Combine(path, XbitFile);
            if (File.Exists(xbitFile))
            {
                foreach (string file in GetFiles(XbitFile, path))
                    FileUtils.SetExecutable(file, executable: true);

                File.Delete(xbitFile);
            }

            string symlinkFile = Path.Combine(path, SymlinkFile);
            if (File.Exists(xbitFile))
            {
                foreach (string file in GetFiles(SymlinkFile, path))
                {
                    string linkDestination = File.ReadAllText(file);
                    File.Delete(file);
                    FileUtils.CreateSymlink(file, linkDestination);
                }
                File.Delete(symlinkFile);
            }
        }
        #endregion
    }
}
