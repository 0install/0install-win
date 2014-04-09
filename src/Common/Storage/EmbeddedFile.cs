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
using System.IO;

namespace NanoByte.Common.Storage
{
    /// <summary>
    /// Information about an additional file to be stored along side an ZIP archive using <see cref="XmlStorage"/> or <see cref="BinaryStorage"/>.
    /// </summary>
    public struct EmbeddedFile
    {
        #region Variables
        private readonly string _filename;
        private readonly int _compressionLevel;
        private readonly Action<Stream> _streamDelegate;
        #endregion

        #region Properties
        /// <summary>
        /// The filename in the archive
        /// </summary>
        public string Filename { get { return _filename; } }

        /// <summary>
        /// The level of compression (0-9) to apply to this entry
        /// </summary>
        public int CompressionLevel { get { return _compressionLevel; } }

        /// <summary>
        /// The delegate to be called when the data is ready to be read/written to/form a stream
        /// </summary>
        public Action<Stream> StreamDelegate { get { return _streamDelegate; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new XML ZIP entry for reading
        /// </summary>
        /// <param name="filename">The filename in the archive</param>
        /// <param name="readDelegate">The delegate to be called when the data is ready to be read</param>
        public EmbeddedFile(string filename, Action<Stream> readDelegate)
        {
            _filename = filename;
            _compressionLevel = 0;
            _streamDelegate = readDelegate;
        }

        /// <summary>
        /// Creates a new XML ZIP entry for writing
        /// </summary>
        /// <param name="filename">The filename in the archive</param>
        /// <param name="compressionLevel">The level of compression (0-9) to apply to this entry</param>
        /// <param name="writeDelegate">The delegate to be called when the data is ready to be written</param>
        public EmbeddedFile(string filename, int compressionLevel, Action<Stream> writeDelegate)
        {
            _filename = filename;
            _compressionLevel = compressionLevel;
            _streamDelegate = writeDelegate;
        }
        #endregion
    }
}
