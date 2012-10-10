/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using Common.Utils;

namespace Common.Storage
{
    /// <summary>
    /// Provides a temporary path to write to and atomically inserts it at the target location on disposal.
    /// </summary>
    public sealed class AtomicWrite : IDisposable
    {
        #region Properties
        /// <summary>
        /// The file path of the final target.
        /// </summary>
        public string TargetPath { get; private set; }

        /// <summary>
        /// The temporary file path to write to.
        /// </summary>
        public string WritePath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares a atomic write operation.
        /// </summary>
        /// <param name="path">The file path of the final target.</param>
        public AtomicWrite(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            TargetPath = path;

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Prepend random string for temp file name
            WritePath = directory + Path.DirectorySeparatorChar + "temp." + Path.GetRandomFileName() + "." + Path.GetFileName(path);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Replaces <see cref="TargetPath"/> with the contents of <see cref="WritePath"/>.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (File.Exists(WritePath)) FileUtils.Replace(WritePath, TargetPath);
            }
            finally
            {
                if (File.Exists(WritePath)) File.Delete(WritePath);
            }
        }
        #endregion
    }
}
