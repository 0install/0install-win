/*
 * Copyright 2006-2011 Bastian Eicher
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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
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
        #region Serializer generation
        /// <summary>An internal cache of XML serializers identified by the target type and ignored sub-types.</summary>
        private static readonly Dictionary<string, XmlSerializer> _serializers = new Dictionary<string, XmlSerializer>();

        /// <summary>Used to mark something as "serialize as XML attribute".</summary>
        private static readonly XmlAttributes _asAttribute = new XmlAttributes {XmlAttribute = new XmlAttributeAttribute()};
        
        /// <summary>
        /// Gets a <see cref="XmlSerializer"/> for classes of the type <paramref name="type"/>. Results are automatically cached internally.
        /// </summary>
        /// <param name="type">The type to get the serializer for.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <returns>The cached or newly created <see cref="XmlSerializer"/>.</returns>
        private static XmlSerializer GetSerializer(Type type, IEnumerable<MemberInfo> ignoreMembers)
        {
            // Create a string key containing the type name and optionally the ignore-type names
            string key = type.FullName ?? "";
            if (ignoreMembers != null)
            {
                foreach (MemberInfo ignoreMember in ignoreMembers)
                {
                    if (ignoreMember != null)
                        key += " \\ " + ignoreMember.ReflectedType.FullName + ignoreMember.Name;
                }
            }

            XmlSerializer serializer;
            // Try to find a suitable serializer in the cache
            if (!_serializers.TryGetValue(key, out serializer))
            {
                // Create a new serializer and add it to the cache
                serializer = CreateSerializer(type, ignoreMembers);
                _serializers.Add(key, serializer);
            }
            
            return serializer;
        }

        /// <summary>
        /// Creates a new <see cref="XmlSerializer"/> for the type <paramref name="type"/> and applies a set of default augmentations for .NET types.
        /// </summary>
        /// <param name="type">The type to create the serializer for.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <returns>The newly created <see cref="XmlSerializer"/>.</returns>
        /// <remarks>This method may be rather slow, so its results should be cached.</remarks>
        private static XmlSerializer CreateSerializer(Type type, IEnumerable<MemberInfo> ignoreMembers)
        {
            var overrides = new XmlAttributeOverrides();
            var ignore = new XmlAttributes {XmlIgnore = true};

            #region Augment .NET BCL types
            MembersAsAttributes<Point>(overrides, "X", "Y");
            MembersAsAttributes<Size>(overrides, "Width", "Height");
            MembersAsAttributes<Rectangle>(overrides, "X", "Y", "Width", "Height");
            overrides.Add(typeof(Rectangle), "Location", ignore);
            overrides.Add(typeof(Rectangle), "Size", ignore);
            overrides.Add(typeof(Exception), "Data", ignore);
            #endregion

            // Ignore specific fields)
            if (ignoreMembers != null)
            {
                foreach (MemberInfo ignoreMember in ignoreMembers)
                {
                    if (ignoreMember != null) overrides.Add(ignoreMember.ReflectedType, ignoreMember.Name, ignore);
                }
            }

            var serializer = new XmlSerializer(type, overrides);
            serializer.UnknownAttribute += (sender, e) => Log.Warn("Ignored XML attribute while deserializing: " + e.Attr);
            serializer.UnknownElement += (sender, e) => Log.Warn("Ignored XML element while deserializing: " + e.Element);
            return serializer;
        }

        /// <summary>
        /// Configures a set of members of a type to be serialized as XML attributes.
        /// </summary>
        private static void MembersAsAttributes<T>(XmlAttributeOverrides overrides, params string[] members)
        {
            Type type = typeof(T);
            foreach (string memeber in members)
                overrides.Add(type, memeber, _asAttribute);
        }
        #endregion

        //--------------------//

        #region Load plain
        /// <summary>
        /// Loads an object from an XML file.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="stream">The XML file to be loaded.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T Load<T>(Stream stream, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            try { return (T)GetSerializer(typeof(T), ignoreMembers).Deserialize(stream); }
            #region Error handling
            catch (InvalidOperationException ex)
            { // Convert exception type
                throw new InvalidDataException(ex.Message, ex.InnerException) {Source = ex.Source};
            }
            #endregion
        }

        /// <summary>
        /// Loads an object from an XML file.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="path">The XML file to be loaded.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T Load<T>(string path, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            try {
                using (var fileStream = File.OpenRead(path))
                    return Load<T>(fileStream, ignoreMembers);
            }
            #region Error handling
            catch (InvalidDataException ex)
            {
                // Write additional diagnostic information to log
                string message = string.Format(Resources.ProblemLoading, path) + "\n" + ex.Message;
                if (ex.InnerException != null) message += "\n" + ex.InnerException.Message;
                Log.Error(message);

                throw;
            }
            #endregion
        }

        /// <summary>
        /// Loads an object from an XML string.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="data">The XML string to be parsed.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromString<T>(string data, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException("data");
            #endregion

            // Copy string to a stream and then parse
            using (var stream = StreamUtils.CreateFromString(data))
                return Load<T>(stream, ignoreMembers);
        }
        #endregion

        #region Save plain
        /// <summary>
        /// Saves an object in an XML stream.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
        /// <param name="stream">The XML file to be written.</param>
        /// <param name="data">The object to be stored.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        public static void Save<T>(Stream stream, T data, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings {Encoding = new UTF8Encoding(false), Indent = true, IndentChars = "\t", NewLineChars = "\n"}))
            {
                var serializer = GetSerializer(typeof(T), ignoreMembers);

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
                
                // End file with newline
                xmlWriter.WriteWhitespace("\n");
            }
        }

        /// <summary>
        /// Saves an object in an XML file.
        /// </summary>
        /// <remarks>This method performs an atomic write operation when possible.</remarks>
        /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
        /// <param name="path">The XML file to be written.</param>
        /// <param name="data">The object to be stored.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public static void Save<T>(string path, T data, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            try
            {
                // Write to temporary file first
                using (var fileStream = File.Create(path + ".new"))
                    Save(fileStream, data, ignoreMembers);
                FileUtils.Replace(path + ".new", path);
            }
            #region Error handling
            catch (Exception)
            {
                // Clean up failed transactions
                if (File.Exists(path + ".new")) File.Delete(path + ".new");
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Returns an object as an XML string.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
        /// <param name="data">The object to be stored.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <returns>A string containing the XML code.</returns>
        public static string ToString<T>(T data, params MemberInfo[] ignoreMembers)
        {
            using (var stream = new MemoryStream())
            {
                // Write to a memory stream
                Save(stream, data, ignoreMembers);

                // Copy the stream to a string
                return StreamUtils.ReadToString(stream);
            }
        }
        #endregion

        #region Stylesheet
        /// <summary>
        /// Adds an XSL stylesheet instruction to an XML file.
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
            var feedDom = new XmlDocument();
            feedDom.Load(path);

            // Adds a new XSL stylesheet instruction to the DOM
            var stylesheetInstruction = feedDom.CreateProcessingInstruction("xml-stylesheet", string.Format(CultureInfo.InvariantCulture, "type='text/xsl' href='{0}'", stylesheetFile));
            feedDom.InsertAfter(stylesheetInstruction, feedDom.FirstChild);

            // Writes back the modified XML document
            using (var xmlWriter = XmlWriter.Create(path, new XmlWriterSettings {Encoding = new UTF8Encoding(false), Indent = true, IndentChars = "\t", NewLineChars = "\n"}))
            {
                feedDom.WriteTo(xmlWriter);

                // End file with newline
                xmlWriter.WriteWhitespace("\n");
            }
        }
        #endregion

        //--------------------//

        #region Load ZIP
        /// <summary>
        /// Loads an object from an XML file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="stream">The ZIP archive to be loaded.</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption.</param>
        /// <param name="additionalFiles">Additional files stored alongside the XML file in the ZIP archive to be read; may be <see langword="null"/>.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="ZipException">Thrown if a problem occurred while reading the ZIP data.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromZip<T>(Stream stream, string password, IEnumerable<EmbeddedFile> additionalFiles, params MemberInfo[] ignoreMembers)
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
                        using (var inputStream = zipFile.GetInputStream(zipEntry))
                            output = Load<T>(inputStream, ignoreMembers);
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
                                    using (var inputStream = zipFile.GetInputStream(zipEntry))
                                        file.StreamDelegate(inputStream);
                                }
                            }
                        }
                    }
                }
            }

            if (xmlFound) return output;
            throw new InvalidOperationException(Resources.NoXmlDataInZip);
        }

        /// <summary>
        /// Loads an object from an XML file embedded in a ZIP archive.
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
        /// <param name="path">The ZIP archive to be loaded.</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryptio.n</param>
        /// <param name="additionalFiles">Additional files stored alongside the XML file in the ZIP archive to be read; may be <see langword="null"/>.</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <returns>The loaded object.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="ZipException">Thrown if a problem occurred while reading the ZIP data.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromZip<T>(string path, string password, IEnumerable<EmbeddedFile> additionalFiles, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.OpenRead(path))
                return FromZip<T>(fileStream, password, additionalFiles, ignoreMembers);
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
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        public static void ToZip<T>(Stream stream, T data, string password, IEnumerable<EmbeddedFile> additionalFiles, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var zipStream = new ZipOutputStream(stream) {IsStreamOwner = false})
            {
                zipStream.Password = password;

                // Write the XML file to the ZIP archive
                {
                    var entry = new ZipEntry("data.xml") { DateTime = DateTime.Now };
                    zipStream.SetLevel(9);
                    zipStream.PutNextEntry(entry);
                    Save(zipStream, data, ignoreMembers);
                    zipStream.CloseEntry();
                }

                // Write additional files to the ZIP archive
                if (additionalFiles != null)
                {
                    foreach (EmbeddedFile file in additionalFiles)
                    {
                        var entry = new ZipEntry(file.Filename) { DateTime = DateTime.Now };
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
        /// <param name="ignoreMembers">Fields to be ignored when serializing.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public static void ToZip<T>(string path, T data, string password, IEnumerable<EmbeddedFile> additionalFiles, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(path);
            if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            using (var fileStream = File.Create(path))
                ToZip(fileStream, data, password, additionalFiles, ignoreMembers);
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