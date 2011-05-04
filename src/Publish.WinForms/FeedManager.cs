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
using ZeroInstall.Model;
using ZeroInstall.Publish.WinForms.Properties;

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// This class manages with dialogs the saving and the opening of a feed.
    /// </summary>
    public partial class FeedManager : Component
    {
        #region Variables

        /// <summary>
        /// The current <see cref="FeedEditing" /> to edit.
        /// </summary>
        private FeedEditing _feedEditing = new FeedEditing();

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

        #region New/Open/SaveChanges

        /// <summary>
        /// Creates a new <see cref="FeedEditing"/> after asking the user for saving changes on <see cref="_feedEditing"/>..
        /// </summary>
        /// <returns>The current <see cref="FeedEditing"/>. A new one if the user allowed it, else the old one.</returns>
        public FeedEditing New()
        {
            return SaveChanges(delegate { _feedEditing = new FeedEditing(); });
        }

        /// <summary>
        /// Opens a <see cref="FeedEditing"/> after asking the user for saving changes on <see cref="_feedEditing"/>..
        /// </summary>
        /// <returns>The current <see cref="FeedEditing"/>. The opened one if the user allowed it, else the old one.</returns>
        public FeedEditing Open()
        {
            return SaveChanges(OpenFeed);
        }

        /// <summary>
        /// Shows an <see cref="openFileDialog"/> and opens the chosen <see cref="Feed"/>.
        /// </summary>
        private void OpenFeed()
        {
            if (openFileDialog.ShowDialog(null) != DialogResult.OK) return;

            try
            {
                _feedEditing = FeedEditing.Load(openFileDialog.FileName);
            }
            #region Error handling
            catch (InvalidOperationException)
            {
                Msg.Inform(null, Resources.FeedNotValid, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException exception)
            {
                Msg.Inform(null, exception.Message, MsgSeverity.Error);
            }
            catch (IOException exception)
            {
                Msg.Inform(null, exception.Message, MsgSeverity.Error);
            }
            #endregion

            openFileDialog.FileName = _feedEditing.Path;
        }

        /// <summary>
        /// Asks the user for saving changes on <see cref="_feedEditing"/>.
        /// </summary>
        public void SaveChanges()
        {
            SaveChanges(delegate { });
        }

        /// <summary>
        /// Asks the user for saving changes and resets the feed.
        /// </summary>
        /// <param name="afterSave">Procedure that will be performed after saving the <see cref="Feed"/>.</param>
        /// <returns>The current <see cref="_feedEditing"/></returns>
        private FeedEditing SaveChanges(SimpleEventHandler afterSave)
        {
            #region Sanity checks
            if (afterSave == null) throw new ArgumentNullException("afterSave");
            #endregion

            if (!_feedEditing.Changed)
            {
                afterSave();
                return _feedEditing;
            }

            switch (AskSavingChanges())
            {
                case DialogResult.No:
                    afterSave();
                    break;
                case DialogResult.Yes:
                    if (Save())
                        afterSave();
                    break;
            }

            return _feedEditing;
        }

        /// <summary>
        /// Asks the user to save the changes on the edited <see cref="Feed"/>.
        /// </summary>
        /// <returns>The <see cref="DialogResult"/> of <see cref="Msg.Choose"/>.</returns>
        private static DialogResult AskSavingChanges()
        {
            return Msg.Choose(null, Resources.SaveQuestion, MsgSeverity.Info, true,
                              Resources.SaveChanges, Resources.DiscardChanges);
        }
        #endregion

        #region Save

        /// <summary>
        /// Saves the <see cref="Feed"/> under the current path or asks for path if necessary.
        /// </summary>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        public bool Save()
        {
            return string.IsNullOrEmpty(_feedEditing.Path) ? SaveAs() : SaveAs(_feedEditing.Path);
        }

        /// <summary>
        /// Asks the user for a path under which the <see cref="Feed"/> will be saved.
        /// </summary>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        public bool SaveAs()
        {
            return (saveFileDialog.ShowDialog(null) == DialogResult.OK && SaveAs(saveFileDialog.FileName));
        }

        /// <summary>
        /// Saves the <see cref="Feed"/>.
        /// </summary>
        /// <param name="path">Where to save the <see cref="Feed"/>.</param>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        private bool SaveAs(string path)
        {
            try
            {
                _feedEditing.Save(path);
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
    }
}