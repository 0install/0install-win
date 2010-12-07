/*
 * Copyright 2010 Simon E. Silva Lauinger, Bastian Eicher
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
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Undo;
using Common.Utils;
using ZeroInstall.Model;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using ZeroInstall.Publish.WinForms.FeedStructure;
using ZeroInstall.Store.Feed;
using Binding = ZeroInstall.Model.Binding;
using ZeroInstall.Publish.WinForms.Controls;

namespace ZeroInstall.Publish.WinForms
{
    public partial class MainForm : Form
    {
        #region Events

        /// <summary>To be called when the controls on the form need to filled with content from the feed.</summary>
        private event SimpleEventHandler Populate;

        #endregion

        #region Constants
        private const string FeedFileFilter = "ZeroInstall Feed (*.xml)|*.xml|All Files|*.*";
        private readonly ImageFormat[] _supportedImageFormats = new[] {ImageFormat.Png, ImageFormat.Icon};
        private readonly string[] _supportedInjectorVersions = new[] { string.Empty, "0.31", "0.32", "0.33", "0.34",
            "0.35", "0.36", "0.37", "0.38", "0.39", "0.40", "0.41", "0.41.1", "0.42", "0.42.1", "0.43", "0.44", "0.45"};
        #endregion

        #region Variables
        private FeedEditing _feedEditing = new FeedEditing();
        #endregion

        #region Initialization

        /// <summary>
        /// Creats a new <see cref="MainForm"/> object.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            InitializeComponentsExtended();
            InitializeCommandHooks();
            InitializeEditingHooks();
        }

        /// <summary>
        /// Initializes settings of the form components which can't be setted by the property grid.
        /// </summary>
        private void InitializeComponentsExtended()
        {
            InitializeSaveFileDialog();
            InitializeLoadFileDialog();
            InitializeComboBoxIconType();
            InitializeTreeViewFeedStructure();
            InitializeFeedStructureButtons();
            InitializeComboBoxMinInjectorVersion();
            InitializeComboBoxGpg();
        }

        /// <summary>
        /// Initializes the <see cref="saveFileDialog"/> with a file filter for .xml files.
        /// </summary>
        private void InitializeSaveFileDialog()
        {
            if (_feedEditing.Path != null) saveFileDialog.InitialDirectory = _feedEditing.Path;
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = FeedFileFilter;
        }

        /// <summary>
        /// Initializes the <see cref="openFileDialog"/> with a file filter for .xml files.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA2204:Literals should be spelled correctly", MessageId = "ZeroInstall")]
        private void InitializeLoadFileDialog()
        {
            if (_feedEditing.Path != null) openFileDialog.InitialDirectory = _feedEditing.Path;
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = FeedFileFilter;
        }

        /// <summary>
        /// Adds the supported <see cref="ImageFormat"/>s to <see cref="comboBoxIconType"/>.
        /// </summary>
        private void InitializeComboBoxIconType()
        {
            comboBoxIconType.Items.AddRange(_supportedImageFormats);
        }

        /// <summary>
        /// Adds <see cref="Feed"/> to the Tag of the first <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>.
        /// </summary>
        private void InitializeTreeViewFeedStructure()
        {
            treeViewFeedStructure.Nodes[0].Tag = _feedEditing.Feed;
        }

        /// <summary>
        /// Sets the Tag of the feed structure buttons with a object they shall add to <see cref="treeViewFeedStructure"/>.
        /// E.g. the button <see cref="btnAddGroup"/> gets a <see cref="Group"/> object.
        /// Used to identify the buttons.
        /// </summary>
        private void InitializeFeedStructureButtons()
        {
            btnAddGroup.Tag = new Group();
            btnAddDependency.Tag = new Dependency();
            btnAddEnvironmentBinding.Tag = new EnvironmentBinding();
            btnAddOverlayBinding.Tag = new OverlayBinding();
            btnAddWorkingDir.Tag = new WorkingDirBinding();
            btnAddPackageImplementation.Tag = new PackageImplementation();
            btnAddImplementation.Tag = new Implementation();
            buttonAddArchive.Tag = new Archive();
            buttonAddRecipe.Tag = new Recipe();
        }

        private void InitializeComboBoxMinInjectorVersion()
        {
            comboBoxMinInjectorVersion.Items.AddRange(_supportedInjectorVersions);
        }

        /// <summary>
        /// Adds a list of secret gpg keys of the user to comboBoxGpg.
        /// </summary>
        private void InitializeComboBoxGpg()
        {
            toolStripComboBoxGpg.Items.Add(string.Empty);

            foreach (var secretKey in GetGnuPGSecretKeys())
            {
                toolStripComboBoxGpg.Items.Add(secretKey);
            }
        }

        /// <summary>
        /// Returns all GnuPG secret keys of the user. If GnuPG can not be found on the system, a message box informs the user.
        /// </summary>
        /// <returns>The GnuPG secret keys.</returns>
        private IEnumerable<GnuPGSecretKey> GetGnuPGSecretKeys()
        {
            try
            {
                return new GnuPG().ListSecretKeys();
            }
            catch (IOException)
            {
                Msg.Inform(this, "GnuPG could not be found on your system.\nYou can not sign feeds.",
                           MsgSeverity.Warning);
                return new GnuPGSecretKey[0];
            }
        }

        /// <summary>
        /// Sets up hooks for keeping the WinForms controls synchronized with the <see cref="Feed"/> data using the command pattern.
        /// </summary>
        private void InitializeCommandHooks()
        {
            SetupCommandHooks(textName, () => _feedEditing.Feed.Name, value => _feedEditing.Feed.Name = value);
            SetupCommandHooks(checkedListBoxCategories, () => _feedEditing.Feed.Categories);
            SetupCommandHooks(textInterfaceUri, () => _feedEditing.Feed.Uri, value => _feedEditing.Feed.Uri = value);
            SetupCommandHooks(textHomepage, () => _feedEditing.Feed.Homepage, value => _feedEditing.Feed.Homepage = value);
            SetupCommandHooks(summariesControl, () => _feedEditing.Feed.Summaries);
            SetupCommandHooks(descriptionControl, () => _feedEditing.Feed.Descriptions);
            SetupCommandHooks(checkBoxNeedsTerminal, () => _feedEditing.Feed.NeedsTerminal, value => _feedEditing.Feed.NeedsTerminal = value);
            
            SetupCommandHooks(comboBoxMinInjectorVersion, () => _feedEditing.Feed.MinInjectorVersion, value => _feedEditing.Feed.MinInjectorVersion = (value == null ? null : value.ToString()));
        }

        /// <summary>
        /// Sets up event handlers for <see cref="_feedEditing"/> integration.
        /// </summary>
        private void InitializeEditingHooks()
        {
            _feedEditing.Update += OnUpdate;
            _feedEditing.UndoEnabled += value => buttonUndo.Enabled = value;
            _feedEditing.RedoEnabled += value => buttonRedo.Enabled = value;

            buttonUndo.Enabled = buttonRedo.Enabled = false;
        }

        #endregion

        #region Undo/Redo

        private void OnUpdate()
        {
            FillForm();
            if (Populate != null) Populate();
        }

        /// <summary>
        /// Hooks up a <see cref="UriTextBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the coressponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the coressponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(UriTextBox textBox, SimpleResult<Uri> getValue, Action<Uri> setValue)
        {
            // Transfer data from the feed to the TextBox when refreshing
            Populate += delegate
            {
                textBox.CausesValidation = false;
                textBox.Uri = getValue();
                textBox.CausesValidation = true;
            };

            // Transfer data from the TextBox to the feed via a command object
            textBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (textBox.Uri == getValue()) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<Uri>(textBox.Uri, getValue, setValue));
            };

            // Enable the undo button even before the command has been created
            textBox.KeyPress += delegate
            {
                try { _feedEditing.UpdateButtonStatus(textBox.Uri != getValue()); }
                catch (UriFormatException) {}
            };
            textBox.ClearButtonClicked += delegate
            {
                try { _feedEditing.UpdateButtonStatus(textBox.Uri != getValue()); }
                catch (UriFormatException) {}
            };
        }
        
        /// <summary>
        /// Hooks up a <see cref="HintTextBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the coressponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the coressponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(HintTextBox textBox, SimpleResult<string> getValue, Action<string> setValue)
        {
            SetupCommandHooks((TextBox)textBox, getValue, setValue);

            // Enable the undo button even before the command has been created
            textBox.ClearButtonClicked += delegate { _feedEditing.UpdateButtonStatus(StringUtils.CompareEmptyNull(textBox.Text, getValue())); };
        }

        /// <summary>
        /// Hooks up a <see cref="TextBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the coressponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the coressponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(TextBox textBox, SimpleResult<string> getValue, Action<string> setValue)
        {
            // Transfer data from the feed to the TextBox when refreshing
            Populate += delegate
            {
                textBox.CausesValidation = false;
                textBox.Text = getValue();
                textBox.CausesValidation = true;
            };

            // Transfer data from the TextBox to the feed via a command object
            textBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (StringUtils.CompareEmptyNull(textBox.Text, getValue())) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<string>(textBox.Text, getValue, setValue));
            };

            // Enable the undo button even before the command has been created
            textBox.KeyPress += delegate { _feedEditing.UpdateButtonStatus(StringUtils.CompareEmptyNull(textBox.Text, getValue())); };
        }

        /// <summary>
        /// Hooks up a <see cref="CheckBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="checkBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the coressponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the coressponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(CheckBox checkBox, SimpleResult<bool> getValue, Action<bool> setValue)
        {
            // Transfer data from the feed to the CheckBox when refreshing
            Populate += delegate
            {
                checkBox.CausesValidation = false;
                checkBox.Checked = getValue();
                checkBox.CausesValidation = true;
            };

            // Transfer data from the CheckBox to the feed via a command object
            checkBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (checkBox.Checked == getValue()) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<bool>(checkBox.Checked, getValue, setValue));
            };
        }

        /// <summary>
        /// Hooks up a <see cref="CheckedListBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="checkedListBox">The <see cref="CheckedListBox"/> to track for input and to update.</param>
        /// <param name="getCollection">A delegate that reads the coressponding value from the <see cref="ICollection{T}"/>.</param>
        private void SetupCommandHooks(CheckedListBox checkedListBox, SimpleResult<ICollection<string>> getCollection)
        {
            ItemCheckEventHandler itemCheckEventHandler = (sender, e) =>
            {
                switch (e.NewValue)
                {
                    case CheckState.Checked:
                        _feedEditing.ExecuteCommand(new AddToCollection<string>(getCollection(), checkedListBox.Items[e.Index].ToString()));
                        break;
                    case CheckState.Unchecked:
                        _feedEditing.ExecuteCommand(new RemoveFromCollection<string>(getCollection(), checkedListBox.Items[e.Index].ToString()));
                        break;
                }
            };

            Populate += delegate
            {
                checkedListBox.ItemCheck -= itemCheckEventHandler;
                for(int i = 0; i < checkedListBox.Items.Count; i++)
                {
                    if(getCollection().Contains(checkedListBox.Items[i].ToString()))
                    {
                        checkedListBox.SetItemChecked(i, true);
                    }
                }
                checkedListBox.ItemCheck += itemCheckEventHandler;
            };

            checkedListBox.ItemCheck += itemCheckEventHandler;
        }

        /// <summary>
        /// Hooks up a <see cref="ComboBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="comboBox">The <see cref="ComboBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the coressponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the coressponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(ComboBox comboBox, SimpleResult<object> getValue, Action<object> setValue)
        {
            Populate += delegate
            {
                comboBox.CausesValidation = false;
                if (getValue() == null)
                    comboBox.SelectedItem = string.Empty;
                else
                    comboBox.SelectedItem = getValue();
                comboBox.CausesValidation = true;
            };

            comboBox.Validating += (sender, e) =>
            {
                if (e.Cancel) return;

                if (comboBox.SelectedItem == getValue()) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<object>(comboBox.SelectedItem, getValue, setValue));
            };

        }

        private void SetupCommandHooks(LocalizableTextControl localizableTextControl, SimpleResult<LocalizableStringCollection> getCollection)
        {

            localizableTextControl.Values.ItemsAdded += (sender, itemCountEventArgs) =>
            {
                _feedEditing.ExecuteCommand(new AddToCollection<LocalizableString>(getCollection(), itemCountEventArgs.Item));
            };

            localizableTextControl.Values.ItemsRemoved += (sender, itemCountEventArgs) =>
            {
                _feedEditing.ExecuteCommand(new RemoveFromCollection<LocalizableString>(getCollection(), itemCountEventArgs.Item));
            };

            Populate += delegate
            {
                localizableTextControl.Values = getCollection();
            };
        }
        #endregion

        #region Toolbar

        /// <summary>
        /// Sets all controls on the <see cref="MainForm"/> to default values.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ToolStripButtonNew_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            if (_feedEditing.Changed)
            {
                if (!AskSave()) return;
            }

            _feedEditing = new FeedEditing();
            OnUpdate();
            InitializeEditingHooks();
        }

        /// <summary>
        /// Shows a dialog to open a new <see cref="ZeroInstall.Model"/> for editing.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            if (_feedEditing.Changed)
            {
                if (!AskSave()) return;
            }

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _feedEditing = FeedEditing.Load(openFileDialog.FileName);
                }
                    #region Error handling

                catch (InvalidOperationException)
                {
                    Msg.Inform(this, "The feed you tried to open is not valid.", MsgSeverity.Error);
                }
                catch (UnauthorizedAccessException exception)
                {
                    Msg.Inform(this, exception.Message, MsgSeverity.Error);
                }
                catch (IOException exception)
                {
                    Msg.Inform(this, exception.Message, MsgSeverity.Error);
                }

                #endregion

                InitializeEditingHooks();

                OnUpdate();
            }
        }

        /// <summary>
        /// Shows a dialog to save the edited <see cref="ZeroInstall.Model"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ToolStripButtonSave_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            Save();
        }

        private void toolStripButtonSaveAs_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            SaveAs();
        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            _feedEditing.Undo();
        }

        private void buttonRedo_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            _feedEditing.Redo();
        }

        #endregion

        #region Save and open

        /// <summary>
        /// Saves feed to a specific path as xml.
        /// </summary>
        /// <param name="toPath">Path to save.</param>
        private void SaveFeed(string toPath)
        {
            SaveGeneralTab();
            SaveFeedTab();
            SaveAdvancedTab();

            _feedEditing.Save(toPath);
            SignFeed(toPath);
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageGeneral"/>.
        /// </summary>
        private void SaveGeneralTab()
        {
            _feedEditing.Feed.Icons.Clear();
            foreach (Model.Icon icon in listBoxIconsUrls.Items) _feedEditing.Feed.Icons.Add(icon);
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageFeed"/>.
        /// </summary>
        private static void SaveFeedTab()
        {
            // the feed structure objects will be saved directly into _feedEditing.Feed => no extra saving needed.
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageAdvanced"/>.
        /// </summary>
        private void SaveAdvancedTab()
        {
            _feedEditing.Feed.Feeds.Clear();
            foreach (var feed in listBoxExternalFeeds.Items) _feedEditing.Feed.Feeds.Add((FeedReference) feed);

            _feedEditing.Feed.FeedFor.Clear();
            foreach (InterfaceReference feedFor in listBoxFeedFor.Items) _feedEditing.Feed.FeedFor.Add(feedFor);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_feedEditing.Changed)
            {
                if (!AskSave()) e.Cancel = true;
            }
        }

        /// <summary>
        /// Asks the user whether he wants to save the feed.
        /// </summary>
        /// <returns><see langword="true"/> if all went well (either Yes or No), <see langword="false"/> if the user chose to cancel.</returns>
        private bool AskSave()
        {
            switch (
                Msg.Choose(this, "Do you want to save the changes you made?", MsgSeverity.Information, true,
                           "&Save\nSave the file and then close", "&Don't save\nIgnore the unsaved changes"))
            {
                case DialogResult.Yes:
                    return Save();

                case DialogResult.No:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Saves the feed at at a new location.
        /// </summary>
        /// <returns>The result of the "Save as" common dialog box used.</returns>
        /// <returns><see langword="true"/> if all went well, <see langword="false"/> if the user chose to cancel.</returns>
        private bool SaveAs()
        {
            saveFileDialog.FileName = _feedEditing.Path;
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                SaveFeed(_feedEditing.Path);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves the feed at its original location.
        /// </summary>
        /// <returns><see langword="true"/> if all went well, <see langword="false"/> if the user chose to cancel.</returns>
        private bool Save()
        {
            if (string.IsNullOrEmpty(_feedEditing.Path)) return SaveAs();
            else
            {
                SaveFeed(_feedEditing.Path);
                return true;
            }
        }

        /// <summary>
        /// Asks the user for his/her GnuPG passphrase and adds the Base64 signature of the given file to the end of it.
        /// </summary>
        /// <param name="path">The feed file to sign.</param>
        private void SignFeed(string path)
        {
            bool wrongPassphrase = false;

            if (toolStripComboBoxGpg.Text == string.Empty) return;
            var key = (GnuPGSecretKey) toolStripComboBoxGpg.SelectedItem;
            do
            {
                string passphrase = InputBox.Show(
                    (wrongPassphrase
                         ? "Wrong passphrase entered.\nPlease retry entering the GnuPG passphrase for "
                         : "Please enter the GnuPG passphrase for ") + key.UserID,
                    "Enter GnuPG passphrase", String.Empty, true);

                if (passphrase == null) return;

                try
                {
                    FeedUtils.SignFeed(path, key.KeyID, passphrase);
                }
                catch (WrongPassphraseException)
                {
                    wrongPassphrase = true;
                }
            } while (wrongPassphrase);
        }

        #endregion

        #region Fill form controls

        /// <summary>
        /// Clears all form controls and fills them with the values from a <see cref="Feed"/>.
        /// </summary>
        private void FillForm()
        {
            ResetFormControls();

            FillGeneralTab();
            FillFeedTab();
            FillAdvancedTab();
        }

        /// <summary>
        /// Fills the <see cref="tabPageGeneral"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        private void FillGeneralTab()
        {
            // fill icons list box
            listBoxIconsUrls.BeginUpdate();
            listBoxIconsUrls.Items.Clear();
            foreach (var icon in _feedEditing.Feed.Icons) listBoxIconsUrls.Items.Add(icon);
            listBoxIconsUrls.EndUpdate();
        }

        /// <summary>
        /// Fills the <see cref="tabPageFeed"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        private void FillFeedTab()
        {
            treeViewFeedStructure.BeginUpdate();
            treeViewFeedStructure.Nodes[0].Nodes.Clear();
            treeViewFeedStructure.Nodes[0].Tag = _feedEditing.Feed;
            BuildElementsTreeNodes(_feedEditing.Feed.Elements, treeViewFeedStructure.Nodes[0]);
            treeViewFeedStructure.EndUpdate();

            treeViewFeedStructure.ExpandAll();
        }

        /// <summary>
        /// Fills the <see cref="tabPageAdvanced"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        private void FillAdvancedTab()
        {
            foreach (var feed in _feedEditing.Feed.Feeds) listBoxExternalFeeds.Items.Add(feed);
            foreach (var feedFor in _feedEditing.Feed.FeedFor) listBoxFeedFor.Items.Add(feedFor);
        }

        #endregion

        #region Reset form controls

        /// <summary>
        /// Sets all controls on the <see cref="MainForm"/> to default values.
        /// </summary>
        private void ResetFormControls()
        {
            ResetGeneralTabControls();
            ResetFeedTabControls();
            ResetAdvancedTabControls();
        }

        /// <summary>
        /// Sets all controls on <see cref="tabPageGeneral"/> to default values.
        /// </summary>
        private void ResetGeneralTabControls()
        {
            hintTextBoxIconUrl.ResetText();
            comboBoxIconType.SelectedIndex = 0;
            pictureBoxIconPreview.Image = null;
            listBoxIconsUrls.Items.Clear();
            lblIconUrlError.ResetText();
        }

        /// <summary>
        /// Sets all controls on <see cref="tabPageFeed"/> to default values.
        /// </summary>
        private void ResetFeedTabControls()
        {
            treeViewFeedStructure.Nodes[0].Nodes.Clear();
        }

        /// <summary>
        /// Sets all controls on <see cref="tabPageAdvanced"/> to default values.
        /// </summary>
        private void ResetAdvancedTabControls()
        {
            listBoxExternalFeeds.Items.Clear();
            hintTextBoxFeedFor.Clear();
            listBoxFeedFor.Items.Clear();
            feedReferenceControl.FeedReference = null;
        }

        #endregion

        #region Tabs

        #region General Tab

        #region Icon Group

        /// <summary>
        /// Tries to download the image with the url from <see cref="hintTextBoxIconUrl"/> and shows it in <see cref="pictureBoxIconPreview"/>.
        /// Sets the right <see cref="ImageFormat"/> from the downloaded image in <see cref="comboBoxIconType"/>.
        /// Error messages will be shown in <see cref="lblIconUrlError"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnIconPreviewClick(object sender, EventArgs e)
        {
            Uri iconUrl;
            Image icon;
            lblIconUrlError.Text = "Downloading image for preview...";
            lblIconUrlError.ForeColor = Color.Black;
            // check url
            if (!Feed.IsValidUrl(hintTextBoxIconUrl.Text, out iconUrl)) return;

            // try downloading image
            try
            {
                icon = GetImageFromUrl(iconUrl);
            }
            catch (WebException ex)
            {
                lblIconUrlError.ForeColor = Color.Red;
                switch (ex.Status)
                {
                    case WebExceptionStatus.ConnectFailure:
                        lblIconUrlError.Text = "File not found on the web server.";
                        break;
                    default:
                        lblIconUrlError.Text = "Couldn't download image.";
                        break;
                }
                return;
            }
            catch (ArgumentException)
            {
                lblIconUrlError.ForeColor = Color.Red;
                lblIconUrlError.Text = "URL does not describe an image.";
                return;
            }

            if (!IsIconFormatSupported(icon.RawFormat))
            {
                lblIconUrlError.ForeColor = Color.Red;
                lblIconUrlError.Text = "Image format not supported by 0install.";
                return;
            }

            lblIconUrlError.Text = "Image downloaded.";
            comboBoxIconType.SelectedItem = icon.RawFormat;
            pictureBoxIconPreview.Image = icon;
        }

        private static Image GetImageFromUrl(Uri url)
        {
            var fileRequest = (HttpWebRequest) WebRequest.Create(url);
            var fileReponse = (HttpWebResponse) fileRequest.GetResponse();
            Stream stream = fileReponse.GetResponseStream();
            return Image.FromStream(stream);
        }

        /// <summary>
        /// Checks if the <see cref="ImageFormat"/> is supported by 0install.
        /// </summary>
        /// <param name="toCheck"><see cref="ImageFormat"/> to check.</param>
        /// <returns>true, if supported, else false.</returns>
        private static bool IsIconFormatSupported(ImageFormat toCheck)
        {
            var supportedImageFormats = new C5.HashSet<ImageFormat>() {ImageFormat.Png, ImageFormat.Icon};
            return supportedImageFormats.Contains(toCheck);
        }

        /// <summary>
        /// Adds the url from <see cref="hintTextBoxIconUrl"/> and the chosen mime type in <see cref="comboBoxIconType"/>
        /// to <see cref="listBoxIconsUrls"/> if the url is a valid url.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnIconListAddClick(object sender, EventArgs e)
        {
            Uri uri;
            if (!Feed.IsValidUrl(hintTextBoxIconUrl.Text, out uri)) return;
            var icon = new Model.Icon {Location = uri};

            var imageMimeTypes = new C5.HashDictionary<Guid, string>
                                     {
                                         {ImageFormat.Png.Guid, "image/png"},
                                         {ImageFormat.Icon.Guid, "image/vnd-microsoft-icon"}
                                     };
            icon.MimeType = imageMimeTypes[((ImageFormat) comboBoxIconType.SelectedItem).Guid];

            if (!listBoxIconsUrls.Items.Contains(icon)) listBoxIconsUrls.Items.Add(icon);
        }

        /// <summary>
        /// Removes chosen image url in <see cref="listBoxIconsUrls"/> from the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnIconListRemoveClick(object sender, EventArgs e)
        {
            var icon = listBoxIconsUrls.SelectedItem;
            if (listBoxIconsUrls.SelectedItem == null) return;
            listBoxIconsUrls.Items.Remove(icon);
        }

        /// <summary>
        /// Replaces the text and the icon type of <see cref="hintTextBoxIconUrl"/> and <see cref="comboBoxIconType"/> with the text and the icon type from the chosen entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ListIconsUrlsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxIconsUrls.SelectedItem == null) return;

            var icon = (Model.Icon) listBoxIconsUrls.SelectedItem;
            hintTextBoxIconUrl.Text = icon.LocationString;

            switch (icon.MimeType)
            {
                case null:
                    comboBoxIconType.Text = String.Empty;
                    break;
                case "image/png":
                    comboBoxIconType.SelectedItem = ImageFormat.Png;
                    break;
                case "image/vnd-microsoft-icon":
                    comboBoxIconType.SelectedItem = ImageFormat.Icon;
                    break;
                default:
                    comboBoxIconType.SelectedText = "unsupported";
                    break;
            }
        }

        /// <summary>
        /// Checks if the text in <see cref="hintTextBoxIconUrl"/> is a valid url and enables the <see cref="buttonIconPreview"/> if true and disables it if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TextIconUrlTextChanged(object sender, EventArgs e)
        {
            if (Feed.IsValidUrl(hintTextBoxIconUrl.Text))
            {
                hintTextBoxIconUrl.ForeColor = Color.Green;
                buttonIconPreview.Enabled = true;
            }
            else
            {
                hintTextBoxIconUrl.ForeColor = Color.Red;
                buttonIconPreview.Enabled = false;
            }
        }

        #endregion

        #endregion

        #region Feed Tab

        #region Add buttons clicked

        /// <summary>
        /// Detects the feed structure button which was clicked and adds the accordent feed structure elemtent to <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <param name="sender"><see cref="Button"/> which was clicked.</param>
        /// <param name="e">Not used.</param>
        private void AddFeedStructureElementButtonClick(object sender, EventArgs e)
        {
            if (((Button) sender).Tag is Group) AddFeedStructureObject(new Group());
            else if (((Button) sender).Tag is Dependency) AddFeedStructureObject(new Dependency());
            else if (((Button) sender).Tag is EnvironmentBinding) AddFeedStructureObject(new EnvironmentBinding());
            else if (((Button) sender).Tag is OverlayBinding) AddFeedStructureObject(new OverlayBinding());
            else if (((Button) sender).Tag is WorkingDirBinding) AddFeedStructureObject(new WorkingDirBinding());
            else if (((Button) sender).Tag is PackageImplementation) AddFeedStructureObject(new PackageImplementation());
            else if (((Button) sender).Tag is Implementation) AddFeedStructureObject(new Implementation());
            else if (((Button) sender).Tag is Archive) AddFeedStructureObject(new Archive());
            else if (((Button) sender).Tag is Recipe) AddFeedStructureObject(new Recipe());
        }

        /// <summary>
        /// Gets the selected <see cref="TreeNode"/> from <see cref="treeViewFeedStructure"/> and adds (if allowed) <paramref name="feedStructureObject"/> to the container stored
        /// in the Tag of the selected <see cref="TreeNode"/>. Then <see cref="treeViewFeedStructure"/> will be rebuild from <see cref="Feed"/>.
        /// </summary>
        /// <remarks>
        /// It is allowed if <paramref name="feedStructureObject"/> can be added to the selected container (e.g. a <see cref="Implementation"/> can be added to a
        /// <see cref="Group"/>).
        /// See http://0install.net/interface-spec.html for more information.
        /// </remarks>
        /// <param name="feedStructureObject"></param>
        private void AddFeedStructureObject(object feedStructureObject)
        {
            var firstFeedStructureNode = treeViewFeedStructure.Nodes[0];
            var selectedNode = treeViewFeedStructure.SelectedNode ?? firstFeedStructureNode;

            if (selectedNode.Tag is Feed)
            {
                var feed = (Feed) selectedNode.Tag;

                if (feedStructureObject is Group)
                {
                    feed.Elements.Add((Group) feedStructureObject);
                }
                else if (feedStructureObject is Implementation)
                {
                    feed.Elements.Add((Implementation) feedStructureObject);
                }
                else if (feedStructureObject is PackageImplementation)
                {
                    feed.Elements.Add((PackageImplementation) feedStructureObject);
                }
            }
            else if (selectedNode.Tag is Group)
            {
                var group = (Group) selectedNode.Tag;

                if (feedStructureObject is Dependency)
                {
                    group.Dependencies.Add((Dependency) feedStructureObject);
                }
                else if (feedStructureObject is EnvironmentBinding)
                {
                    group.Bindings.Add((EnvironmentBinding) feedStructureObject);
                }
                else if (feedStructureObject is Group)
                {
                    group.Elements.Add((Group) feedStructureObject);
                }
                else if (feedStructureObject is Implementation)
                {
                    group.Elements.Add((Implementation) feedStructureObject);
                }
                else if (feedStructureObject is OverlayBinding)
                {
                    group.Bindings.Add((OverlayBinding) feedStructureObject);
                }
                else if (feedStructureObject is PackageImplementation)
                {
                    group.Elements.Add((PackageImplementation) feedStructureObject);
                }
                else if ((feedStructureObject is WorkingDirBinding))
                {
                    group.Bindings.Add((WorkingDirBinding) feedStructureObject);
                }
            }
            else if (selectedNode.Tag is Implementation)
            {
                var implementation = (Implementation) selectedNode.Tag;

                if (feedStructureObject is Archive)
                {
                    implementation.RetrievalMethods.Add((Archive) feedStructureObject);
                }
                else if (feedStructureObject is Recipe)
                {
                    implementation.RetrievalMethods.Add((Recipe) feedStructureObject);
                }
                else if (feedStructureObject is Dependency)
                {
                    implementation.Dependencies.Add((Dependency) feedStructureObject);
                }
                else if (feedStructureObject is EnvironmentBinding)
                {
                    implementation.Bindings.Add((EnvironmentBinding) feedStructureObject);
                }
                else if (feedStructureObject is OverlayBinding)
                {
                    implementation.Bindings.Add((OverlayBinding) feedStructureObject);
                }
                else if ((feedStructureObject is WorkingDirBinding))
                {
                    implementation.Bindings.Add((WorkingDirBinding) feedStructureObject);
                }
            }
            else if (selectedNode.Tag is PackageImplementation)
            {
                var packageImplementation = (PackageImplementation) selectedNode.Tag;
                if (feedStructureObject is Dependency)
                {
                    packageImplementation.Dependencies.Add((Dependency) feedStructureObject);
                }
                else if (feedStructureObject is EnvironmentBinding)
                {
                    packageImplementation.Bindings.Add((EnvironmentBinding) feedStructureObject);
                }
                else if (feedStructureObject is OverlayBinding)
                {
                    packageImplementation.Bindings.Add((OverlayBinding) feedStructureObject);
                }
                else if ((feedStructureObject is WorkingDirBinding))
                {
                    packageImplementation.Bindings.Add((WorkingDirBinding) feedStructureObject);
                }
            }
            else if (selectedNode.Tag is Dependency)
            {
                var dependecy = (Dependency) selectedNode.Tag;
                if (feedStructureObject is EnvironmentBinding)
                {
                    dependecy.Bindings.Add((EnvironmentBinding) feedStructureObject);
                }
                else if (feedStructureObject is OverlayBinding)
                {
                    dependecy.Bindings.Add((OverlayBinding) feedStructureObject);
                }
                else if ((feedStructureObject is WorkingDirBinding))
                {
                    dependecy.Bindings.Add((WorkingDirBinding) feedStructureObject);
                }
            }

            FillFeedTab();
        }

        #endregion

        #region treeviewFeedStructure methods

        /// <summary>
        /// Enables the buttons which allow the user to add specific new <see cref="TreeNode"/>s in subject to the selected <see cref="TreeNode"/>.
        /// For example: The user selected a "Dependency"-node. Now only the buttons <see cref="btnAddEnvironmentBinding"/> and <see cref="btnAddOverlayBinding"/> will be enabled.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TreeViewFeedStructureAfterSelect(object sender, TreeViewEventArgs e)
        {
            var selectedNodeTag = treeViewFeedStructure.SelectedNode.Tag;

            Button[] addButtons = {
                                      btnAddGroup, btnAddImplementation, buttonAddArchive, buttonAddRecipe,
                                      btnAddPackageImplementation, btnAddDependency, btnAddEnvironmentBinding,
                                      btnAddOverlayBinding, btnAddWorkingDir
                                  };
            Button[] enableAddButtons = null;
            // disable all addButtons
            foreach (var button in addButtons) button.Enabled = false;

            // mark the addButtons that can be selected
            if (selectedNodeTag is Feed)
            {
                enableAddButtons = new[] {btnAddGroup, btnAddImplementation, btnAddPackageImplementation};
            }
            else if (selectedNodeTag is Group)
            {
                enableAddButtons = new[]
                                       {
                                           btnAddGroup, btnAddImplementation, btnAddPackageImplementation,
                                           btnAddDependency
                                           , btnAddEnvironmentBinding, btnAddOverlayBinding, btnAddWorkingDir
                                       };
            }
            else if (selectedNodeTag is PackageImplementation)
            {
                enableAddButtons = new[]
                                       {
                                           btnAddDependency, btnAddEnvironmentBinding, btnAddOverlayBinding,
                                           btnAddWorkingDir
                                       };
            }
            else if (selectedNodeTag is Implementation)
            {
                enableAddButtons = new[]
                                       {
                                           buttonAddArchive, buttonAddRecipe, btnAddDependency, btnAddEnvironmentBinding
                                           ,
                                           btnAddOverlayBinding, btnAddWorkingDir
                                       };
            }
            else if (selectedNodeTag is Dependency)
            {
                enableAddButtons = new[] {btnAddEnvironmentBinding, btnAddOverlayBinding, btnAddWorkingDir};
            }

            // enable marked buttons
            if (enableAddButtons == null) return;
            for (int i = 0; i < enableAddButtons.Length; i++)
                enableAddButtons[i].Enabled = true;
        }

        /// <summary>
        /// Opens a new window to edit the selected entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TreeViewFeedStructureNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.Nodes[0]) return;

            selectedNode.Toggle();

            // show a dialog to change the selected object
            if (selectedNode.Tag is Group) (new GroupForm {Group = (Group) selectedNode.Tag}).ShowDialog();
            else if (selectedNode.Tag is Implementation)
                (new ImplementationForm {Implementation = (Implementation) selectedNode.Tag}).ShowDialog();
            else if (selectedNode.Tag is Archive)
            {
                var archiveForm = new ArchiveForm {Archive = (Archive) selectedNode.Tag};
                if (archiveForm.ShowDialog() != DialogResult.OK) return;

                var manifestDigestFromArchive = archiveForm.ManifestDigest;
                var implementationNode = selectedNode.Parent;

                if (implementationNode.FirstNode.Tag is ManifestDigest)
                {
                    var existingManifestDigest = (ManifestDigest) implementationNode.FirstNode.Tag;
                    if (ControlHelpers.IsEmpty(existingManifestDigest))
                    {
                        implementationNode.FirstNode.Tag = manifestDigestFromArchive;
                        ((Implementation) implementationNode.Tag).ManifestDigest = manifestDigestFromArchive;
                    }
                    else if (
                        !ControlHelpers.CompareManifestDigests(existingManifestDigest, manifestDigestFromArchive))
                    {
                        Msg.Inform(this,
                                   "The manifest digest of this archive is not the same as the manifest digest of the other archives. The archive was discarded.",
                                   MsgSeverity.Warning);
                        selectedNode.Tag = new Archive();
                        return;
                    }
                }
                else
                {
                    InsertManifestDigestNode(implementationNode, manifestDigestFromArchive);
                }
                var implementation = (Implementation) implementationNode.Tag;

                if (String.IsNullOrEmpty(implementation.ID) || implementation.ID.StartsWith("sha1new="))
                {
                    implementation.ID = "sha1new=" + manifestDigestFromArchive.Sha1New;
                }
            }
            else if (selectedNode.Tag is Recipe)
            {
                var recipeForm = new RecipeForm {Recipe = (Recipe) selectedNode.Tag};
                if (recipeForm.ShowDialog() != DialogResult.OK) return;

                var manifestDigestFromRecipe = recipeForm.ManifestDigest;
                var implementationNode = selectedNode.Parent;

                if (implementationNode.FirstNode.Tag is ManifestDigest)
                {
                    var existingManifestDigest = (ManifestDigest) implementationNode.FirstNode.Tag;
                    if (ControlHelpers.IsEmpty(existingManifestDigest))
                    {
                        implementationNode.FirstNode.Tag = manifestDigestFromRecipe;
                        ((Implementation) implementationNode.Tag).ManifestDigest = manifestDigestFromRecipe;
                    }
                    else if (!ControlHelpers.CompareManifestDigests(existingManifestDigest, manifestDigestFromRecipe))
                    {
                        Msg.Inform(this,
                                   "The manifest digest of this recipe is not the same as the manifest digest of the other retrieval methods. The recipe was discarded.",
                                   MsgSeverity.Warning);
                        selectedNode.Tag = new Recipe {Steps = {new Archive()}};
                        return;
                    }
                }
                else
                {
                    InsertManifestDigestNode(selectedNode.Parent, manifestDigestFromRecipe);
                }
            }
            else if (selectedNode.Tag is PackageImplementation)
                (new PackageImplementationForm
                     {PackageImplementation = (PackageImplementation) selectedNode.Tag}).ShowDialog();
            else if (selectedNode.Tag is Dependency)
                (new DependencyForm {Dependency = (Dependency) selectedNode.Tag}).ShowDialog();
            else if (selectedNode.Tag is EnvironmentBinding)
                (new EnvironmentBindingForm {EnvironmentBinding = (EnvironmentBinding) selectedNode.Tag})
                    .ShowDialog();
            else if (selectedNode.Tag is OverlayBinding)
                (new OverlayBindingForm {OverlayBinding = (OverlayBinding) selectedNode.Tag}).
                    ShowDialog();
            else if (selectedNode.Tag is ManifestDigest)
                (new ManifestDigestForm((ManifestDigest) selectedNode.Tag)).ShowDialog();
            else if (selectedNode.Tag is WorkingDirBinding)
                (new WorkingDirBindingForm()
                     {WorkingDirBinding = (WorkingDirBinding) selectedNode.Tag}).ShowDialog();
            else throw new InvalidOperationException("Not an object to change.");
            FillFeedTab();
        }

        /// <summary>
        /// Inserts a new <see cref="TreeNode"/> to the first position of <paramref name="insertInto"/>.
        /// Adds <paramref name="toAddToTag"/> to the Tag of this <see cref="TreeNode"/>.
        /// </summary>
        /// <param name="insertInto">New <see cref="TreeNode"/> will be inserted to this <see cref="TreeNode"/>s first position.</param>
        /// <param name="toAddToTag"><see cref="ManifestDigest"/> to add to the Tag of the new <see cref="TreeNode"/>.</param>
        private static void InsertManifestDigestNode(TreeNode insertInto, ManifestDigest toAddToTag)
        {
            var manifestDigestNode = new TreeNode("Manifest digest") {Tag = toAddToTag};
            ((Implementation) insertInto.Tag).ManifestDigest = toAddToTag;
            insertInto.Nodes.Insert(0, manifestDigestNode);
        }

        /// <summary>
        /// Removes the Tag of the selected <see cref="TreeNode"/> from <see cref="Feed"/> and rebuilds <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnRemoveFeedStructureObjectClick(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.Nodes[0]) return;
            RemoveObjectFromFeedStructure(selectedNode.Parent.Tag, selectedNode.Tag);
            FillFeedTab();
            treeViewFeedStructure.SelectedNode = treeViewFeedStructure.Nodes[0];
            TreeViewFeedStructureAfterSelect(null, null);
        }

        /// <summary>
        /// Removes an feed structure object from <see cref="Feed"/>.
        /// </summary>
        /// <param name="container">Not used.</param>
        /// <param name="toRemove">Not used.</param>
        private static void RemoveObjectFromFeedStructure(object container, object toRemove)
        {
            if (toRemove is Element) ((IElementContainer) container).Elements.Remove((Element) toRemove);
            else if (toRemove is Dependency) ((Element) container).Dependencies.Remove((Dependency) toRemove);
            else if (toRemove is Binding) ((IBindingContainer) container).Bindings.Remove((Binding) toRemove);
            else if (toRemove is RetrievalMethod)
            {
                var implementationContainer = (Implementation) container;
                implementationContainer.RetrievalMethods.Remove((RetrievalMethod) toRemove);
                if (implementationContainer.RetrievalMethods.Count == 0)
                {
                    implementationContainer.ManifestDigest = default(ManifestDigest);
                    if (implementationContainer.ID != null && implementationContainer.ID.StartsWith("sha1new="))
                        implementationContainer.ID = String.Empty;
                }
            }
        }

        /// <summary>
        /// Removes all feed structure objects from <see cref="Feed"/> and rebuilds <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonClearListClick(object sender, EventArgs e)
        {
            _feedEditing.Feed.Elements.Clear();
            FillFeedTab();
            treeViewFeedStructure.SelectedNode = treeViewFeedStructure.Nodes[0];
            TreeViewFeedStructureAfterSelect(null, null);
        }

        private static void BuildElementsTreeNodes(IEnumerable<Element> elements, TreeNode parentNode)
        {
            #region Sanity checks

            if (elements == null) throw new ArgumentNullException("elements");

            #endregion

            foreach (var element in elements)
            {
                var group = element as Group;
                if (group != null)
                {
                    var groupNode = new TreeNode(group.ToString()) {Tag = group};
                    parentNode.Nodes.Add(groupNode);
                    BuildElementsTreeNodes(group.Elements, groupNode);
                    BuildDependencyTreeNodes(group.Dependencies, groupNode);
                    BuildBindingTreeNodes(group.Bindings, groupNode);
                }
                else
                {
                    var implementation = element as Implementation;
                    if (implementation != null)
                    {
                        var implementationNode = new TreeNode(implementation.ToString()) {Tag = implementation};
                        parentNode.Nodes.Add(implementationNode);
                        BuildManifestDigestTreeNode(implementation.ManifestDigest, implementationNode);
                        BuildRetrievalMethodsTreeNodes(implementation.RetrievalMethods, implementationNode);
                        BuildDependencyTreeNodes(implementation.Dependencies, implementationNode);
                        BuildBindingTreeNodes(implementation.Bindings, implementationNode);
                    }
                    else
                    {
                        var packageImplementation = element as PackageImplementation;
                        if (packageImplementation != null)
                        {
                            var packageImplementationNode = new TreeNode(packageImplementation.ToString())
                                                                {Tag = packageImplementation};
                            parentNode.Nodes.Add(packageImplementationNode);
                            BuildDependencyTreeNodes(packageImplementation.Dependencies, packageImplementationNode);
                            BuildBindingTreeNodes(packageImplementation.Bindings, packageImplementationNode);
                        }
                    }
                }
            }
        }

        private static void BuildBindingTreeNodes(IEnumerable<Binding> bindings, TreeNode parentNode)
        {
            #region Sanity checks

            if (bindings == null) throw new ArgumentNullException("bindings");

            #endregion

            foreach (var binding in bindings)
            {
                var bindingNode = new TreeNode(binding.ToString()) {Tag = binding};
                parentNode.Nodes.Add(bindingNode);
            }
        }

        private static void BuildManifestDigestTreeNode(ManifestDigest manifestDigest, TreeNode parentNode)
        {
            if (ControlHelpers.IsEmpty(manifestDigest)) return;
            var manifestDigestNode = new TreeNode("Manifest digest") {Tag = manifestDigest};
            parentNode.Nodes.Insert(0, manifestDigestNode);
        }

        private static void BuildDependencyTreeNodes(IEnumerable<Dependency> dependencies, TreeNode parentNode)
        {
            #region Sanity checks

            if (dependencies == null) throw new ArgumentNullException("dependencies");

            #endregion

            foreach (var dependency in dependencies)
            {
                string constraints = String.Empty;
                foreach (var constraint in dependency.Constraints) constraints += constraint.ToString();

                var dependencyNode = new TreeNode(string.Format("{0} {1}", dependency, constraints)) {Tag = dependency};
                parentNode.Nodes.Add(dependencyNode);
                BuildBindingTreeNodes(dependency.Bindings, dependencyNode);
            }
        }

        private static void BuildRetrievalMethodsTreeNodes(IEnumerable<RetrievalMethod> retrievalMethods,
                                                           TreeNode parentNode)
        {
            #region Sanity checks

            if (retrievalMethods == null) throw new ArgumentNullException("retrievalMethods");

            #endregion

            foreach (var retrievalMethod in retrievalMethods)
            {
                var retrievalMethodNode = new TreeNode(retrievalMethod.ToString()) {Tag = retrievalMethod};
                parentNode.Nodes.Add(retrievalMethodNode);
            }
        }

        #endregion

        #endregion

        #region Advanced Tab

        #region External Feeds Group

        /// <summary>
        /// Adds a clone of <see cref="FeedReference"/> from <see cref="feedReferenceControl"/> to <see cref="listBoxExternalFeeds"/> if no equal object is in the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnExtFeedsAddClick(object sender, EventArgs e)
        {
            var feedReference = feedReferenceControl.FeedReference.CloneFeedPreferences();
            if (string.IsNullOrEmpty(feedReference.Source)) return;
            foreach (FeedReference feedReferenceFromListBox in listBoxExternalFeeds.Items)
            {
                if (feedReference.Equals(feedReferenceFromListBox)) return;
            }
            listBoxExternalFeeds.Items.Add(feedReference);
        }

        /// <summary>
        /// Removes the selected <see cref="FeedReference"/> from <see cref="listBoxExternalFeeds"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnExtFeedsRemoveClick(object sender, EventArgs e)
        {
            var selectedItem = listBoxExternalFeeds.SelectedItem;
            if (selectedItem == null) return;
            listBoxExternalFeeds.Items.Remove(selectedItem);
        }

        /// <summary>
        /// Loads a clone of the selected <see cref="FeedReference"/> from <see cref="listBoxExternalFeeds"/> into <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ListBoxExtFeedsSelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (FeedReference) listBoxExternalFeeds.SelectedItem;
            if (selectedItem == null) return;
            feedReferenceControl.FeedReference = selectedItem.CloneFeedPreferences();
        }

        /// <summary>
        /// Updates the selected <see cref="FeedReference"/> in <see cref="listBoxExternalFeeds"/> with the new values from <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnExtFeedUpdateClick(object sender, EventArgs e)
        {
            var selectedFeedReferenceIndex = listBoxExternalFeeds.SelectedIndex;
            var feedReference = feedReferenceControl.FeedReference.CloneFeedPreferences();
            if (selectedFeedReferenceIndex < 0) return;
            if (String.IsNullOrEmpty(feedReference.Source)) return;
            listBoxExternalFeeds.Items[selectedFeedReferenceIndex] = feedReference;
        }

        #endregion

        #region FeedFor Group

        /// <summary>
        /// Adds a new <see cref="InterfaceReference"/> with the Uri from <see cref="hintTextBoxFeedFor"/> to <see cref="listBoxFeedFor"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnFeedForAddClick(object sender, EventArgs e)
        {
            Uri uri;
            if (!Feed.IsValidUrl(hintTextBoxFeedFor.Text, out uri)) return;
            var interfaceReference = new InterfaceReference {Target = uri};
            listBoxFeedFor.Items.Add(interfaceReference);
        }

        /// <summary>
        /// Removes the selected entry from <see cref="listBoxFeedFor"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnFeedForRemoveClick(object sender, EventArgs e)
        {
            var feedFor = listBoxFeedFor.SelectedItem;
            if (feedFor == null) return;
            listBoxFeedFor.Items.Remove(feedFor);
        }

        /// <summary>
        /// Clears <see cref="listBoxFeedFor"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnFeedForClearClick(object sender, EventArgs e)
        {
            listBoxFeedFor.Items.Clear();
        }

        #endregion

        #endregion

        #endregion
    }
}