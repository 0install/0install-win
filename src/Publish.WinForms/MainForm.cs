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
using System.Windows.Forms;
using Common;
using Common.Controls;
using ZeroInstall.Publish.WinForms.Properties;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish.WinForms
{
    public partial class MainForm : Form
    {
        #region Properties
        private FeedEditing _feedEditing;

        /// <summary>
        /// The feed currently being edited.
        /// </summary>
        private FeedEditing FeedEditing
        {
            get { return _feedEditing; }
            set
            {
                _feedEditing = value;

                menuUndo.Enabled = buttonUndo.Enabled = menuRedo.Enabled = buttonRedo.Enabled = false;
                if (_feedEditing != null)
                {
                    _feedEditing.UndoEnabled += state => menuUndo.Enabled = buttonUndo.Enabled = state;
                    _feedEditing.RedoEnabled += state => menuRedo.Enabled = buttonRedo.Enabled = state;

                    feedEditorControl.Target = _feedEditing.SignedFeed.Feed;
                    feedEditorControl.CommandExecutor = _feedEditing;
                }

                comboBoxKeys.SelectedItem = FeedEditing.SignedFeed.SecretKey;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new feed editing form.
        /// </summary>
        /// <param name="feedEditing">The feed to open on start up.</param>
        public MainForm(FeedEditing feedEditing)
        {
            InitializeComponent();

            // ReSharper disable CoVariantArrayConversion
            comboBoxKeys.Items.AddRange(OpenPgpFactory.CreateDefault().ListSecretKeys());
            // ReSharper restore CoVariantArrayConversion
            comboBoxKeys.Items.Add("");

            FeedEditing = feedEditing;
        }
        #endregion

        #region Events
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                AskForChangeSave();
            }
            catch (OperationCanceledException)
            {
                e.Cancel = true;
            }
        }

        private void menuNew_Click(object sender, EventArgs e)
        {
            try
            {
                NewFeed();
            }
            catch (OperationCanceledException)
            {}
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFeed();
            }
            catch (OperationCanceledException)
            {}
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFeed();
            }
            catch (OperationCanceledException)
            {}
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFeedAs();
            }
            catch (OperationCanceledException)
            {}
        }

        private void menuUndo_Click(object sender, EventArgs e)
        {
            FeedEditing.Undo();
        }

        private void menuRedo_Click(object sender, EventArgs e)
        {
            FeedEditing.Redo();
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBoxKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            FeedEditing.SignedFeed.SecretKey = comboBoxKeys.SelectedItem as OpenPgpSecretKey;
        }
        #endregion

        #region Storage
        private void NewFeed()
        {
            AskForChangeSave();

            FeedEditing = new FeedEditing();
            comboBoxKeys.SelectedItem = null;
        }

        private void OpenFeed()
        {
            AskForChangeSave();

            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) throw new OperationCanceledException();
            FeedEditing = FeedEditing.Load(openFileDialog.FileName);
        }

        private void SaveFeed()
        {
            if (string.IsNullOrEmpty(FeedEditing.Path)) SaveFeedAs();
            else SaveFeed(FeedEditing.Path);
        }

        private void SaveFeedAs()
        {
            saveFileDialog.FileName = "";
            if (saveFileDialog.ShowDialog(this) != DialogResult.OK) throw new OperationCanceledException();
            SaveFeed(saveFileDialog.FileName);
        }

        private void SaveFeed(string path)
        {
            var key = FeedEditing.SignedFeed.SecretKey;
            if (key == null) FeedEditing.Save(path, null);
            else
            {
                string passphrase = InputBox.Show(this, Text, string.Format(Resources.AskForPassphrase, key), "", true);
                Retry:
                if (passphrase == null) throw new OperationCanceledException();
                try
                {
                    FeedEditing.Save(path, passphrase);
                }
                catch (WrongPassphraseException)
                {
                    passphrase = InputBox.Show(this, Text, Resources.WrongPassphrase + "\n" + string.Format(Resources.AskForPassphrase, key), "", true);
                    goto Retry;
                }
            }
        }

        private void AskForChangeSave()
        {
            if (FeedEditing.Changed)
            {
                switch (Msg.YesNoCancel(this, Resources.SaveQuestion, MsgSeverity.Warn, Resources.SaveChanges, Resources.DiscardChanges))
                {
                    case DialogResult.Yes:
                        SaveFeed();
                        break;
                    case DialogResult.Cancel:
                        throw new OperationCanceledException();
                }
            }
        }
        #endregion
    }
}
