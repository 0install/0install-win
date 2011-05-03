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
using System.Windows.Forms;
using Common;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using System.Collections.Generic;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A specialized <see cref="ToolStrip"/> for the Feed Editor. It has <see cref="Button"/>s to create, open, save and save as a feed.
    /// As well it has <see cref="Button"/>s to undo or redo an action. With an <see cref="ComboBox"/> the user can select an <see cref="OpenPgpSecretKey"/>.
    /// </summary>
    public partial class FeedEditorToolStrip : ToolStrip
    {
        #region Events
        /// <summary>
        /// Raised when a new <see cref="Feed"/> shall be created.
        /// </summary>
        public event SimpleEventHandler New;

        /// <summary>
        /// Raised when an existing <see cref="Feed"/> shall be opened.
        /// </summary>
        public event SimpleEventHandler Open;

        /// <summary>
        /// Raised when an the current editted <see cref="Feed"/> shall be saved.
        /// </summary>
        public event SimpleEventHandler Save;

        /// <summary>
        /// Raised when an the current editted <see cref="Feed"/> shall be saved on a new place.
        /// </summary>
        public event SimpleEventHandler SaveAs;

        /// <summary>
        /// Raised when the last action on the <see cref="Feed"/> shall be undone.
        /// </summary>
        public event SimpleEventHandler Undo;

        /// <summary>
        /// Raised when the last undo on the <see cref="Feed"/> shall be redone.
        /// </summary>
        public event SimpleEventHandler Redo;

        /// <summary>
        /// Raised when the user selected an other <see cref="OpenPgpSecretKey"/>.
        /// </summary>
        public event Action<OpenPgpSecretKey> SecretKeyChanged;
        #endregion

        #region Variables
        /// <summary>
        /// The selected <see cref="OpenPgpSecretKey"/> by the user.
        /// </summary>
        private OpenPgpSecretKey _selectedSecretKey = default(OpenPgpSecretKey);
        #endregion

        #region Properties
        /// <summary>
        /// The selected <see cref="OpenPgpSecretKey"/> by the user.
        /// </summary>
        public OpenPgpSecretKey SelectedSecretKey
        {
            get { return _selectedSecretKey; }
            set
            {
                if (!comboBoxGnuPG.Items.Contains(value)) comboBoxGnuPG.Items.Add(value);
                comboBoxGnuPG.SelectedItem = value;
            }
        }

        /// <summary>
        /// The <see cref="OpenPgpSecretKey"/>s the user can select from.
        /// </summary>
        public IEnumerable<OpenPgpSecretKey> SecretKeyValues
        {
            set
            {
                #region Sanity checks
                if (value == null) throw new ArgumentNullException("value");
                #endregion

                comboBoxGnuPG.BeginUpdate();
                
                comboBoxGnuPG.Items.Clear();
                comboBoxGnuPG.Items.Add(default(OpenPgpSecretKey));
                foreach (var openPgpSecretKey in value)
                    comboBoxGnuPG.Items.Add(openPgpSecretKey);

                comboBoxGnuPG.EndUpdate();
            }
        }

        /// <summary>
        /// Enables or disables the undo <see cref="Button"/>.
        /// </summary>
        public bool UndoEnabled
        {
            set
            {
                buttonUndo.Enabled = value;
            }
        }

        /// <summary>
        /// Enables or disables the redo <see cref="Button"/>.
        /// </summary>
        public bool RedoEnabled
        {
            set
            {
                buttonRedo.Enabled = value;
            }
        }
        #endregion

        #region Constructor
        public FeedEditorToolStrip()
        {
            InitializeComponent();
            ConnectEvents();
            SetDefaultValues();
        }

        /// <summary>
        /// Sets the default values.
        /// </summary>
        private void SetDefaultValues()
        {
            comboBoxGnuPG.Items.Add(_selectedSecretKey);
            comboBoxGnuPG.SelectedItem = _selectedSecretKey;
        }

        /// <summary>
        /// Connects the events to use with their methods.
        /// </summary>
        private void ConnectEvents()
        {
            buttonNew.Click += delegate { if (New != null) New(); };
            buttonOpen.Click += delegate { if (Open != null) Open(); };
            buttonSave.Click += delegate { if (Save != null) Save(); };
            buttonSaveAs.Click += delegate { if (SaveAs != null) SaveAs(); };
            buttonUndo.Click += delegate { if (Undo != null) Undo(); };
            buttonRedo.Click += delegate { if (Redo != null) Redo(); };
            comboBoxGnuPG.SelectedIndexChanged += SelectedSecretKeyChanged;
        }
        #endregion

        //--------------------//

        #region Control events

        /// <summary>
        /// Sets the property <see cref="SelectedSecretKey"/> and raises the event <see cref="SecretKeyChanged"/>.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void SelectedSecretKeyChanged(object sender, EventArgs e)
        {
            if (SecretKeyChanged == null) return;

            var selectedKey = comboBoxGnuPG.SelectedItem ?? default(OpenPgpSecretKey);

            _selectedSecretKey = (OpenPgpSecretKey) selectedKey;
            SecretKeyChanged((OpenPgpSecretKey) selectedKey);
        }

        #endregion
    }
}