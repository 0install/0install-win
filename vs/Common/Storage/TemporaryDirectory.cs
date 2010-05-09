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
using Common.Helpers;

namespace Common.Storage
{
    /// <summary>
    /// Disposable class to create a temporary folder and delete it again when disposed.
    /// </summary>
    public class TemporaryDirectory : IDisposable
    {
        #region Properties
        /// <summary>
        /// The fully qualified path of the temporary directory.
        /// </summary>
        public string Path { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a temporary directory in the system's temp directory.
        /// </summary>
        public TemporaryDirectory()
        {
            Path = FileHelper.GetTempDirectory();
        }

        /// <summary>
        /// Creates a temporary directory at a given path.
        /// </summary>
        /// <param name="path">Path where the new folder should be created.</param>
        /// <exception cref="IOException">Thrown if a directory already exists at the location specified by <paramref name="path"/>.</exception>
        public TemporaryDirectory(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (Directory.Exists(path)) throw new IOException("Directory already exists.");
            #endregion

            Directory.CreateDirectory(path);
            Path = System.IO.Path.GetFullPath(path);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Deletes the temporary folder.
        /// </summary>
        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
        #endregion
    }
}
