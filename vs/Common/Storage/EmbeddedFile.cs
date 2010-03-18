using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Common.Storage
{
    /// <summary>
    /// Information about an additional file to be stored along side an ZIP archive using <see cref="XmlStorage"/> or <see cref="BinaryStorage"/>.
    /// </summary>
    /// <seealso cref="XmlStorage.FromZip{T}(Stream,string,IEnumerable{EmbeddedFile},MemberInfo[])"/>
    /// <seealso cref="BinaryStorage.FromZip{T}(Stream,string,EmbeddedFile[])"/>
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