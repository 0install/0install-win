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
        /// <summary>
        /// The parent <see cref="IWin32Window"/> of this component.
        /// </summary>
        //private Form parent;
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
            if (_feedEditing.Changed) return SaveChanges(delegate { _feedEditing = new FeedEditing(); });
            else {
                _feedEditing = new FeedEditing();
                return _feedEditing;
            }
        }

        /// <summary>
        /// Opens a <see cref="FeedEditing"/> after asking the user for saving changes on <see cref="_feedEditing"/>..
        /// </summary>
        /// <returns>The current <see cref="FeedEditing"/>. The opened one if the user allowed it, else the old one.</returns>
        public FeedEditing Open()
        {
            if (_feedEditing.Changed) return SaveChanges(OpenFeed);
            else
            {
                OpenFeed();
                return _feedEditing;
            }
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
        /// <param name="AfterSave">Procedure that will be performed after resetting <see cref="_feedEditing"/>.</param>
        /// <returns>The current <see cref="_feedEditing"/></returns>
        private FeedEditing SaveChanges(SimpleEventHandler AfterSave)
        {
            #region Sanity checks
            if (AfterSave == null) throw new ArgumentNullException("AfterSave");
            #endregion

            if (AskSave() == DialogResult.No || Save()) AfterSave();

            return _feedEditing;
        }

        /// <summary>
        /// Asks the user to save the edited <see cref="Feed"/>.
        /// </summary>
        /// <returns></returns>
        private DialogResult AskSave()
        {
            return Msg.Choose(null, Resources.SaveQuestion, MsgSeverity.Info, true,
                           Resources.SaveChanges, Resources.DiscardChanges);
        }

        /// <summary>
        /// Shows an <see cref="openFileDialog"/> and opens the chosen <see cref="Feed"/>.
        /// </summary>
        private void OpenFeed()
        {
            openFileDialog.FileName = _feedEditing.Path;
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
            saveFileDialog.FileName = _feedEditing.Path;
            if (saveFileDialog.ShowDialog(null) != DialogResult.OK) return false;

            return SaveAs(saveFileDialog.FileName);
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
            catch (UnauthorizedAccessException exception)
            {

                Msg.Inform(null, exception.Message, MsgSeverity.Error);
                return false;
            }
            #endregion

            return true;
        }
        #endregion
    }
}
