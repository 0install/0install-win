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
using System.IO;
using System.Security.Cryptography;

namespace Common.Helpers
{
    /// <summary>
    /// Provides additional or simplified string functions
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Computes the hash of the content of a file.
        /// </summary>
        /// <param name="path">The path of the file to hash.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <returns>A hexadecimal string representation of the hash value.</returns>
        public static string ComputeHash(string path, HashAlgorithm algorithm)
        {
            byte[] hash;
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                hash = algorithm.ComputeHash(stream);

            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Creates a uniquely named, empty temporary directory on disk and returns the full path of that directory.
        /// </summary>
        /// <returns>The full path of the temporary directory.</returns>
        /// <exception cref="IOException">Thrown if an IO error occurred, such as no unique temporary directory name is available.</exception>
        public static string GetTempDirectory()
        {
            string tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }
    }
}
