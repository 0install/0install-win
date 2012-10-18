/*
 * Copyright 2011 Simon E. Silva Lauinger
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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Publish.WinForms.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// This class manages with dialogs the saving and the opening of a feed.
    /// </summary>
    public partial class FeedManager : Component
    {
        #region Properties
        /// <summary>
        /// The <see cref="OpenPgpSecretKey"/> with the key will be signed. The <see cref="Feed"/> will not be signed if it's the default key.
        /// </summary>
        public OpenPgpSecretKey SigningKey
        {
            get { return _feedEditing.Feed.SecretKey; }
            set
            {
                if (_feedEditing.Feed.SecretKey == value) return;
                _feedEditing.Feed.SecretKey = value;
                _signingKeyPassphrase = null;
            }
        }
        #endregion

        #region Variables
        /// <summary>
        /// The current <see cref="FeedEditing" /> to edit.
        /// </summary>
        private FeedEditing _feedEditing = new FeedEditing();

        private string _signingKeyPassphrase;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="FeedManager"/> instance.
        /// </summary>
        /// <param name="parent">The parent <see cref="IWin32Window"/>. Must not be <see langword="null"/>.</param>
        public FeedManager(IContainer parent)
        {
            #region Sanity checks
            if (parent == null) throw new ArgumentNullException("parent");
            #endregion

            parent.Add(this);
            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region New/Open
        /// <summary>
        /// Creates a new <see cref="FeedEditing"/> after asking the user for saving changes on <see cref="_feedEditing"/>..
        /// </summary>
        /// <returns>The current <see cref="FeedEditing"/>. A new one if the user allowed it, else <see langword="null"/>.</returns>
        public FeedEditing New()
        {
            return SaveChanges(delegate
            {
                _feedEditing = new FeedEditing();
                return true;
            }) ? _feedEditing : null;
        }

        /// <summary>
        /// Opens a <see cref="FeedEditing"/> after asking the user for saving changes on <see cref="_feedEditing"/>..
        /// </summary>
        /// <returns>The current <see cref="FeedEditing"/>. The opened one if the user allowed it, else <see langword="null"/>.</returns>
        public FeedEditing Open()
        {
            return SaveChanges(OpenFeed) ? _feedEditing : null;
        }

        /// <summary>
        /// Shows an <see cref="openFileDialog"/> and opens the chosen <see cref="Feed"/>.
        /// </summary>
        private bool OpenFeed()
        {
            //TODO: determine the signature key
            if (openFileDialog.ShowDialog(null) != DialogResult.OK) return false;

            try
            {
                _feedEditing = FeedEditing.Load(openFileDialog.FileName);
            }
                #region Error handling
            catch (InvalidDataException ex)
            {
                Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (IOException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            #endregion

            openFileDialog.FileName = _feedEditing.Path;
            return true;
        }
        #endregion

        #region Save
        /// <summary>
        /// Asks the user for saving changes on <see cref="_feedEditing"/>.
        /// </summary>
        public bool SaveChanges()
        {
            return SaveChanges(() => true);
        }

        /// <summary>
        /// Asks the user for saving changes and resets the feed.
        /// </summary>
        /// <param name="afterSave">Procedure that will be performed after saving the <see cref="Feed"/>.</param>
        /// <returns>The current <see cref="_feedEditing"/></returns>
        private bool SaveChanges(Func<bool> afterSave)
        {
            #region Sanity checks
            if (afterSave == null) throw new ArgumentNullException("afterSave");
            #endregion

            if (!_feedEditing.Changed)
                return afterSave();

            switch (AskSavingChanges())
            {
                case DialogResult.No:
                    afterSave();
                    return true;
                case DialogResult.Yes:
                    if (Save())
                    {
                        afterSave();
                        return true;
                    }
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Asks the user to save the changes on the edited <see cref="Feed"/>.
        /// </summary>
        /// <returns><see cref="DialogResult.Yes"/> if 'Yes' was chosen,
        /// <see cref="DialogResult.No"/> if 'No' was chosen,
        /// <see cref="DialogResult.Cancel"/> otherwise.</returns>
        private static DialogResult AskSavingChanges()
        {
            return Msg.YesNoCancel(null, Resources.SaveQuestion, MsgSeverity.Info, Resources.SaveChanges, Resources.DiscardChanges);
        }

        /// <summary>
        /// Saves the <see cref="Feed"/> under the current path or asks for path if necessary.
        /// </summary>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        public bool Save()
        {
            if (string.IsNullOrEmpty(_feedEditing.Path)) return SaveAs();

            return NeedsPassphrase() ? AskForPassphrase() && SaveAs(_feedEditing.Path) : SaveAs(_feedEditing.Path);
        }

        /// <summary>
        /// Asks the user for a path under which the <see cref="Feed"/> will be saved.
        /// </summary>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        public bool SaveAs()
        {
            if (NeedsPassphrase())
            {
                return saveFileDialog.ShowDialog(null) == DialogResult.OK && AskForPassphrase() &&
                    SaveAs(saveFileDialog.FileName);
            }
            return saveFileDialog.ShowDialog(null) == DialogResult.OK && SaveAs(saveFileDialog.FileName);
        }

        /// <summary>
        /// Saves the <see cref="Feed"/> with a signature.
        /// </summary>
        /// <param name="path">Where to save the <see cref="Feed"/>.</param>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        private bool SaveAs(string path)
        {
            try
            {
                _feedEditing.Save(path, _signingKeyPassphrase);
            }
                #region Error handling
            catch (IOException exception)
            {
                Msg.Inform(null, exception.Message, MsgSeverity.Error);
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                saveFileDialog.InitialDirectory = Environment.GetEnvironmentVariable("USERPROFILE");
                return SaveAs();
            }
            #endregion

            saveFileDialog.FileName = _feedEditing.Path;
            return true;
        }
        #endregion

        #region Passphrase
        /// <summary>
        /// Checks whether the stored passphrase is actual.
        /// </summary>
        /// <returns><see langword="true"/>, if the passphrase is needed, else <see langword="false"/>.</returns>
        private bool NeedsPassphrase()
        {
            return _feedEditing.Feed.SecretKey != null && string.IsNullOrEmpty(_signingKeyPassphrase);
        }

        /// <summary>
        /// Asks the user for the passphrase of his secret key.
        /// </summary>
        /// <returns><see langword="true"/>, if the user entered the correct passphrase, <see langword="false"/> if the user aborted.</returns>
        private bool AskForPassphrase()
        {
            bool wrongPassphrase = false;
            while (true)
            {
                string passphraseMessage = string.Format(wrongPassphrase
                    ? Resources.WrongPassphrase
                    : Resources.AskForPassphrase, _feedEditing.Feed.SecretKey.UserID);
                string passphrase = InputBox.Show(null, Resources.AskForPassphraseTitle, passphraseMessage, "", true);

                if (passphrase == null) return false;

                if (!string.IsNullOrEmpty(passphrase) && IsPassphraseCorrect(_feedEditing.Feed.SecretKey.UserID, passphrase))
                {
                    _signingKeyPassphrase = passphrase;
                    return true;
                }

                wrongPassphrase = true;
            }
        }

        /// <inheritdoc />
        private static bool IsPassphraseCorrect(string keySpecifier, string passphrase)
        {
            if (string.IsNullOrEmpty(passphrase)) return false;

            using (var tempFile = new TemporaryFile("gpg"))
            {
                try
                {
                    OpenPgpProvider.CreateDefault().DetachSign(tempFile.Path, keySpecifier, passphrase);
                }
                catch (WrongPassphraseException)
                {
                    return false;
                }
                return true;
            }
        }
        #endregion
    }
}
