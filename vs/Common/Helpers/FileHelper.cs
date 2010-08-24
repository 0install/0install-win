/*
 * Copyright 2006-2010 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Security.Principal;
using Common.Properties;

namespace Common.Helpers
{
    /// <summary>
    /// Provides filesystem-related helper methods.
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
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "The returned characters are only 0-9 and A-F")]
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
        /// Converts a <see cref="DateTime"/> into the number of seconds since the Unix epoch (1970-1-1).
        /// </summary>
        public static long UnixTime(DateTime time)
        {
            TimeSpan timepan = (time - new DateTime(1970, 1, 1));
            return (long)timepan.TotalSeconds;
        }

        /// <summary>
        /// Copies the content of a directory to a new location preserving the original file and directory modification times.
        /// </summary>
        /// <param name="sourcePath">The path of source directory. Must exist!</param>
        /// <param name="destinationPath">The path of the target directory. Must not exist!</param>
        /// <param name="overwrite">Overwrite exisiting files and directories at the <paramref name="destinationPath"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sourcePath"/> and <paramref name="destinationPath"/> are equal.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="sourcePath"/> does not exist.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="destinationPath"/> already exists and <paramref name="overwrite"/> is <see langword="false"/>.</exception>
        public static void CopyDirectory(string sourcePath, string destinationPath, bool overwrite)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException("destinationPath");
            if (sourcePath == destinationPath) throw new ArgumentException(Resources.SourceDestinationEqual);
            if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException(Resources.SourceDirNotExist);
            if (!overwrite && Directory.Exists(destinationPath)) throw new IOException(Resources.DestinationDirExist);
            #endregion

            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

            foreach (string entry in Directory.GetFileSystemEntries(sourcePath))
            {
                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(entry));
                if (Directory.Exists(entry))
                {
                    // Recurse into sub-direcories
                    CopyDirectory(entry, destinationFilePath, overwrite);
                }
                else
                {
                    // Copy individual files
                    File.Copy(entry, destinationFilePath, overwrite);
                }
            }

            // Set directory write time as last step, since file changes within the dir will reset the value
            Directory.SetLastWriteTimeUtc(destinationPath, Directory.GetLastWriteTimeUtc(sourcePath));
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
                {
                    // ToDo: Run for each contained file: "fileInfo.Attributes |= FileAttributes.ReadOnly;"
                    break;
                }

                case PlatformID.Win32NT:
                {
                    DirectorySecurity security = dirInfo.GetAccessControl();
                    security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-1-0"), FileSystemRights.Write, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny));
                    dirInfo.SetAccessControl(security);
                    break;
                }

                case PlatformID.Unix:
                case PlatformID.MacOSX:
                {
                    // ToDo: Set Unix octals
                    break;
                }
            }
        }

        /// <summary>
        /// Determines the accuracy with which the filesystem underlying a specific directory can store file-changed times.
        /// </summary>
        /// <param name="path">The path of the directory to check.</param>
        /// <returns>The accuracy in number of seconds. (i.e. 0 = perfect, 1 = may be off by up to one second)</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified directory doesn't exist.</exception>
        /// <exception cref="IOException">Thrown if writing to the directory fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to write to the directory.</exception>
        public static int DetermineTimeAccuracy(string path)
        {
            // Prepare a file name and fake change time
            var referenceTime = new DateTime(2000, 1, 1, 0, 0, 1); // 1 second past mid-night on 1st of January 2000
            string tempFile = Path.Combine(path, Path.GetRandomFileName());

            File.WriteAllText(tempFile, @"a");
            File.SetLastWriteTimeUtc(tempFile, referenceTime);
            var resultTime = File.GetLastWriteTimeUtc(tempFile);
            File.Delete(tempFile);

            return Math.Abs((resultTime - referenceTime).Seconds);
        }
    }
}
