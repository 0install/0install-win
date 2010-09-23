using System;
using System.IO;
using System.Text;
using System.Xml;
using Common;
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
        /// Adds an XSL stylesheet instruction to a feed file.
        /// </summary>
        /// <param name="path">The feed file to add the stylesheet instruction to.</param>
        /// <param name="stylesheetFile">The file name of the stylesheet to reference.</param>
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
            var stylesheetInstruction = feedDom.CreateProcessingInstruction("xml-stylesheet", string.Format("type='text/xsl' href='{0}'", stylesheetFile));
            feedDom.InsertAfter(stylesheetInstruction, feedDom.FirstChild);

            // Writes the modified XML document
            using (var xmlWriter = XmlWriter.Create(path, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true, IndentChars = "\t", NewLineChars = "\n" }))
            {
                feedDom.WriteTo(xmlWriter);
                xmlWriter.WriteWhitespace("\n");
            }
        }
        #endregion

        #region Signature
        /// <summary>
        /// Adds a Base64 signature to a feed file and exports the appropriate public key file in the same directory.
        /// </summary>
        /// <remarks>The feed file is not parsed before signing. Invalid XML files would be signed aswell. Old feed signatures are not removed.</remarks>
        /// <param name="path">The feed file to sign.</param>
        /// <param name="user">The GnuPG ID of the user whose signture to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the feed file is not permitted.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the feed file to be signed could not be found.</exception>
        public static void SignFeed(string path, string user, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
            if (string.IsNullOrEmpty(passphrase)) throw new ArgumentNullException("passphrase");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            #endregion

            var gnuPG = new GnuPG();

            // Delete any pre-exisiting signature file
            string signatureFile = path + ".sig";
            if (File.Exists(signatureFile)) File.Delete(signatureFile);

            // Create a new signature file, parse it as Base64 and then delete it again
            gnuPG.DetachSign(path, user, passphrase);
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
                string publicKeyFile = Path.Combine(feedDir, user + ".gpg");
                File.WriteAllText(publicKeyFile, gnuPG.GetPublicKey(user), Encoding.ASCII);
            }
        }
        #endregion
    }
}
