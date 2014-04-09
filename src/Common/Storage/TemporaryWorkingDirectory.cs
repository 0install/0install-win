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

namespace NanoByte.Common.Storage
{
    /// <summary>
    /// Like <see cref="TemporaryDirectory"/> but also sets <see cref="Environment.CurrentDirectory"/> to <see cref="TemporaryDirectory.Path"/>.
    /// </summary>
    public sealed class TemporaryWorkingDirectory : TemporaryDirectory
    {
        #region Variables
        private readonly string _oldWorkingDir;
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public TemporaryWorkingDirectory(string prefix) : base(prefix)
        {
            // Remember the current working directory for later restoration and then change it
            _oldWorkingDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path;
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            // Restore the original working directory
            if (disposing) Environment.CurrentDirectory = _oldWorkingDir;

            base.Dispose(disposing);
        }
        #endregion
    }
}
