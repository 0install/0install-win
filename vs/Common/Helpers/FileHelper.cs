/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Common.Properties;

namespace Common.Helpers
{
    /// <summary>
    /// Provides additional or simplified string functions
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Computes the hash value of the content of a file.
        /// </summary>
        /// <param name="path">The path of the file to hash.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <returns>A hexadecimal string representation of the hash value.</returns>
        public static string ComputeHash(string path, HashAlgorithm algorithm)
        {
            #region Sanity checks
            if (path == null) throw new ArgumentNullException("path");
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            #endregion

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                return ComputeHash(stream, algorithm);
        }

        /// <summary>
        /// Computes the hash value of the content of a stream.
        /// </summary>
        /// <param name="stream">The stream containing the data to hash.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <returns>A hexadecimal string representation of the hash value.</returns>
        public static string ComputeHash(Stream stream, HashAlgorithm algorithm)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            #endregion
            
            return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Creates a uniquely named, empty temporary directory on disk and returns the full path of that directory.
        /// </summary>
        /// <returns>The full path of the temporary directory.</returns>
        /// <exception cref="IOException">Thrown if an IO error occurred, such as no unique temporary directory name is available.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Delivers a new value on each call")]
        public static string GetTempDirectory()
        {
            string tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        /// <summary>
        /// Finds a unique random file or directory name for a new entry inside an existing folder.
        /// </summary>
        /// <param name="path">The path of the existing folder inside which the new entry is to be created.</param>
        /// <returns>The complete path for the new file or directory.</returns>
        public static string GetUniqueFileName(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            string uniquePath;
            do uniquePath = Path.Combine(path, Path.GetRandomFileName());
            while (File.Exists(uniquePath) || Directory.Exists(uniquePath));
            return uniquePath;
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> into the number of seconds since the Unix epoch (1970-1-1).
        /// </summary>
        public static long UnixTime(DateTime time)
        {
            TimeSpan timepan = (time - new DateTime(1970, 1, 1));
            return (long)timepan.TotalSeconds;
        }

        /// <summary>
        /// Copies the content of a directory to a new location.
        /// </summary>
        /// <param name="source">The path of source directory. Must exist!</param>
        /// <param name="destination">The path of the target directory. Must not exist!</param>
        /// <exception cref="IOException">Thrown if <paramref name="source"/> does not exist or if <see cref="destination"/> already exists.</exception>
        public static void CopyDirectory(string source, string destination)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            if (!Directory.Exists(source)) throw new IOException(Resources.SourceDirNotExist);
            if (Directory.Exists(destination)) throw new IOException(Resources.DestinationDirExist);
            #endregion

            Directory.CreateDirectory(destination);
            foreach (string entry in Directory.GetFileSystemEntries(source))
            {
                if (Directory.Exists(entry))
                {
                    // Recurse into sub-direcories
                    CopyDirectory(entry, Path.Combine(destination, Path.GetFileName(entry)));
                }
                else
                {
                    // Copy individual files
                    File.Copy(entry, Path.Combine(destination, Path.GetFileName(entry)));
                }
            }
        }

        /// <summary>
        /// Uses whatever means the current platform provides to prevent further write access to a directory (read-only attribute, ACLs, Unix octals, etc.).
        /// </summary>
        /// <remarks>May do nothing if the platform doesn't provide any known protection mechanisms.</remarks>
        /// <param name="path">The directory to protect.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to apply the write protection.</exception>
        public static void WriteProtection(string path)
        {
            var dirInfo = new DirectoryInfo(path);

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                    // ToDo: Rund for each contained file: "fileInfo.Attributes |= FileAttributes.ReadOnly;"
                    break;

                case PlatformID.Win32NT:
                    DirectorySecurity security = dirInfo.GetAccessControl();
                    security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny));
                    dirInfo.SetAccessControl(security);
                    break;

                case PlatformID.Unix:
                    // ToDo: Set Unix octals
                    break;
            }
        }
    }
}
