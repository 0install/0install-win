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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common.Properties;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Storage
{
    /// <summary>
    /// Provides easy serialization to binary files (optionally wrapped in ZIP archives).
    /// </summary>
    /// <remarks>This class serializes private fields.</remarks>
    public static class BinaryStorage
    {
        private static readonly BinaryFormatter _serializer = new BinaryFormatter();

        //--------------------//

        #region Load plain
        /// <summary>
        /// Loads an object from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object the binary stream shall be converted into.</typeparam>
        /// <param name="stream">The binary file to be loaded.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the binary data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T LoadBinary<T>(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            if (stream.CanSeek) stream.Position = 0;
            try
            {
                return (T)_serializer.Deserialize(stream);
            }
                #region Error handling
            catch (SerializationException ex)
            { // Convert exception type
                throw new InvalidDataException(ex.Message, ex.InnerException) {Source = ex.Source};
            }
            #endregion
        }

        /// <summary>
        /// Loads an object from a binary filen
        /// </summary>
        /// <typeparam name="T">The type of object the binary stream shall be converted into.</typeparam>
        /// <param name="path">The binary file to be loaded.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the binary data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T LoadBinary<T>(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.OpenRead(path))
                return LoadBinary<T>(fileStream);
        }
        #endregion

        #region Save plain
        /// <summary>
        /// Saves an object in a binary stream.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in a binary stream.</typeparam>
        /// <param name="data">The object to be stored.</param>
        /// <param name="stream">The binary file to be written.</param>
        public static void SaveBinary<T>(this T data, Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            _serializer.Serialize(stream, data);
        }

        /// <summary>
        /// Saves an object in a binary file.
        /// </summary>
        /// <remarks>This method performs an atomic write operation when possible.</remarks>
        /// <typeparam name="T">The type of object to be saved in a binary stream.</typeparam>
        /// <param name="data">The object to be stored.</param>
        /// <param name="path">The binary file to be written.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public static void SaveBinary<T>(this T data, string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var atomic = new AtomicWrite(path))
            using (var fileStream = File.Create(atomic.WritePath))
            {
                SaveBinary(data, fileStream);
                atomic.Commit();
            }
        }
        #endregion

        //--------------------//

        #region Load ZIP
        /// <summary>
        /// Loads an object from a binary file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object the binary stream shall be converted into.</typeparam>
        /// <param name="stream">The ZIP archive to be loaded.</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files stored alongside the binary file in the ZIP archive to be read; may be <see langword="null"/>.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="ZipException">Thrown if a problem occurred while reading the ZIP data or if <paramref name="password"/> is wrong.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the binary data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T LoadBinaryZip<T>(Stream stream, string password, params EmbeddedFile[] additionalFiles)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            bool binaryFound = false;
            T output = default(T);

            if (stream.CanSeek) stream.Position = 0;
            using (var zipFile = new ZipFile(stream) {Password = password})
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (StringUtils.EqualsIgnoreCase(zipEntry.Name, "data"))
                    {
                        // Read the binary file from the ZIP archive
                        var inputStream = zipFile.GetInputStream(zipEntry);
                        output = LoadBinary<T>(inputStream);
                        binaryFound = true;
                    }
                    else
                    {
                        if (additionalFiles != null)
                        {
                            // Read additional files from the ZIP archive
                            foreach (EmbeddedFile file in additionalFiles)
                            {
                                if (StringUtils.EqualsIgnoreCase(zipEntry.Name, file.Filename))
                                {
                                    var inputStream = zipFile.GetInputStream(zipEntry);
                                    file.StreamDelegate(inputStream);
                                }
                            }
                        }
                    }
                }
            }

            if (binaryFound) return output;
            throw new InvalidDataException(Resources.NoBinaryDataInZip);
        }

        /// <summary>
        /// Loads an object from a binary file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object the binary stream shall be converted into.</typeparam>
        /// <param name="path">The ZIP archive to be loaded.</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files stored alongside the binary file in the ZIP archive to be read; may be <see langword="null"/>.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="ZipException">Thrown if a problem occurred while reading the ZIP data or if <paramref name="password"/> is wrong.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the binary data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T LoadBinaryZip<T>(string path, string password, params EmbeddedFile[] additionalFiles)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.OpenRead(path))
                return LoadBinaryZip<T>(fileStream, password, additionalFiles);
        }
        #endregion

        #region Save ZIP
        /// <summary>
        /// Saves an object in a binary file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in a binary stream.</typeparam>
        /// <param name="data">The object to be stored.</param>
        /// <param name="stream">The ZIP archive to be written.</param>
        /// <param name="password">The password to use for encryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files to be stored alongside the binary file in the ZIP archive; may be <see langword="null"/>.</param>
        public static void SaveBinaryZip<T>(this T data, Stream stream, string password, params EmbeddedFile[] additionalFiles)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var zipStream = new ZipOutputStream(stream) {IsStreamOwner = false, Password = password})
            {
                // Write the binary file to the ZIP archive
                {
                    var entry = new ZipEntry("Data") {DateTime = DateTime.Now, AESKeySize = 128};
                    zipStream.SetLevel(9);
                    zipStream.PutNextEntry(entry);
                    SaveBinary(data, zipStream);
                    zipStream.CloseEntry();
                }

                // Write additional files to the ZIP archive
                if (additionalFiles != null)
                {
                    foreach (EmbeddedFile file in additionalFiles)
                    {
                        var entry = new ZipEntry(file.Filename) {DateTime = DateTime.Now, AESKeySize = 128};
                        zipStream.SetLevel(file.CompressionLevel);
                        zipStream.PutNextEntry(entry);
                        file.StreamDelegate(zipStream);
                        zipStream.CloseEntry();
                    }
                }
            }
        }

        /// <summary>
        /// Saves an object in a binary file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in a binary stream.</typeparam>
        /// <param name="data">The object to be stored.</param>
        /// <param name="path">The ZIP archive to be written.</param>
        /// <param name="password">The password to use for encryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files to be stored alongside the binary file in the ZIP archive; may be <see langword="null"/>.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public static void SaveBinaryZip<T>(this T data, string path, string password, params EmbeddedFile[] additionalFiles)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.Create(path))
                SaveBinaryZip(data, fileStream, password, additionalFiles);
        }
        #endregion
    }
}
