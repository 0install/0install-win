﻿/*
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
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Info;
using Common.Utils;
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

                if (_feedEditing != null)
                {
                    menuUndo.Enabled = buttonUndo.Enabled = _feedEditing.UndoEnabled;
                    menuRedo.Enabled = buttonRedo.Enabled = _feedEditing.RedoEnabled;

                    _feedEditing.UndoEnabledChanged += () => menuUndo.Enabled = buttonUndo.Enabled = _feedEditing.UndoEnabled;
                    _feedEditing.RedoEnabledChanged += () => menuRedo.Enabled = buttonRedo.Enabled = _feedEditing.RedoEnabled;

                    feedStructureEditor.CommandManager = _feedEditing;
                }

                ListKeys();
            }
        }
        #endregion

        #region Constructor
        private readonly IOpenPgp _openPgp = OpenPgpFactory.CreateDefault();

        /// <summary>
        /// Creates a new feed editing form.
        /// </summary>
        /// <param name="feedEditing">The feed to open on start up.</param>
        public MainForm(FeedEditing feedEditing)
        {
            InitializeComponent();
            FeedEditing = feedEditing;
        }
        #endregion

        #region Event handlers
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
                AskForChangeSave();

                FeedEditing = new FeedEditing();
                comboBoxKeys.SelectedItem = null;
            }
            catch (OperationCanceledException)
            {}
        }

        private void menuNewWizard_Click(object sender, EventArgs e)
        {
            try
            {
                AskForChangeSave();

                // TODO
            }
            catch (OperationCanceledException)
            {}
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            try
            {
                AskForChangeSave();
                FeedEditing = OpenFeed(this);
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

        private void menuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuUndo_Click(object sender, EventArgs e)
        {
            feedStructureEditor.Undo();
        }

        private void menuRedo_Click(object sender, EventArgs e)
        {
            feedStructureEditor.Redo();
        }

        private void menuRemove_Click(object sender, EventArgs e)
        {
            feedStructureEditor.Remove();
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            Msg.Inform(this, AppInfo.Current.Name + " " + AppInfo.Current.Version, MsgSeverity.Info);
        }

        private void comboBoxKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxKeys.SelectedItem == NewKeyAction.Instance) NewKey();
            else FeedEditing.SignedFeed.SecretKey = comboBoxKeys.SelectedItem as OpenPgpSecretKey;
        }
        #endregion

        #region Storage
        internal static FeedEditing OpenFeed(IWin32Window owner)
        {
            using (var openFileDialog = new OpenFileDialog {Filter = "XML files|*.xml|All files|*"})
            {
                if (openFileDialog.ShowDialog(owner) != DialogResult.OK) throw new OperationCanceledException();
                try
                {
                    return FeedEditing.Load(openFileDialog.FileName);
                }
                    #region Error handling
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    throw new OperationCanceledException();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                    throw new OperationCanceledException();
                }
                catch (InvalidDataException ex)
                {
                    Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Warn);
                    throw new OperationCanceledException();
                }
                #endregion
            }
        }

        private void SaveFeed()
        {
            if (string.IsNullOrEmpty(FeedEditing.Path)) SaveFeedAs();
            else SaveFeed(FeedEditing.Path);
        }

        private void SaveFeedAs()
        {
            using (var saveFileDialog = new SaveFileDialog {DefaultExt = "xml", Filter = "XML files|*.xml|All files|*"})
            {
                if (saveFileDialog.ShowDialog(this) != DialogResult.OK) throw new OperationCanceledException();
                SaveFeed(saveFileDialog.FileName);
            }
        }

        private void SaveFeed(string path)
        {
            try
            {
                var key = FeedEditing.SignedFeed.SecretKey;
                if (key == null) FeedEditing.Save(path);
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
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                throw new OperationCanceledException();
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                throw new OperationCanceledException();
            }
            #endregion
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

        #region Keys
        private void ListKeys()
        {
            comboBoxKeys.Items.Clear();
            comboBoxKeys.Items.Add("");
            // ReSharper disable once CoVariantArrayConversion
            comboBoxKeys.Items.AddRange(_openPgp.ListSecretKeys());
            comboBoxKeys.Items.Add(NewKeyAction.Instance);

            if (FeedEditing != null) comboBoxKeys.SelectedItem = FeedEditing.SignedFeed.SecretKey;
        }

        private void NewKey()
        {
            var process = _openPgp.GenerateKey();

            // Update key list when done
            ProcessUtils.RunBackground(() =>
            {
                process.WaitForExit();
                Invoke((Action)ListKeys);
            });
        }

        private class NewKeyAction
        {
            private NewKeyAction()
            {}

            public static readonly NewKeyAction Instance = new NewKeyAction();

            public override string ToString()
            {
                return Resources.NewKey;
            }
        }
        #endregion
    }
}
