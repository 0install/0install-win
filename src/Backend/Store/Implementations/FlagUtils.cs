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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using NanoByte.Common.Storage;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Some file flags (executable, symlink, etc.) cannot be stored directly as filesystem attributes on some platforms (e.g. Windows). They can be kept track of in external "flag files" instead.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
    public static class FlagUtils
    {
        #region Read
        /// <summary>
        /// Retrieves a list of files for which an external flag is set.
        /// </summary>
        /// <param name="name">The name of the flag type to search for (<code>.xbit</code> or <code>.symlink</code>).</param>
        /// <param name="target">The target directory to start the search from (will go upwards through directory levels one-by-one, thus may deliver "too many" results).</param>
        /// <returns>A list of fully qualified paths of files that are named in an external flag file.</returns>
        /// <exception cref="IOException">There was an error reading the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to read the flag file.</exception>
        /// <remarks>The flag file is searched for instead of specifiying it directly to allow handling of special cases like creating manifests of subdirectories of extracted archives.</remarks>
        public static ICollection<string> GetFiles(string name, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            string flagDir = FindRootDir(name, target);
            if (flagDir == null) return new string[0];

            var externalFlags = new List<string>();
            using (StreamReader flagFile = File.OpenText(Path.Combine(flagDir, name)))
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
        /// <param name="flagName">The name of the flag type to search for (<code>.xbit</code> or <code>.symlink</code>).</param>
        /// <param name="target">The target directory to start the search from.</param>
        /// <returns>The full path to the closest flag file that was found; <see langword="null"/> if none was found.</returns>
        private static string FindRootDir(string flagName, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(flagName)) throw new ArgumentNullException("flagName");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
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
        /// Sets a flag for a file in an external flag file.
        /// </summary>
        /// <param name="file">The path to the flag file, ending with the type in the type of flag to store (<code>.xbit</code> or <code>.symlink</code>).</param>
        /// <param name="relativePath">The path of the file to set relative to <paramref name="file"/>.</param>
        /// <exception cref="IOException">There was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to write the flag file.</exception>
        public static void Set(string file, string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (Path.IsPathRooted(relativePath)) throw new ArgumentException(Resources.PathNotRelative, "relativePath");
            #endregion

            // Convert path to rooted Unix-style
            string unixPath = "/" + relativePath.Replace(Path.DirectorySeparatorChar, '/');

            using (var flagFile = new StreamWriter(file, append: true, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) {NewLine = "\n"})
                flagFile.WriteLine(unixPath);
        }

        /// <summary>
        /// Removes one or more flags for a file or directory in an external flag file.
        /// </summary>
        /// <param name="file">The path to the flag file, ending with the type in the type of flag to store (<code>.xbit</code> or <code>.symlink</code>).</param>
        /// <param name="relativePath">The path of the file or directory to remove relative to <paramref name="file"/>.</param>
        /// <exception cref="IOException">There was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to write the flag file.</exception>
        public static void Remove(string file, string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (Path.IsPathRooted(relativePath)) throw new ArgumentException(Resources.PathNotRelative, "relativePath");
            #endregion

            if (!File.Exists(file)) return;

            // Convert path to rooted Unix-style
            string unixPath = "/" + relativePath.Replace(Path.DirectorySeparatorChar, '/');

            using (var atomic = new AtomicWrite(file))
            using (var newFlagFile = new StreamWriter(atomic.WritePath, append: false, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) {NewLine = "\n"})
            using (var oldFlagFile = File.OpenText(file))
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
        /// <param name="file">The path to the flag file.</param>
        /// <param name="source">The old path of the renamed file or directory relative to <paramref name="file"/>.</param>
        /// <param name="destination">The new path of the renamed file or directory relative to <paramref name="file"/>.</param>
        /// <exception cref="IOException">There was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">You have insufficient rights to write the flag file.</exception>
        public static void Rename(string file, string source, string destination)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (Path.IsPathRooted(source)) throw new ArgumentException(Resources.PathNotRelative, "source");
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            if (Path.IsPathRooted(destination)) throw new ArgumentException(Resources.PathNotRelative, "destination");
            #endregion

            if (!File.Exists(file)) return;

            // Convert paths to rooted Unix-style
            source = "/" + source.Replace(Path.DirectorySeparatorChar, '/');
            destination = "/" + destination.Replace(Path.DirectorySeparatorChar, '/');

            using (var atomic = new AtomicWrite(file))
            using (var newFlagFile = new StreamWriter(atomic.WritePath, append: false, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) { NewLine = "\n" })
            using (var oldFlagFile = File.OpenText(file))
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
    }
}
