/*
 * Copyright 2010 Roland Leopold Walkling, Bastian Eicher
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

namespace Common.Storage
{
    /// <summary>
    /// Disposable class that allows operating on a newly created directory at a specified path, and if necessary temporarily moving the original directory to a new position.
    /// </summary>
    public sealed class TemporaryDirectoryReplacement : IDisposable
    {
        #region Variables
        private readonly TemporaryDirectory _tempDir;
        private readonly TemporaryDirectoryMove _backupMove;
        #endregion

        #region Properties
        /// <summary>
        /// The path of the temporary directory.
        /// </summary>
        public string Path
        {
            get { return _tempDir.Path; }
        }

        /// <summary>
        /// The path were the original directory is moved to.
        /// </summary>
        public string BackupPath
        {
            get { return _backupMove.BackupPath; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Applies <see cref="TemporaryDirectoryMove"/> and <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="path">The path of the temporary directory.</param>
        public TemporaryDirectoryReplacement(string path)
        {
            _backupMove = new TemporaryDirectoryMove(path);
            _tempDir = new TemporaryDirectory(path);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Applies the Dispose methods of <see cref="TemporaryDirectoryMove"/> and <see cref="TemporaryDirectory"/>.
        /// </summary>
        public void Dispose()
        {
            _tempDir.Dispose();
            _backupMove.Dispose();
        }
        #endregion
    }
}
