/*
 * Copyright 2010 Bastian Eicher
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
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using Common;
using Common.Helpers;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// Provides access to the signature functions of GnuPG.
    /// </summary>
    public class GnuPG : CliAppControl
    {
        #region Properties
        /// <inheritdoc/>
        protected override string AppName { get { return "GnuPG"; } }

        /// <inheritdoc/>
        protected override string AppBinaryName { get { return "gpg"; } }
        #endregion

        //--------------------//

        /// <summary>
        /// Returns a list of all secret keys available to the current user.
        /// </summary>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public IEnumerable<GpgSecretKey> ListSecretKeys()
        {
            string result = Execute("--batch --list-secret-keys", null, ErrorHandler);
            
            return ExtractSecretKeys(result);
        }

        /// <summary>
        /// Returns a list of all secret gpg keys on this system.
        /// </summary>
        /// <param name="gpgResult">The ouput of stdout when listing all secret keys.</param>
        /// <returns>The secret keys registred on this system.</returns>
        private static IEnumerable<GpgSecretKey> ExtractSecretKeys(string gpgResult)
        {
            var secretKeys = new LinkedList<GpgSecretKey>();

            var lines = StringHelper.SplitMultilineText(gpgResult);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith("sec")) continue;
                var secretKey = new GpgSecretKey
                                    {
                                        BitLength = ExtractBitLength(lines[i]),
                                        KeyType = ExtractKeyType(lines[i]),
                                        MainSigningKey = ExtractMainSigningKey(lines[i]),
                                        CreationDate = ExtractCreationDate(lines[i]),
                                        Owner = ExtractOwner(lines[i + 1]),
                                        EmailAdress = ExtractEmailAdress(lines[i + 1])
                                    };

                secretKeys.AddLast(secretKey);
            }

            return secretKeys;
        }

        /// <summary>
        /// Returns the bit length of a secret gpg key.
        /// </summary>
        /// <param name="secLine">A line beginning with "sec" of the output from gpg when listing all secret keys.</param>
        /// <returns>The bit length.</returns>
        private static int ExtractBitLength(string secLine)
        {
            var prefixRemover = new Regex("sec\\s*");
            var suffixRemover = new Regex("[DgR]/[\\w\\W]*");
            var removedPrefix = prefixRemover.Replace(secLine, String.Empty);
            var bitLength = suffixRemover.Replace(removedPrefix, String.Empty);
            return Int32.Parse(bitLength);
        }

        /// <summary>
        /// Returns the key type of a secret gpg key.
        /// </summary>
        /// <param name="secLine">A line beginning with "sec" of the output from gpg when listing all secret keys.</param>
        /// <returns>The key type.</returns>
        private static KeyType ExtractKeyType(string secLine)
        {
            var prefixRemover = new Regex("sec\\s*[0-9]*");
            var suffixRemover = new Regex("/[\\w\\W]*");
            var removedPrefix = prefixRemover.Replace(secLine, String.Empty);
            var keyType = suffixRemover.Replace(removedPrefix, String.Empty);
            switch (keyType)
            {
                case "D":
                    return KeyType.Dsa;
                case "g":
                    return KeyType.Elgamal;
                case "R":
                    return KeyType.Rsa;
                default: throw new InvalidEnumArgumentException("Unknow key type.");
            }
        }

        /// <summary>
        /// Returns the id of a secret gpg key.
        /// </summary>
        /// <param name="secLine">A line beginning with "sec" of the output from gpg when listing all secret keys.</param>
        /// <returns>The key id.</returns>
        private static string ExtractMainSigningKey(string secLine)
        {
            var prefixRemover = new Regex("sec\\s*[0-9]*[DgR]/");
            var suffixRemover = new Regex("\\s*[0-9]{4}-[0-9]{2}-[0-9]{2}[\\w\\W]*");
            var removedPrefix = prefixRemover.Replace(secLine, String.Empty);
            var mainSigningKey = suffixRemover.Replace(removedPrefix, String.Empty);
            return mainSigningKey;
        }

        /// <summary>
        /// Returns the creation date of a secret gpg key.
        /// </summary>
        /// <param name="secLine">A line beginning with "sec" of the output from gpg when listing all secret keys.</param>
        /// <returns>The creation date of the key.</returns>
        private static DateTime ExtractCreationDate(string secLine)
        {
            var prefixRemover = new Regex("sec\\s*[0-9]*[DgR]/\\w* ");
            var suffixRemover = new Regex("[^[0-9]{4}-[0-9]{2}-[0-9]{2}]");
            var removedPrefix = prefixRemover.Replace(secLine, String.Empty);
            var creationDate = suffixRemover.Replace(removedPrefix, String.Empty);
            var splittedDate = creationDate.Split(new[] {'-'});
            return new DateTime(Int32.Parse(splittedDate[0]), Int32.Parse(splittedDate[1]), Int32.Parse(splittedDate[2]));
        }

        /// <summary>
        /// Returns the owner of a secret gpg key.
        /// </summary>
        /// <param name="uidLine">A line beginning with "uid" of the output from gpg when listing all secret keys.</param>
        /// <returns>The owner of the key.</returns>
        private static string ExtractOwner(string uidLine)
        {
            var prefixRemover = new Regex("uid\\s*");
            var suffixRemover = new Regex("\\s*<[\\w\\W]*");
            var removedPrefix = prefixRemover.Replace(uidLine, String.Empty);
            var owner = suffixRemover.Replace(removedPrefix, String.Empty);
            return owner;
        }

        /// <summary>
        /// Returns the email adress of the owner of a secret gpg key.
        /// </summary>
        /// <param name="uidLine">A line beginning with "uid" of the output from gpg when listing all secret keys.</param>
        /// <returns>The email adress of a gpg key.</returns>
        private static string ExtractEmailAdress(string uidLine)
        {
            var prefixRemover = new Regex("uid[\\w\\W]* <");
            var suffixRemover = new Regex(">[\\w\\W]*");
            var removedPrefix = prefixRemover.Replace(uidLine, String.Empty);
            var emailAdress = suffixRemover.Replace(removedPrefix, String.Empty);
            return emailAdress;            
        }

        /// <summary>
        /// Creates a detached signature for a specific file using the user's default key.
        /// </summary>
        /// <param name="path">The file to create the signature for.</param>
        /// <param name="user">The GnuPG ID of the user whose signture to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="FileNotFoundException"></exception>
        public void DetachSign(string path, string user, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
            if (string.IsNullOrEmpty(passphrase)) throw new ArgumentNullException("passphrase");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            #endregion

            Execute("--batch --passphrase-fd 0 --local-user" + user + "--detach-sign " + path, passphrase, ErrorHandler);
        }

        /// <summary>
        /// Provides error handling for GnuPG stderr.
        /// </summary>
        /// <param name="line">The error line written to stderr.</param>
        /// <returns>Always <see langword="null"/>.</returns>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        private static string ErrorHandler(string line)
        {
            if (line.StartsWith("gpg: NOTE:")) Log.Info(line);
            else throw new UnhandledErrorsException(line);
            return null;
        }
    }
}
