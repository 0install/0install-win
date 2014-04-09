/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NanoByte.Common.Properties;

namespace NanoByte.Common.Cli
{
    /// <summary>
    /// Provides helper methods for for parsing command-line arguments.
    /// </summary>
    public static class ArgumentUtils
    {
        /// <summary>
        /// Parses command-line arguments as file paths including wildcard support.
        /// </summary>
        /// <param name="args">The arguments to parse.</param>
        /// <param name="defaultPattern">The default pattern to use for finding files when a directory is specified.</param>
        /// <returns>Handles to all matching files that were found</returns>
        /// <exception cref="FileNotFoundException">Thrown if a file that was explicitly specified in <paramref name="args"/> (no wildcards) could not be found.</exception>
        /// <remarks><paramref name="args"/> are first interpreted as files, then as directories. Directories are searched using the <paramref name="defaultPattern"/>. * and ? characters are considered as wildcards.</remarks>
        public static IList<FileInfo> GetFiles(IEnumerable<string> args, string defaultPattern)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            var result = new List<FileInfo>();

            foreach (var entry in args)
            {
                if (entry.Contains("*") || entry.Contains("?"))
                {
                    string dewildcardedPath = entry.Replace("*", "x").Replace("?", "x");
                    string directory = Path.GetDirectoryName(Path.GetFullPath(dewildcardedPath)) ?? Environment.CurrentDirectory;
                    string filePattern = Path.GetFileName(entry);
                    if (string.IsNullOrEmpty(filePattern)) filePattern = defaultPattern;
                    result.AddRange(Directory.GetFiles(directory, filePattern).Select(file => new FileInfo(Path.GetFullPath(file))));
                }
                else if (File.Exists(entry)) result.Add(new FileInfo(Path.GetFullPath(entry)));
                else if (Directory.Exists(entry))
                    result.AddRange(Directory.GetFiles(entry, defaultPattern).Select(file => new FileInfo(file)));
                else throw new FileNotFoundException(string.Format(Resources.FileNotFound, entry), entry);
            }

            return result;
        }
    }
}
