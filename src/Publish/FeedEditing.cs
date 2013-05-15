/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Cli;
using Common.Undo;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Represents a <see cref="Feed"/> being edited using <see cref="IUndoCommand"/>s.
    /// </summary>
    public class FeedEditing : CommandManager
    {
        #region Properties
        /// <summary>
        /// The feed to be editted.
        /// </summary>
        public SignedFeed Feed { get; private set; }

        /// <summary>
        /// The path of the file the <see cref="Feed"/> was loaded from. <see langword="null"/> if none.
        /// </summary>
        public string Path { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Starts with an empty <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        public FeedEditing()
        {
            Feed = new SignedFeed(new Feed(), null);
        }

        /// <summary>
        /// Starts with a <see cref="ZeroInstall.Model.Feed"/> loaded from a file.
        /// </summary>
        /// <param name="feed">The feed to be editted.</param>
        /// <param name="path">The path of the file the <paramref name="feed"/> was loaded from.</param>
        public FeedEditing(SignedFeed feed, string path)
        {
            Feed = feed;
            Path = path;
        }
        #endregion

        /// <summary>
        /// Uses <see cref="CommandManager.UndoEnabled"/> and <see cref="CommandManager.RedoEnabled"/> to enable the appropriate buttons based on the current state of the Undo system.
        /// </summary>
        /// <param name="commandPending">Pretend another command has already been executed.</param>
        public void UpdateButtonStatus(bool commandPending)
        {
            OnUndoEnabled(commandPending || UndoStack.Count > 0);
            OnRedoEnabled(!commandPending && RedoStack.Count > 0);
        }

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
        /// Saves <see cref="Feed"/> to an XML file, adds the default stylesheet and signs it with <see cref="SignedFeed.SecretKey"/> (if specified).
        /// </summary>
        /// <remarks>Writing and signing the feed file are performed as an atomic operation (i.e. if signing fails an existing file remains unchanged).</remarks>
        /// <param name="path">The file to save in.</param>
        /// <param name="passphrase">The passphrase to use to unlock the secret key; may be <see langword="null"/> if <see cref="SignedFeed.SecretKey"/> is <see langword="null"/>.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        public void Save(string path, string passphrase)
        {
            Feed.Save(path, passphrase);

            Path = path;
            Reset();
        }
        #endregion
    }
}
