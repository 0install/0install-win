/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common.Utils;

namespace NanoByte.Common.Storage
{
    /// <summary>
    /// Disposable class to create a temporary file and delete it again when disposed.
    /// </summary>
    public class TemporaryFile : IDisposable
    {
        #region Properties
        /// <summary>
        /// The fully qualified path of the temporary file.
        /// </summary>
        public string Path { get; private set; }

        public static implicit operator string(TemporaryFile file)
        {
            return (file == null) ? null : file.Path;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a uniquely named, empty temporary file on disk.
        /// </summary>
        /// <param name="prefix">A short string the directory name should start with.</param>
        /// <returns>The full path of the newly created temporary directory.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a file in <see cref="System.IO.Path.GetTempPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a file in <see cref="System.IO.Path.GetTempPath"/> is not permitted.</exception>
        public TemporaryFile(string prefix)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException("prefix");
            #endregion

            Path = FileUtils.GetTempFile(prefix);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Deletes the temporary file.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Do not trigger via GC
            if (!disposing) return;

            if (File.Exists(Path)) File.Delete(Path);
        }
        #endregion
    }
}
