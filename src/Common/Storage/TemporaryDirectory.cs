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
using System.IO;
using Common.Utils;

namespace Common.Storage
{
    /// <summary>
    /// Disposable class to create a temporary directory and delete it again when disposed.
    /// </summary>
    public class TemporaryDirectory : IDisposable
    {
        #region Variables
        private string _oldWorkingDir;
        #endregion

        #region Properties
        /// <summary>
        /// The fully qualified path of the temporary directory.
        /// </summary>
        public string Path { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a uniquely named, empty temporary directory on disk.
        /// </summary>
        /// <param name="prefix">A short string the directory name should start with.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory in <see cref="System.IO.Path.GetTempPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory in <see cref="System.IO.Path.GetTempPath"/> is not permitted.</exception>
        public TemporaryDirectory(string prefix) : this(prefix, false)
        {}

        /// <summary>
        /// Creates a uniquely named, empty temporary directory on disk.
        /// </summary>
        /// <param name="prefix">A short string the directory name should start with.</param>
        /// <param name="makeWorkingDir">Set to <see langword="true"/> to make the new directory the <see cref="Environment.CurrentDirectory"/> until <see cref="Dispose"/> is called.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory in <see cref="System.IO.Path.GetTempPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory in <see cref="System.IO.Path.GetTempPath"/> is not permitted.</exception>
        public TemporaryDirectory(string prefix, bool makeWorkingDir)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException("prefix");
            #endregion

            Path = FileUtils.GetTempDirectory(prefix);

            if (makeWorkingDir)
            {
                // Remember the current working directory for later restoration and then change it
                _oldWorkingDir = Environment.CurrentDirectory;
                Environment.CurrentDirectory = Path;
            }
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Deletes the temporary directory.
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

            // Restore the original working directory if it was changed
            if (_oldWorkingDir != null) Environment.CurrentDirectory = _oldWorkingDir;

            if (Directory.Exists(Path))
            {
#if FS_SECURITY
                // Write protection might prevent a directory from being deleted (especially on Unixoid systems)
                try
                {
                    FileUtils.DisableWriteProtection(Path);
                }
                catch (IOException)
                {}
                catch (UnauthorizedAccessException)
                { }
#endif

                Directory.Delete(Path, true);
            }
        }
        #endregion
    }
}
