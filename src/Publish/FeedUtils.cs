/*
 * Copyright 2010-2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Common;
using Common.Cli;
using Common.Compression;
using Common.Net;
using Common.Storage;
using Common.Streams;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Helper functions for manipulating <see cref="Feed"/>s.
    /// </summary>
    public static class FeedUtils
    {
        #region Stylesheet
        /// <summary>
        /// Adds the XSL stylesheet to a feed.
        /// </summary>
        /// <param name="path">The feed file to add the stylesheet to.</param>
        /// <exception cref="FileNotFoundException">Thrown if the feed file to add the stylesheet reference to could not be found.</exception>
        /// <exception cref="IOException">Thrown if the feed file to add the stylesheet reference to could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        public static void AddStylesheet(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            XmlStorage.AddStylesheet(path, "interface.xsl");

            string directory = Path.GetDirectoryName(path);
            if (directory == null) return;

            // Write the default XSL with its accompanying CSS file unless there is already an XSL in place
            if (!File.Exists(Path.Combine(directory, "interface.xsl")))
            {
                File.WriteAllText(Path.Combine(directory, "interface.xsl"), GetEmbeddedResource("interface.xsl"));
                File.WriteAllText(Path.Combine(directory, "interface.css"), GetEmbeddedResource("interface.css"));
            }
        }

        private static string GetEmbeddedResource(string name)
        {
            var assembly = Assembly.GetAssembly(typeof(FeedUtils));
            using (var stream = assembly.GetManifestResourceStream(typeof(FeedUtils), name))
                return StreamUtils.ReadToString(stream);
        }
        #endregion

        #region Sign
        /// <summary>
        /// Adds a Base64 signature to a feed file and exports the appropriate public key file in the same directory.
        /// </summary>
        /// <param name="path">The feed file to sign.</param>
        /// <param name="secretKey">The private key to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the key.</param>
        /// <exception cref="FileNotFoundException">Thrown if the feed file to be signed could not be found.</exception>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        /// <remarks>The feed file is not parsed before signing. Invalid XML files would be signed as well. Old feed signatures are not removed.</remarks>
        public static void SignFeed(string path, OpenPgpSecretKey secretKey, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            #endregion

            var pgp = OpenPgpProvider.Default;

            // Delete any pre-exisiting signature file
            string signatureFile = path + ".sig";
            if (File.Exists(signatureFile)) File.Delete(signatureFile);

            // Create a new signature file, parse it as Base64 and then delete it again
            pgp.DetachSign(path, secretKey.KeyID, passphrase);
            string base64Signature = Convert.ToBase64String(File.ReadAllBytes(signatureFile));
            File.Delete(signatureFile);

            // Add the Bas64 encoded signature to the end of the file in an XML comment
            using (var sw = new StreamWriter(path, true) {NewLine = "\n"})
            {
                sw.WriteLine("<!-- Base64 Signature");
                sw.WriteLine(base64Signature);
                sw.WriteLine();
                sw.Write("-->");
                sw.WriteLine();
            }

            // Export the user's public key
            string feedDir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (feedDir != null)
            {
                string publicKeyFile = Path.Combine(feedDir, secretKey.KeyID + ".gpg");
                File.WriteAllText(publicKeyFile, pgp.GetPublicKey(secretKey.KeyID), Encoding.ASCII);
            }
        }

        /// <summary>
        /// Adds a Base64 signature to a feed file and exports the appropriate public key file in the same directory.
        /// </summary>
        /// <param name="path">The feed file to sign.</param>
        /// <param name="secretKey">The private key to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the key.</param>
        /// <exception cref="FileNotFoundException">Thrown if the feed file to be signed could not be found.</exception>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        /// <remarks>The feed file is not parsed before signing. Invalid XML files would be signed as well. Old feed signatures are not removed.</remarks>
        public static void SignFeed(string path, string secretKey, string passphrase)
        {
            try { SignFeed(path, OpenPgpProvider.Default.GetSecretKey(secretKey), passphrase); }
            #region Error handling
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new UnhandledErrorsException(ex.Message, ex);
            }
            #endregion
        }

        /// <summary>
        /// Removes any Base64 signature from a feed file.
        /// </summary>
        /// <param name="path">The feed file to remove the signature from.</param>
        /// <exception cref="FileNotFoundException">Thrown if the feed file to be signed could not be found.</exception>
        /// <exception cref="IOException">Thrown if the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <remarks>The feed file is not parsed before removing the signature.</remarks>
        public static void UnsignFeed(string path)
        {
            // ToDo: Implement without reparsing
            Feed.Load(path).Save(path);
        }
        #endregion

        #region Verify
        /// <summary>
        /// ToDo
        /// </summary>
        /// <param name="path">The feed file to verify.</param>
        public static void VerifyFeed(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToVerifyNotFound, path);
            #endregion

            // ToDo: Implement
            //var gnuPG = new GnuPG();
        }
        #endregion

        #region Implementation
        /// <summary>
        /// Creates a new <see cref="Implementation"/> by downloading an archive from a specific URL, extracting it and calculating its <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="source">The URL used to locate the archive.</param>
        /// <param name="extract">The name of the subdirectory in the archive to extract; <see langword="null"/> or <see cref="string.Empty"/> for entire archive.</param>
        /// <param name="cache">Adds the downloaded archive to <see cref="StoreProvider.CreateDefault"/> when set to <see langword="true"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>A newly created <see cref="Implementation"/> containing one <see cref="Archive"/>.</returns>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if there was a problem extracting the archive.</exception>
        /// <exception cref="WebException">Thrown if there was a problem downloading the archive.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to temporary files was not permitted.</exception>
        /// <exception cref="NotSupportedException">Thrown if the archive's MIME type could not be determined.</exception>
        /// <remarks>The archive's MIME type is guessed based on its file extension in <paramref name="source"/>.</remarks>
        public static Implementation BuildImplementation(string source, string extract, bool cache, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string mimeType = ArchiveUtils.GuessMimeType(source);
            var location = new Uri(source);

            using (var tempFile = new TemporaryFile("0install"))
            {
                handler.RunTask(new DownloadFile(location, tempFile.Path), null);

                using (var tempDir = new TemporaryDirectory("0install"))
                {
                    using (var extractor = Extractor.CreateExtractor(mimeType, tempFile.Path, 0, tempDir.Path))
                    {
                        extractor.SubDir = extract;
                        handler.RunTask(extractor, null);
                    }

                    var digest = Manifest.CreateDigest(tempDir.Path, handler);
                    if (cache)
                    {
                        var store = StoreProvider.CreateDefault();
                        try { store.AddDirectory(tempDir.Path, digest, handler); }
                        catch(ImplementationAlreadyInStoreException) {}
                    }

                    var archive = new Archive {Location = location, MimeType = mimeType, Size = new FileInfo(tempFile.Path).Length, Extract = extract};
                    return new Implementation {ID = "sha1new=" + digest.Sha1New, ManifestDigest = digest, RetrievalMethods = {archive}};
                }
            }
        }
        #endregion
    }
}
