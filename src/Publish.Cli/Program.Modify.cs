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
using Common.Cli;
using ZeroInstall.Publish.Cli.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Publish.Cli
{
    public static partial class Program
    {
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="InvalidDataException">Thrown if the feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the feed file could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if a OpenPGP key could not be found.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        public static void ModifyFeeds(ParseResults results)
        {
            foreach (var file in results.Feeds)
            {
                var feed = SignedFeed.Load(file.FullName);

                // ToDo: Apply modifications

                HandleResults(feed, ref results);

                feed.Save(file.FullName, results.GnuPGPassphrase);
            }
        }

        private static void HandleResults(SignedFeed feed, ref ParseResults results)
        {
            if (results.Unsign)
            {
                // Remove any existing signatures
                feed.SecretKey = null;
            }
            else
            {
                var openPgp = OpenPgpProvider.Default;

                // Use default secret key if there are no existing signatures
                if (results.XmlSign && feed.SecretKey == null)
                    feed.SecretKey = openPgp.GetSecretKey(null);

                // Use specific secret key for signature
                if (!string.IsNullOrEmpty(results.Key))
                    feed.SecretKey = openPgp.GetSecretKey(results.Key);
            }

            // Ask for passphrase to unlock secret key
            if (feed.SecretKey != null && string.IsNullOrEmpty(results.GnuPGPassphrase))
                results.GnuPGPassphrase = CliUtils.ReadPassword(Resources.PleaseEnterGnuPGPassphrase);
        }
    }
}
