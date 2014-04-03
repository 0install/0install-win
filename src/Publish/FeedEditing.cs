﻿/*
 * Copyright 2010-2014 Bastian Eicher
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
using Common.Undo;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Represents a <see cref="Feed"/> being edited using <see cref="IUndoCommand"/>s.
    /// </summary>
    public class FeedEditing : CommandManager<Feed>
    {
        #region Properties
        /// <summary>
        /// The (optionally signed) feed being edited.
        /// </summary>
        public SignedFeed SignedFeed { get; private set; }

        /// <summary>
        /// The passphrase to use to unlock <see cref="Publish.SignedFeed.SecretKey"/> (if specified).
        /// </summary>
        public string Passphrase { get; set; }

        /// <inheritdoc/>
        public override Feed Target { get { return SignedFeed.Feed; } set { SignedFeed.Feed = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Starts with a <see cref="Feed"/> loaded from a file.
        /// </summary>
        /// <param name="signedFeed">The feed to be edited.</param>
        /// <param name="path">The path of the file the <paramref name="signedFeed"/> was loaded from.</param>
        public FeedEditing(SignedFeed signedFeed, string path)
        {
            #region Sanity checks
            if (signedFeed == null) throw new ArgumentNullException("signedFeed");
            #endregion

            Path = path;
            SignedFeed = signedFeed;
        }

        /// <summary>
        /// Starts with a <see cref="Feed"/> that has not been saved on disk yet.
        /// </summary>
        /// <param name="signedFeed">The feed to be edited.</param>
        public FeedEditing(SignedFeed signedFeed) : this(signedFeed, null)
        {
            Changed = true; // Makes sure remind the user to save before closing
        }

        /// <summary>
        /// Starts with an empty <see cref="Feed"/>.
        /// </summary>
        public FeedEditing() : this(new SignedFeed(new Feed()), null)
        {}
        #endregion

        #region Storage
        /// <summary>
        /// Loads a <see cref="Feed"/> from an XML file (feed).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>A <see cref="FeedEditing"/> containing the loaded <see cref="Feed"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static FeedEditing Load(string path)
        {
            return new FeedEditing(SignedFeed.Load(path), path);
        }

        /// <summary>
        /// Saves <see cref="Feed"/> to an XML file, adds the default stylesheet and signs it with <see cref="Publish.SignedFeed.SecretKey"/> (if specified).
        /// </summary>
        /// <remarks>Writing and signing the feed file are performed as an atomic operation (i.e. if signing fails an existing file remains unchanged).</remarks>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        public void Save(string path)
        {
            SignedFeed.Save(path, Passphrase);

            Path = path;
            Reset();
        }
        #endregion
    }
}
