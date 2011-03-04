/*
 * Copyright 2010-2011 Simon E. Silva Lauinger, Bastian Eicher
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
using ZeroInstall.Publish.WinForms.FeedStructure;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Publish.WinForms.Controls;
using Binding = ZeroInstall.Model.Binding;
using Icon = ZeroInstall.Model.Icon;

namespace ZeroInstall.Publish.WinForms
{
    public partial class MainForm : Form
    {
        #region Events
        /// <summary>To be called when the controls on the form need to filled with content from the feed.</summary>
        private event SimpleEventHandler Populate;

        /// <summary>To be called when the <see cref="treeViewFeedStructure"/> on the form need to filled with content from the feed.</summary>
        private event SimpleEventHandler UpdateStructureButtons;
        #endregion

        #region Constants
        private const string FeedFileFilter = "Zero Install Feed (*.xml)|*.xml|All Files|*.*";
        private readonly string[] _supportedInjectorVersions = new[] { "", "0.31", "0.32", "0.33", "0.34",
            "0.35", "0.36", "0.37", "0.38", "0.39", "0.40", "0.41", "0.41.1", "0.42", "0.42.1", "0.43", "0.44", "0.45"};
        #endregion

        #region Variables
        private FeedEditing _feedEditing = new FeedEditing();
        #endregion

        #region Properties
        /// <summary>
        /// Returns part of the <see cref="Feed"/> currently selected in the <see cref="treeViewFeedStructure"/> or the <see cref="Feed"/> itself if nothing is selected.
        /// </summary>
        private object SelectedFeedStructureElement
        {
            get
            {
                return (treeViewFeedStructure.SelectedNode == null ? _feedEditing.Feed : treeViewFeedStructure.SelectedNode.Tag);
            }
        }

        /// <summary>
        /// Returns parent of <see cref="SelectedFeedStructureElement"/> or <see langword="null"/> if there is no parent.
        /// </summary>
        private object SelectedFeedStructureElementParent
        {
            get
            {
                var selectedNode = treeViewFeedStructure.SelectedNode;
                return (selectedNode == null || selectedNode.Parent == null ? null : treeViewFeedStructure.SelectedNode.Parent.Tag);
            }
        }
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
        private void InitializeLoadFileDialog()
        {
            if (_feedEditing.Path != null) openFileDialog.InitialDirectory = _feedEditing.Path;
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = FeedFileFilter;
        }

        /// <summary>
        /// Adds <see cref="Feed"/> to the Tag of the first <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>.
        /// </summary>
        private void InitializeTreeViewFeedStructure()
        {
            treeViewFeedStructure.Nodes[0].Tag = _feedEditing.Feed;
        }

        /// <summary>
        /// Configures buttons to add and remove entries from the <see cref="treeViewFeedStructure"/> and the coressponding backend model.
        /// </summary>
        private void InitializeFeedStructureButtons()
        {
            SetupFeedStructureHooks<IElementContainer, Element, Implementation>(btnAddImplementation, implementation => new ImplementationForm {Implementation = implementation}, container => container.Elements);
            SetupFeedStructureHooks<IElementContainer, Element, PackageImplementation>(btnAddPackageImplementation, implementation => new PackageImplementationForm {PackageImplementation = implementation}, container => container.Elements);
            SetupFeedStructureHooks<IElementContainer, Element, Group>(btnAddGroup, group => new GroupForm {Group = group}, container => container.Elements);

            SetupFeedStructureHooks<IBindingContainer, Binding, EnvironmentBinding>(btnAddEnvironmentBinding, binding => new EnvironmentBindingForm {EnvironmentBinding = binding}, container => container.Bindings);
            SetupFeedStructureHooks<IBindingContainer, Binding, OverlayBinding>(btnAddOverlayBinding, binding => new OverlayBindingForm {OverlayBinding = binding}, container => container.Bindings);

            SetupFeedStructureHooks<IDependencyContainer, Dependency, Dependency>(btnAddDependency, dependency => new DependencyForm {Dependency = dependency}, container => container.Dependencies);

            SetupFeedStructureHooks<Element, Command, Command>(btnAddCommand, command => new CommandForm {Command = command}, element => element.Commands);
            //SetupFeedStructureHooks<Command, Runner, Runner>(btnAddRunner, runner => new RunnerForm {Runner = runner}, command => new PropertyPointer<Runner>(() => command.Runner, newValue => command.Runner = newValue));

            SetupFeedStructureHooks<Implementation, RetrievalMethod, Archive>(buttonAddArchive, archive => new ArchiveForm {Archive = archive}, implementation => implementation.RetrievalMethods);
            SetupFeedStructureHooks<Implementation, RetrievalMethod, Recipe>(buttonAddRecipe, recipe => new RecipeForm {Recipe = recipe}, implementation => implementation.RetrievalMethods);
        }

        /// <summary>
        /// Configures event handlers to add, remove and modify property elements visualized in <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <typeparam name="TContainer">A type that contains <typeparamref name="TAbstractEntry"/> child elements.</typeparam>
        /// <typeparam name="TAbstractEntry">The abstract super-type of <typeparamref name="TSpecialEntry"/>.</typeparam>
        /// <typeparam name="TSpecialEntry">The specific entry type to hook up for handling</typeparam>
        /// <param name="addButton">The button used to add new <typeparamref name="TSpecialEntry"/>s to <typeparamref name="TContainer"/>s.</param>
        /// <param name="getEditDialog">A delegate describing how to get a dialog to edit a <typeparamref name="TSpecialEntry"/>.</param>
        /// <param name="getPointer">A delegate describing how to get a pointer to read/write the value to be modified.</param>
        private void SetupFeedStructureHooks<TContainer, TAbstractEntry, TSpecialEntry>(Button addButton, MapAction<TSpecialEntry, Form> getEditDialog, MapAction<TContainer, PropertyPointer<TAbstractEntry>> getPointer)
            where TContainer : class
            where TAbstractEntry : class, ICloneable
            where TSpecialEntry : class, TAbstractEntry, new()
        {
            #region Sanity checks
            if (addButton == null) throw new ArgumentNullException("addButton");
            if (getEditDialog == null) throw new ArgumentNullException("getEditDialog");
            if (getPointer == null) throw new ArgumentNullException("getPointer");
            #endregion

            #region Add button
            addButton.Click += delegate
            {
                var container = SelectedFeedStructureElement as TContainer;
                if (container != null)
                {
                    // Set the value in the container
                    _feedEditing.ExecuteCommand(new SetValueCommand<TAbstractEntry>(getPointer(container), new TSpecialEntry()));
                    FillFeedTab();
                }
            };

            UpdateStructureButtons += delegate
            {
                var container = SelectedFeedStructureElement as TContainer;

                // Only enable add button if correct parent type is selected and there isn't already an element in place
                addButton.Enabled = (container != null && getPointer(container).Value == null);
            };
            #endregion

            #region Edit dialog
            treeViewFeedStructure.DoubleClick += delegate
            {
                var entry = SelectedFeedStructureElement as TSpecialEntry;
                var parent = SelectedFeedStructureElementParent as TContainer;
                if (entry != null && parent != null)
                {
                    // Clone entry for undoable modification
                    var clonedEntry = (TSpecialEntry)entry.Clone();

                    using (var dialog = getEditDialog(clonedEntry))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            _feedEditing.ExecuteCommand(new SetValueCommand<TAbstractEntry>(getPointer(parent), clonedEntry));

                            FillFeedTab();
                        }
                    }
                }
            };
            #endregion

            #region Remove button
            btnRemoveFeedStructureObject.Click += delegate
            {
                var entry = SelectedFeedStructureElement as TSpecialEntry;
                var parent = SelectedFeedStructureElementParent as TContainer;
                if (entry != null && parent != null)
                {
                    // Remove by setting value to null
                    _feedEditing.ExecuteCommand(new SetValueCommand<TAbstractEntry>(getPointer(parent), null));
                    FillFeedTab();
                }
            };
            #endregion
        }

        /// <summary>
        /// Configures event handlers to add, remove and modify collection elements visualized in <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <typeparam name="TContainer">A type that contains <typeparamref name="TAbstractEntry"/> child elements.</typeparam>
        /// <typeparam name="TAbstractEntry">The abstract super-type of <typeparamref name="TSpecialEntry"/>.</typeparam>
        /// <typeparam name="TSpecialEntry">The specific entry type to hook up for handling</typeparam>
        /// <param name="addButton">The button used to add new <typeparamref name="TSpecialEntry"/>s to <typeparamref name="TContainer"/>s.</param>
        /// <param name="getEditDialog">A delegate describing how to get a dialog to edit a <typeparamref name="TSpecialEntry"/>.</param>
        /// <param name="getList">A delegate describing how to get a collection of <typeparamref name="TAbstractEntry"/>s in a <typeparamref name="TContainer"/>.</param>
        private void SetupFeedStructureHooks<TContainer, TAbstractEntry, TSpecialEntry>(Button addButton, MapAction<TSpecialEntry, Form> getEditDialog, MapAction<TContainer, IList<TAbstractEntry>> getList)
            where TContainer : class
            where TAbstractEntry : class, ICloneable
            where TSpecialEntry : class, TAbstractEntry, new()
        {
            #region Sanity checks
            if (addButton == null) throw new ArgumentNullException("addButton");
            if (getEditDialog == null) throw new ArgumentNullException("getEditDialog");
            if (getList == null) throw new ArgumentNullException("getList");
            #endregion

            #region Add button
            addButton.Click += delegate
            {
                var container = SelectedFeedStructureElement as TContainer;
                if (container != null)
                {
                    // Add a new entry to the container
                    _feedEditing.ExecuteCommand(new AddToCollection<TAbstractEntry>(getList(container), new TSpecialEntry()));
                    FillFeedTab();
                }
            };

            // Only enable add button if correct parent type is selected
            UpdateStructureButtons += () => addButton.Enabled = SelectedFeedStructureElement is TContainer;
            #endregion

            #region Edit dialog
            treeViewFeedStructure.DoubleClick += delegate
            {
                var entry = SelectedFeedStructureElement as TSpecialEntry;
                var parent = SelectedFeedStructureElementParent as TContainer;
                if (entry != null && parent != null)
                {
                    // Clone entry for undoable modification
                    var clonedEntry = (TSpecialEntry)entry.Clone();

                    using (var dialog = getEditDialog(clonedEntry))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            // Prepare a list of 1 to 3 commands to be executed as a single transaction
                            var commandList = new List<IUndoCommand>(3)
                            {
                                // Replace original entry with cloned and modified one
                                new SetInList<TAbstractEntry>(getList(parent), entry, clonedEntry)
                            };

                            #region Update manifest digest
                            var digestProvider = dialog as IDigestProvider;
                            var implementation = parent as ImplementationBase;
                            if (digestProvider != null && implementation != null)
                            {
                                // ToDo: Warn when changing an existing digest

                                // Set the ManifestDigest entry
                                commandList.Add(new SetValueCommand<ManifestDigest>(
                                    new PropertyPointer<ManifestDigest>(() => implementation.ManifestDigest, newValue => implementation.ManifestDigest = newValue),
                                    digestProvider.ManifestDigest));

                                // Set the implementation ID unless its already something custom
                                if (string.IsNullOrEmpty(implementation.ID) || implementation.ID.StartsWith("sha1=new"))
                                {
                                    commandList.Add(new SetValueCommand<string>(
                                        new PropertyPointer<string>(() => implementation.ID, newValue => implementation.ID = newValue),
                                        "sha1new=" + digestProvider.ManifestDigest.Sha1New));
                                }
                            }
                            #endregion

                            // Execute the transaction
                            _feedEditing.ExecuteCommand(new CompositeCommand(commandList));

                            FillFeedTab();
                        }
                    }
                }
            };
            #endregion

            #region Remove button
            btnRemoveFeedStructureObject.Click += delegate
            {
                var entry = SelectedFeedStructureElement as TSpecialEntry;
                var parent = SelectedFeedStructureElementParent as TContainer;
                if (entry != null && parent != null)
                {
                    _feedEditing.ExecuteCommand(new RemoveFromCollection<TAbstractEntry>(getList(parent), entry));
                    FillFeedTab();
                }
            };
            #endregion
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
                           MsgSeverity.Warn);
                return new GnuPGSecretKey[0];
            }
        }

        /// <summary>
        /// Sets up hooks for keeping the WinForms controls synchronized with the <see cref="Feed"/> data using the command pattern.
        /// </summary>
        private void InitializeCommandHooks()
        {
            SetupCommandHooks(textName, new PropertyPointer<string>(() => _feedEditing.Feed.Name, value => _feedEditing.Feed.Name = value));
            SetupCommandHooks(checkedListBoxCategories, () => _feedEditing.Feed.Categories);
            SetupCommandHooks(textInterfaceUri, new PropertyPointer<Uri>(() => _feedEditing.Feed.Uri, value => _feedEditing.Feed.Uri = value));
            SetupCommandHooks(textHomepage, new PropertyPointer<Uri>(() => _feedEditing.Feed.Homepage, value => _feedEditing.Feed.Homepage = value));
            SetupCommandHooks(summariesControl, () => _feedEditing.Feed.Summaries);
            SetupCommandHooks(descriptionControl, () => _feedEditing.Feed.Descriptions);
            SetupCommandHooks(checkBoxNeedsTerminal, new PropertyPointer<bool>(() => _feedEditing.Feed.NeedsTerminal, value => _feedEditing.Feed.NeedsTerminal = value));
            SetupCommandHooks(iconManagementControl, () => _feedEditing.Feed.Icons);
            SetupCommandHooks(comboBoxMinInjectorVersion, new PropertyPointer<string>(() => _feedEditing.Feed.MinInjectorVersionString, value => _feedEditing.Feed.MinInjectorVersionString = value));
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
        /// <param name="pointer">The object controlling how to read/write the value to be modified.</param>
        private void SetupCommandHooks(UriTextBox textBox, PropertyPointer<Uri> pointer)
        {
            // Transfer data from the feed to the TextBox when refreshing
            Populate += delegate
            {
                textBox.CausesValidation = false;
                textBox.Uri = pointer.Value;
                textBox.CausesValidation = true;
            };

            // Transfer data from the TextBox to the feed via a command object
            textBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (textBox.Uri == pointer.Value) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<Uri>(pointer, textBox.Uri));
            };

            // Enable the undo button even before the command has been created
            textBox.KeyPress += delegate
            {
                try { _feedEditing.UpdateButtonStatus(textBox.Uri != pointer.Value); }
                catch (UriFormatException) {}
            };
            textBox.ClearButtonClicked += delegate
            {
                try { _feedEditing.UpdateButtonStatus(textBox.Uri != pointer.Value); }
                catch (UriFormatException) {}
            };
        }
        
        /// <summary>
        /// Hooks up a <see cref="HintTextBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="pointer">The object controlling how to read/write the value to be modified.</param>
        private void SetupCommandHooks(HintTextBox textBox, PropertyPointer<string> pointer)
        {
            SetupCommandHooks((TextBox)textBox, pointer);

            // Enable the undo button even before the command has been created
            textBox.ClearButtonClicked += delegate { _feedEditing.UpdateButtonStatus(StringUtils.CompareEmptyNull(textBox.Text, pointer.Value)); };
        }

        /// <summary>
        /// Hooks up a <see cref="TextBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="pointer">The object controlling how to read/write the value to be modified.</param>
        private void SetupCommandHooks(TextBox textBox, PropertyPointer<string> pointer)
        {
            // Transfer data from the feed to the TextBox when refreshing
            Populate += delegate
            {
                textBox.CausesValidation = false;
                textBox.Text = pointer.Value;
                textBox.CausesValidation = true;
            };

            // Transfer data from the TextBox to the feed via a command object
            textBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (StringUtils.CompareEmptyNull(textBox.Text, pointer.Value)) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<string>(pointer, textBox.Text));
            };

            // Enable the undo button even before the command has been created
            textBox.KeyPress += delegate { _feedEditing.UpdateButtonStatus(StringUtils.CompareEmptyNull(textBox.Text, pointer.Value)); };
        }

        /// <summary>
        /// Hooks up a <see cref="CheckBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="checkBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="pointer">The object controlling how to read/write the value to be modified.</param>
        private void SetupCommandHooks(CheckBox checkBox, PropertyPointer<bool> pointer)
        {
            // Transfer data from the feed to the CheckBox when refreshing
            Populate += delegate
            {
                checkBox.CausesValidation = false;
                checkBox.Checked = pointer.Value;
                checkBox.CausesValidation = true;
            };

            // Transfer data from the CheckBox to the feed via a command object
            checkBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (checkBox.Checked == pointer.Value) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<bool>(pointer, checkBox.Checked));
            };
        }

        /// <summary>
        /// Hooks up a <see cref="CheckedListBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="checkedListBox">The <see cref="CheckedListBox"/> to track for input and to update.</param>
        /// <param name="getCollection">A delegate that reads the corresponding value from the collection.</param>
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
                    checkedListBox.SetItemChecked(i, getCollection().Contains(checkedListBox.Items[i].ToString()));
                }
                checkedListBox.ItemCheck += itemCheckEventHandler;
            };

            checkedListBox.ItemCheck += itemCheckEventHandler;
        }

        /// <summary>
        /// Hooks up a <see cref="ComboBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="comboBox">The <see cref="ComboBox"/> to track for input and to update.</param>
        /// <param name="pointer">The object controlling how to read/write the value to be modified.</param>
        private void SetupCommandHooks(ComboBox comboBox, PropertyPointer<string> pointer)
        {
            Populate += delegate
            {
                comboBox.CausesValidation = false;
                comboBox.SelectedItem = pointer.Value ?? string.Empty;
                comboBox.CausesValidation = true;
            };

            comboBox.Validating += (sender, e) =>
            {
                if (e.Cancel) return;

                // Normalize unselected or default entry to null
                string selectedValue = (comboBox.SelectedItem ?? "").ToString();
                if (selectedValue == "") selectedValue = null;

                if (selectedValue == pointer.Value) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<string>(pointer, selectedValue));
            };

        }

        private void SetupCommandHooks(LocalizableTextControl localizableTextControl, SimpleResult<LocalizableStringCollection> getCollection)
        {

            localizableTextControl.Values.ItemsAdded += (sender, itemCountEventArgs) => _feedEditing.ExecuteCommand(new AddToCollection<LocalizableString>(getCollection(), itemCountEventArgs.Item));

            localizableTextControl.Values.ItemsRemoved += (sender, itemCountEventArgs) => _feedEditing.ExecuteCommand(new RemoveFromCollection<LocalizableString>(getCollection(), itemCountEventArgs.Item));

            Populate += delegate
            {
                localizableTextControl.Values = getCollection();
            };
        }

        private void SetupCommandHooks(IconManagementControl iconManagementControl, SimpleResult<C5.ArrayList<Icon>> getCollection)
        {
            iconManagementControl.IconUrls.ItemInserted +=(sender, eventArgs) => _feedEditing.ExecuteCommand(new AddToCollection<Icon>(getCollection(), eventArgs.Item));
            
            iconManagementControl.IconUrls.ItemsRemoved += (sender, eventArgs) => _feedEditing.ExecuteCommand(new RemoveFromCollection<Icon>(getCollection(), eventArgs.Item));

            Populate += () => iconManagementControl.IconUrls = getCollection();
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
            SaveAdvancedTab();

            _feedEditing.Save(toPath);
            SignFeed(toPath);
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
                Msg.Choose(this, "Do you want to save the changes you made?", MsgSeverity.Info, true,
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
                SaveFeed(saveFileDialog.FileName);
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

            if (string.IsNullOrEmpty(toolStripComboBoxGpg.Text)) return;
            var key = (GnuPGSecretKey) toolStripComboBoxGpg.SelectedItem;
            do
            {
                string passphrase = InputBox.Show(this,
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

            FillFeedTab();
            FillAdvancedTab();
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

            if (UpdateStructureButtons != null) UpdateStructureButtons();
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
            ResetFeedTabControls();
            ResetAdvancedTabControls();
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

        #region Feed Tab

        #region treeviewFeedStructure methods

        /// <summary>
        /// Enables the buttons which allow the user to add specific new <see cref="TreeNode"/>s in subject to the selected <see cref="TreeNode"/>.
        /// For example: The user selected a "Dependency"-node. Now only the buttons <see cref="btnAddEnvironmentBinding"/> and <see cref="btnAddOverlayBinding"/> will be enabled.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TreeViewFeedStructureAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (UpdateStructureButtons != null) UpdateStructureButtons();
        }

        /// <summary>
        /// Opens a new window to edit the selected entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TreeViewFeedStructureNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode != null && selectedNode != treeViewFeedStructure.Nodes[0]) selectedNode.Toggle();
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
                    BuildCommandsTreeNodes(group.Commands, groupNode);
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
                        BuildCommandsTreeNodes(implementation.Commands, implementationNode);
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
                            BuildCommandsTreeNodes(packageImplementation.Commands, packageImplementationNode);
                            BuildDependencyTreeNodes(packageImplementation.Dependencies, packageImplementationNode);
                            BuildBindingTreeNodes(packageImplementation.Bindings, packageImplementationNode);
                        }
                    }
                }
            }
        }

        private static void BuildCommandsTreeNodes(IEnumerable<Command> commands, TreeNode parentNode)
        {
            #region Sanity checks

            if (commands == null) throw new ArgumentNullException("commands");

            #endregion

            foreach (var command in commands)
            {
                var commandNode = new TreeNode(command.ToString()) {Tag = command};
                parentNode.Nodes.Add(commandNode);
                BuildDependencyTreeNodes(command.Dependencies, commandNode);
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
            if (!Feed.TryParseUri(hintTextBoxFeedFor.Text, out uri)) return;
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