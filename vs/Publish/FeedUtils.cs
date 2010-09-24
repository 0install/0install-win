using System;
using System.IO;
using System.Text;
using Common;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Feed;

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

            // ToDo: Write default "interface.xsl" file
        }
        #endregion

        #region Signature
        /// <summary>
        /// Adds a Base64 signature to a feed file and exports the appropriate public key file in the same directory.
        /// </summary>
        /// <param name="path">The feed file to sign.</param>
        /// <param name="name">The name of the user or the ID of the private key to use for signing the file; <see langword="null"/> for default key.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="FileNotFoundException">Thrown if the feed file to be signed could not be found.</exception>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <remarks>The feed file is not parsed before signing. Invalid XML files would be signed aswell. Old feed signatures are not removed.</remarks>
        public static void SignFeed(string path, string name, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(passphrase)) throw new ArgumentNullException("passphrase");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            #endregion

            var gnuPG = new GnuPG();

            // Delete any pre-exisiting signature file
            string signatureFile = path + ".sig";
            if (File.Exists(signatureFile)) File.Delete(signatureFile);

            // Create a new signature file, parse it as Base64 and then delete it again
            gnuPG.DetachSign(path, name, passphrase);
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
                string publicKeyFile = Path.Combine(feedDir, gnuPG.GetSecretKey(name).KeyID + ".gpg");
                File.WriteAllText(publicKeyFile, gnuPG.GetPublicKey(name), Encoding.ASCII);
            }
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
    }
}
