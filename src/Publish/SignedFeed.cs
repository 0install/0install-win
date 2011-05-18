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
using System.IO;
using Common;
using Common.Cli;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Represents a Zero Install protected by a PGP signautre.
    /// </summary>
    public class SignedFeed : Feed
    {
        #region Variables
        public string SignatureID { get; set; }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a <see cref="SignedFeed"/> from an XML file (feed) and identifies the signature (if any).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="SignedFeed"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public new static SignedFeed Load(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            try { return XmlStorage.Load<SignedFeed>(path); }
            #region Error handling
            catch (InvalidOperationException ex)
            {
                // Write additional diagnostic information to log
                if (ex.Source == "System.Xml")
                {
                    string message = string.Format(Resources.ProblemLoading, path) + "\n" + ex.Message;
                    if (ex.InnerException != null) message += "\n" + ex.InnerException.Message;
                    Log.Error(message);
                }

                throw;
            }
            #endregion

            // ToDo: Identify signature
        }

        /// <summary>
        /// Saves this <see cref="SignedFeed"/> to an XML file (feed) and signs it with <see cref="SignatureID"/>.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <param name="getPassphrase">A callback method used to ask the user for the password for <see cref="Signature"/>.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the PGP implementation reported a problem.</exception>
        public void Save(string path, SimpleResult<string> getPassphrase)
        {
            #region Sanity checks
            if (getPassphrase == null) throw new ArgumentNullException("getPassphrase");
            #endregion

            XmlStorage.Save(path, this);
            FeedUtils.AddStylesheet(path);
            if (!string.IsNullOrEmpty(SignatureID)) FeedUtils.SignFeed(path, SignatureID, getPassphrase());
        }
        #endregion
    }
}
