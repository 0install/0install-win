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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
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
        /// <exception cref="IOException">Thrown if there was an error reading the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to read the flag file.</exception>
        /// <remarks>The flag file is searched for instead of specifiying it directly to allow handling of special cases like creating manifests of subdirectories of extracted archives.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public static ICollection<string> GetExternalFlags(string name, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            string flagDir = FindFlagDir(name, target);
            if (flagDir == null) return new string[0];

            var externalFlags = new C5.HashSet<string>();
            using (StreamReader flagFile = File.OpenText(Path.Combine(flagDir, name)))
            {
                // Each line in the file signals a flagged file
                while (!flagFile.EndOfStream)
                {
                    string currentLine = flagFile.ReadLine();
                    if (currentLine != null && currentLine.StartsWith("/"))
                    {
                        // Trim away the first slash and then replace Unix-style slashes
                        string relativePath = FileUtils.UnifySlashes(currentLine.Substring(1));
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
        private static string FindFlagDir(string flagName, string target)
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
        /// <param name="file">The path to the flag file ending in the type of flag to store (<code>.xbit</code> or <code>.symlink</code>).</param>
        /// <param name="relativePath">The path of the file to set the flag for relative to <paramref name="file"/>.</param>
        /// <exception cref="IOException">Thrown if there was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to write the flag file.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public static void SetExternalFlag(string file, string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (Path.IsPathRooted(relativePath)) throw new ArgumentException(Resources.PathNotRelative, "relativePath");
            #endregion

            // Convert path to rooted Unix-style
            relativePath = "/" + relativePath.Replace(Path.DirectorySeparatorChar, '/');

            // Use default encoding: UTF-8 without BOM
            using (var xbitWriter = File.AppendText(file))
            {
                xbitWriter.NewLine = "\n";
                xbitWriter.WriteLine(relativePath);
            }
        }

        /// <summary>
        /// Removes a flag for a file in an external flag file.
        /// </summary>
        /// <param name="file">The path to the flag file ending in the type of flag to store (<code>.xbit</code> or <code>.symlink</code>).</param>
        /// <param name="relativePath">The path of the file to remove the flag for relative to <paramref name="file"/>.</param>
        /// <exception cref="IOException">Thrown if there was an error writing the flag file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to write the flag file.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public static void RemoveExternalFlag(string file, string relativePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException("relativePath");
            if (Path.IsPathRooted(relativePath)) throw new ArgumentException(Resources.PathNotRelative, "relativePath");
            #endregion

            if (!File.Exists(file)) return;

            // Convert path to rooted Unix-style
            relativePath = "/" + relativePath.Replace(Path.DirectorySeparatorChar, '/');

            // Read the entire file, remove any matching lines and then rewrite the file
            string xbitFileContent = File.ReadAllText(file);
            xbitFileContent = xbitFileContent.Replace(relativePath + "\n", "");
            File.WriteAllText(file, xbitFileContent, new UTF8Encoding(false));
        }
        #endregion
    }
}
