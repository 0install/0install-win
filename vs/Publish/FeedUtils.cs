using System;
using System.IO;
using Common;
using ZeroInstall.Model;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Helper functions for manipulating <see cref="Feed"/>s.
    /// </summary>
    public static class FeedUtils
    {
        /// <summary>
        /// Adds a Base64 signature to feed file.
        /// </summary>
        /// <remarks>The feed file is not parsed before signing. Invalid XML files would be signed aswell.</remarks>
        /// <param name="path">The feed file to sign.</param>
        /// <param name="user">The GnuPG ID of the user whose signture to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the feed file is not permitted.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the feed file to be signed could not be found.</exception>
        public static void SignFeed(string path, string user, string passphrase)
        {
            // Delete any pre-exisiting signature file
            string signatureFile = path + ".sig";
            if (File.Exists(signatureFile)) File.Delete(signatureFile);

            // Create a new signature file, parse it as Base64 and then delete it again
            new GnuPG().DetachSign(path, user, passphrase);
            string base64Signature = Convert.ToBase64String(File.ReadAllBytes(signatureFile));
            File.Delete(signatureFile);

            // Add the Bas64 encoded signature to the end of the file in an XML comment
            using (var sw = new StreamWriter(path, true) { NewLine = "\n" })
            {
                sw.WriteLine("<!-- Base64 Signature");
                sw.WriteLine(base64Signature);
                sw.WriteLine();
                sw.Write("-->");
                sw.WriteLine();
            }
        }
    }
}
