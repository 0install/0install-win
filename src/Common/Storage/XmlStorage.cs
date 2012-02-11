/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Common.Streams;
using Common.Utils;
using Common.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Storage
{
    /// <summary>
    /// Provides easy serialization to XML files (optionally wrapped in ZIP archives).
    /// </summary>
    /// <remarks>This class only serializes public properties.</remarks>
    public static class XmlStorage
    {
        #region Load plain
        /// <summary>
        /// Loads an object from an XML file.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="stream">The stream to read the encoded XML data from.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T Load<T>(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stream);
            }
                #region Error handling
            catch (InvalidOperationException ex)
            {
                // Convert exception type
                throw new InvalidDataException(ex.Message, ex.InnerException) {Source = ex.Source};
            }
            #endregion
        }

        /// <summary>
        /// Loads an object from an XML file.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="path">The path of the file to load.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T Load<T>(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            try
            {
                using (var fileStream = File.OpenRead(path))
                    return Load<T>(fileStream);
            }
                #region Error handling
            catch (InvalidDataException ex)
            {
                // Change exception message to add context information
                throw new InvalidDataException(string.Format(Resources.ProblemLoading, path) + "\n" + ex.Message, ex.InnerException);
            }
            #endregion
        }

        /// <summary>
        /// Loads an object from an XML string.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="data">The XML string to be parsed.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromString<T>(string data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            // Copy string to a stream and then parse
            using (var stream = StreamUtils.CreateFromString(data))
                return Load<T>(stream);
        }
        #endregion

        #region Save plain
        /// <summary>
        /// Saves an object in an XML stream ending with a line break.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
        /// <param name="stream">The stream to write the encoded XML data to.</param>
        /// <param name="data">The object to be stored.</param>
        public static void Save<T>(Stream stream, T data)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings {Encoding = new UTF8Encoding(false), Indent = true, IndentChars = "\t", NewLineChars = "\n"});
            var serializer = new XmlSerializer(typeof(T));

            // Detect XmlRoot attribute
            var rootAttributes = typeof(T).GetCustomAttributes(typeof(XmlRootAttribute), true);

            if (rootAttributes.Length == 0)
            { // Use default serializer namespaces (XMLSchema)
                serializer.Serialize(xmlWriter, data);
            }
            else
            { // Set custom namespace
                var ns = new XmlSerializerNamespaces(new[] {new XmlQualifiedName("", ((XmlRootAttribute)rootAttributes[0]).Namespace)});
                serializer.Serialize(xmlWriter, data, ns);
            }

            // End file with line break
            if (xmlWriter.Settings != null)
            {
                var newLine = xmlWriter.Settings.Encoding.GetBytes(xmlWriter.Settings.NewLineChars);
                stream.Write(newLine, 0, newLine.Length);
            }
        }

        /// <summary>
        /// Saves an object in an XML file ending with a line break.
        /// </summary>
        /// <remarks>This method performs an atomic write operation when possible.</remarks>
        /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
        /// <param name="path">The path of the file to write.</param>
        /// <param name="data">The object to be stored.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public static void Save<T>(string path, T data)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Prepend random string for temp file name
            string tempPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + "temp." + Path.GetRandomFileName() + "." + Path.GetFileName(path);

            try
            {
                // Write to temporary file first
                using (var fileStream = File.Create(tempPath))
                    Save(fileStream, data);
                FileUtils.Replace(tempPath, path);
            }
                #region Error handling
            catch (Exception)
            {
                // Clean up failed transactions
                if (File.Exists(tempPath)) File.Delete(tempPath);
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Returns an object as an XML string ending with a line break.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
        /// <param name="data">The object to be stored.</param>
        /// <returns>A string containing the XML code.</returns>
        public static string ToString<T>(T data)
        {
            using (var stream = new MemoryStream())
            {
                // Write to a memory stream
                Save(stream, data);

                // Copy the stream to a string
                return StreamUtils.ReadToString(stream);
            }
        }
        #endregion

        #region Stylesheet
        /// <summary>
        /// Adds an XSL stylesheet instruction to an existing XML file.
        /// </summary>
        /// <param name="path">The XML file to add the stylesheet instruction to.</param>
        /// <param name="stylesheetFile">The file name of the stylesheet to reference.</param>
        /// <exception cref="FileNotFoundException">Thrown if the XML file to add the stylesheet reference to could not be found.</exception>
        /// <exception cref="IOException">Thrown if the XML file to add the stylesheet reference to could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the XML file is not permitted.</exception>
        public static void AddStylesheet(string path, string stylesheetFile)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(stylesheetFile)) throw new ArgumentNullException("stylesheetFile");
            #endregion

            // Loads the XML document
            var dom = new XmlDocument();
            dom.Load(path);

            // Adds a new XSL stylesheet instruction to the DOM
            var stylesheetInstruction = dom.CreateProcessingInstruction("xml-stylesheet", string.Format(CultureInfo.InvariantCulture, "type='text/xsl' href='{0}'", stylesheetFile));
            dom.InsertAfter(stylesheetInstruction, dom.FirstChild);

            // Writes back the modified XML document
            using (var xmlWriter = XmlWriter.Create(path, new XmlWriterSettings {Encoding = new UTF8Encoding(false), Indent = true, IndentChars = "\t", NewLineChars = "\n"}))
            {
                dom.WriteTo(xmlWriter);

                // End file with line break
                if (xmlWriter.Settings != null) xmlWriter.WriteWhitespace(xmlWriter.Settings.NewLineChars);
            }
        }
        #endregion

        //--------------------//

        #region Load ZIP
        /// <summary>
        /// Loads an object from an XML file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="stream">The ZIP archive to load.</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files stored alongside the XML file in the ZIP archive to be read; may be <see langword="null"/>.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="ZipException">Thrown if a problem occurred while reading the ZIP data or if <paramref name="password"/> is wrong.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromZip<T>(Stream stream, string password, IEnumerable<EmbeddedFile> additionalFiles)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            bool xmlFound = false;
            T output = default(T);

            using (var zipFile = new ZipFile(stream) {IsStreamOwner = false})
            {
                zipFile.Password = password;

                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (StringUtils.Compare(zipEntry.Name, "data.xml"))
                    {
                        // Read the XML file from the ZIP archive
                        var inputStream = zipFile.GetInputStream(zipEntry);
                        output = Load<T>(inputStream);
                        xmlFound = true;
                    }
                    else
                    {
                        if (additionalFiles != null)
                        {
                            // Read additional files from the ZIP archive
                            foreach (EmbeddedFile file in additionalFiles)
                            {
                                if (StringUtils.Compare(zipEntry.Name, file.Filename))
                                {
                                    var inputStream = zipFile.GetInputStream(zipEntry);
                                    file.StreamDelegate(inputStream);
                                }
                            }
                        }
                    }
                }
            }

            if (xmlFound) return output;
            throw new InvalidDataException(Resources.NoXmlDataInZip);
        }

        /// <summary>
        /// Loads an object from an XML file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="path">The ZIP archive to load.</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files stored alongside the XML file in the ZIP archive to be read; may be <see langword="null"/>.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="ZipException">Thrown if a problem occurred while reading the ZIP data or if <paramref name="password"/> is wrong.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromZip<T>(string path, string password, IEnumerable<EmbeddedFile> additionalFiles)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.OpenRead(path))
                return FromZip<T>(fileStream, password, additionalFiles);
        }
        #endregion

        #region Save ZIP
        /// <summary>
        /// Saves an object in an XML file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
        /// <param name="stream">The ZIP archive to be written.</param>
        /// <param name="data">The object to be stored.</param>
        /// <param name="password">The password to use for encryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files to be stored alongside the XML file in the ZIP archive; may be <see langword="null"/>.</param>
        public static void ToZip<T>(Stream stream, T data, string password, IEnumerable<EmbeddedFile> additionalFiles)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var zipStream = new ZipOutputStream(stream) {IsStreamOwner = false})
            {
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

                // Write the XML file to the ZIP archive
                {
                    var entry = new ZipEntry("data.xml") {DateTime = DateTime.Now};
                    if (!string.IsNullOrEmpty(password)) entry.AESKeySize = 128;
                    zipStream.SetLevel(9);
                    zipStream.PutNextEntry(entry);
                    Save(zipStream, data);
                    zipStream.CloseEntry();
                }

                // Write additional files to the ZIP archive
                if (additionalFiles != null)
                {
                    foreach (EmbeddedFile file in additionalFiles)
                    {
                        var entry = new ZipEntry(file.Filename) {DateTime = DateTime.Now};
                        if (!string.IsNullOrEmpty(password)) entry.AESKeySize = 128;
                        zipStream.SetLevel(file.CompressionLevel);
                        zipStream.PutNextEntry(entry);
                        file.StreamDelegate(zipStream);
                        zipStream.CloseEntry();
                    }
                }
            }
        }

        /// <summary>
        /// Saves an object in an XML file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
        /// <param name="path">The ZIP archive to be written.</param>
        /// <param name="data">The object to be stored.</param>
        /// <param name="password">The password to use for encryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files to be stored alongside the XML file in the ZIP archive; may be <see langword="null"/>.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public static void ToZip<T>(string path, T data, string password, IEnumerable<EmbeddedFile> additionalFiles)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(path);
            if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            using (var fileStream = File.Create(path))
                ToZip(fileStream, data, password, additionalFiles);
        }
        #endregion

        //--------------------//

        #region Embedded files
        /// <summary>
        /// Returns a stream containing a file embedded into a XML-ZIP archive.
        /// </summary>
        /// <param name="stream">The ZIP archive to be loaded.</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption.</param>
        /// <param name="name">The name of the embedded file.</param>
        /// <returns>A stream containing the embedded file.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="ZipException">Thrown if a problem occurred while reading the ZIP data.</exception>
        public static Stream GetEmbeddedFileStream(Stream stream, string password, string name)
        {
            using (var zipFile = new ZipFile(stream))
            {
                zipFile.Password = password;
                return zipFile.GetInputStream(zipFile.GetEntry(name));
            }
        }
        #endregion
    }
}
