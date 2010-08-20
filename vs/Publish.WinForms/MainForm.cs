/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using C5;
using ZeroInstall.Model;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using ZeroInstall.Publish.WinForms.FeedStructure;

namespace ZeroInstall.Publish.WinForms
{
    public partial class MainForm : Form
    {
        #region Attributes

        /// <summary>
        /// The path of the file the <see cref="Feed"/> was loaded from.
        /// </summary>
        private string _openFeedPath;

        /// <summary>
        /// The <see cref="ZeroInstall.Model.Feed"/> to edit by this form.
        /// </summary>
        private Feed _feedToEdit = new Feed();

        #endregion

        #region Initialization

        /// <summary>
        /// Creats a new <see cref="MainForm"/> object.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            _openFeedPath = null;
            treeViewFeedStructure.Nodes[0].Tag = _feedToEdit;
            InitializeSaveFileDialog();
            InitializeLoadFileDialog();
        }

        /// <summary>
        /// Initializes the <see cref="saveFileDialog"/> with a file filter for .xml files.
        /// </summary>
        private void InitializeSaveFileDialog()
        {
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = "ZeroInstall Feed (*.xml)|*.xml|All Files|*.*";
        }

        /// <summary>
        /// Initializes the <see cref="openFileDialog"/> with a file filter for .xml files.
        /// </summary>
        private void InitializeLoadFileDialog()
        {
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = "ZeroInstall Feed (*.xml)|*.xml |All Files|*.*";
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
            ResetFormControls();
            _feedToEdit = new Feed();
        }

        /// <summary>
        /// Shows a dialog to open a new <see cref="ZeroInstall.Model"/> for editing.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog(this);
        }

        /// <summary>
        /// Shows a dialog to save the edited <see cref="ZeroInstall.Model"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ToolStripButtonSave_Click(object sender, EventArgs e)
        {
            if (_openFeedPath != null)
            {
                saveFileDialog.InitialDirectory = _openFeedPath;
            }
            saveFileDialog.ShowDialog(this);
        }

        #endregion

        #region Save and open

        /// <summary>
        /// Opens the in <see cref="openFileDialog"/> chosen feed file and fills <see cref="MainForm"/> with its values.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _openFeedPath = openFileDialog.FileName;
            _feedToEdit = Feed.Load(_openFeedPath);
            FillForm();
        }


        /// <summary>
        /// Saves the values from the filled controls on the <see cref="MainForm"/> in the feed file chosen by <see cref="openFileDialog"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void SaveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveGeneralTab();
            SaveFeedTab();
            SaveAdvancedTab();     

            _feedToEdit.Save(saveFileDialog.FileName);
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageGeneral"/>.
        /// </summary>
        private void SaveGeneralTab()
        {
            Uri url;

            // save categories
            foreach (var category in checkedListBoxCategories.CheckedItems)
            {
                //TODO complete setting attribute type(required: understanding of the category system)
                _feedToEdit.Categories.Add(category.ToString());
            }
            // save icon urls
            foreach (Model.Icon icon in listBoxIconsUrls.Items)
            {
                _feedToEdit.Icons.Add(icon);
            }
            if (!String.IsNullOrEmpty(hintTextBoxDescription.Text))
            {
                _feedToEdit.Descriptions.Clear();
                _feedToEdit.Descriptions.Add(hintTextBoxDescription.Text);
            }
            if (Uri.TryCreate(hintTextBoxInterfaceUrl.Text, UriKind.Absolute, out url))
            {
                _feedToEdit.Uri = url;
            }
            if (Uri.TryCreate(hintTextBoxHomepage.Text, UriKind.Absolute, out url))
            {
                _feedToEdit.Homepage = url;
            }
            _feedToEdit.NeedsTerminal = checkBoxNeedsTerminal.Checked; 
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageFeed"/>.
        /// </summary>
        private void SaveFeedTab()
        {
            // the feed structure objects will be saved directly into _feedToEdit => no extra saving needed.
            
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageAdvanced"/>.
        /// </summary>
        private void SaveAdvancedTab()
        {
            foreach (var feed in listBoxExternalFeeds.Items)
            {
                _feedToEdit.Feeds.Add((FeedReference)feed);
            }
            foreach (InterfaceReference feedFor in listBoxFeedFor.Items)
            {
                _feedToEdit.FeedFor.Add(feedFor);
            }
            if (!String.IsNullOrEmpty(comboBoxMinInjectorVersion.SelectedText))
            {
                _feedToEdit.MinInjectorVersion = comboBoxMinInjectorVersion.SelectedText;
            }
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
            hintTextBoxProgramName.ResetText();
            hintTextBoxSummary.ResetText();
            hintTextBoxDescription.ResetText();
            hintTextBoxHomepage.ResetText();
            hintTextBoxInterfaceUrl.ResetText();
            hintTextBoxIconUrl.ResetText();
            comboBoxIconType.SelectedIndex = 0;
            pictureBoxIconPreview.Image = null;
            listBoxIconsUrls.Items.Clear();
            lblIconUrlError.ResetText();
            foreach (int category in checkedListBoxCategories.CheckedIndices)
            {
                checkedListBoxCategories.SetItemChecked(category, false);
            }
            checkBoxNeedsTerminal.Checked = false;
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
            comboBoxMinInjectorVersion.SelectedIndex = 0;
        }

        #endregion

        #region Fill form controls

        /// <summary>
        /// Clears all form controls and fills them with the values from a <see cref="_feedToEdit"/>.
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
            hintTextBoxProgramName.Text = _feedToEdit.Name;
            if (!_feedToEdit.Summaries.IsEmpty) hintTextBoxSummary.Text = _feedToEdit.Summaries.First.Value;
            if (!_feedToEdit.Descriptions.IsEmpty) hintTextBoxDescription.Text = _feedToEdit.Descriptions.First.Value;
            hintTextBoxHomepage.Text = _feedToEdit.HomepageString;
            hintTextBoxInterfaceUrl.Text = _feedToEdit.UriString;
            // fill icons list box
            listBoxIconsUrls.BeginUpdate();
            listBoxIconsUrls.Items.Clear();
            foreach (var icon in _feedToEdit.Icons)
            {
                listBoxIconsUrls.Items.Add(icon);
            }
            listBoxIconsUrls.EndUpdate();
            // fill category list
            foreach (var category in _feedToEdit.Categories)
            {
                if (checkedListBoxCategories.Items.Contains(category))
                {
                    checkedListBoxCategories.SetItemChecked(checkedListBoxCategories.Items.IndexOf(category), true);
                }
            }
            checkBoxNeedsTerminal.Checked = _feedToEdit.NeedsTerminal;
        }

        /// <summary>
        /// Fills the <see cref="tabPageFeed"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        private void FillFeedTab()
        {
            treeViewFeedStructure.BeginUpdate();
            treeViewFeedStructure.Nodes[0].Nodes.Clear();
            BuildGroupTreeNodes(_feedToEdit.Groups, treeViewFeedStructure.Nodes[0]);
            BuildImplementationTreeNodes(_feedToEdit.Implementations, treeViewFeedStructure.Nodes[0]);
            BuildPackageImplementationTreeNodes(_feedToEdit.PackageImplementations, treeViewFeedStructure.Nodes[0]);
            treeViewFeedStructure.EndUpdate();

            treeViewFeedStructure.ExpandAll();
        }

        /// <summary>
        /// Fills the <see cref="tabPageAdvanced"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        private void FillAdvancedTab()
        {
            foreach (var feed in _feedToEdit.Feeds)
            {
                listBoxFeedFor.Items.Add(feed);
            }
            foreach (var feedFor in _feedToEdit.FeedFor)
            {
                listBoxFeedFor.Items.Add(feedFor);
            }
            comboBoxMinInjectorVersion.Text = _feedToEdit.MinInjectorVersion;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "png")]
        private void BtnIconPreviewClick(object sender, EventArgs e)
        {
            Uri iconUrl;
            Image icon;
            lblIconUrlError.ForeColor = Color.Red;
            // check url
            if(!ControlHelpers.IsValidFeedUrl(hintTextBoxIconUrl.Text, out iconUrl)) return;

            // try downloading image
            try
            {
                icon = GetImageFromUrl(iconUrl);
            }
            catch (WebException ex)
            {
                switch (ex.Status)
                {
                    case WebExceptionStatus.ConnectFailure:
                        lblIconUrlError.Text = "File not found";
                        break;
                    default:
                        lblIconUrlError.Text = "Couldn't download image";
                        break;
                }
                return;
            }
            catch (ArgumentException)
            {
                lblIconUrlError.Text = "URL does not describe an image";
                return;
            }

            // check what format is the icon and set right value into comboIconType
            if (icon.RawFormat.Equals(ImageFormat.Png))
            {
                comboBoxIconType.SelectedIndex = 0;
            }
            else if (icon.RawFormat.Equals(ImageFormat.Icon))
            {
                comboBoxIconType.SelectedIndex = 1;
            }
            else
            {
                lblIconUrlError.Text = "Image format must be png or ico";
                return;
            }

            pictureBoxIconPreview.Image = icon;
        }

        /// <summary>
        /// Adds the url from <see cref="hintTextBoxIconUrl"/> and the chosen mime type in <see cref="comboBoxIconType"/> to <see cref="listBoxIconsUrls"/> if the url is a valid url.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnIconListAdd_Click(object sender, EventArgs e)
        {
            var icon = new Model.Icon();
            Uri uri;
            if (!ControlHelpers.IsValidFeedUrl(hintTextBoxIconUrl.Text, out uri)) return;

            icon.Location = uri;
            // set mime type
            switch (comboBoxIconType.Text)
            {
                case "PNG":
                    icon.MimeType = "image/png";
                    break;
                case "ICO":
                    icon.MimeType = "image/vnd-microsoft-icon";
                    break;
                default: break;
            }

            // add icon object to list box
            if (!listBoxIconsUrls.Items.Contains(icon)) listBoxIconsUrls.Items.Add(icon);
        }

        /// <summary>
        /// Removes chosen image url in <see cref="listBoxIconsUrls"/> from the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnIconListRemove_Click(object sender, EventArgs e)
        {
            var icon = listBoxIconsUrls.SelectedItem;
            if (listBoxIconsUrls.SelectedItem == null) return;
            listBoxIconsUrls.Items.Remove(icon);
        }

        /// <summary>
        /// Replaces the text and the icon type from <see cref="hintTextBoxIconUrl"/> and <see cref="comboBoxIconType"/> with the text and the icon type of the chosen entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void listIconsUrls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxIconsUrls.SelectedItem == null) return;

            var icon = (Model.Icon)listBoxIconsUrls.SelectedItem;
            hintTextBoxIconUrl.Text = icon.LocationString;
            switch (icon.MimeType)
            {
                case null:
                    comboBoxIconType.Text = String.Empty;
                    break;
                case "image/png":
                    comboBoxIconType.Text = "PNG";
                    break;
                case "image/vnd-microsoft-icon":
                    comboBoxIconType.Text = "ICO";
                    break;
                default:
                    throw new InvalidOperationException("Invalid MIME-Type");
            }
        }

        /// <summary>
        /// Checks if the text in <see cref="hintTextBoxIconUrl"/> is a valid url and enables the <see cref="buttonIconPreview"/> if true and disables it if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void textIconUrl_TextChanged(object sender, EventArgs e)
        {
            if (ControlHelpers.IsValidFeedUrl(hintTextBoxIconUrl.Text))
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

        /// <summary>
        /// Checks if the text of <see cref="hintTextBoxInterfaceUrl"/> is a valid feed url and sets the the text to <see cref="Color.Green"/> if true or to <see cref="Color.Red"/> if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void textInterfaceURL_TextChanged(object sender, EventArgs e)
        {
            if(ControlHelpers.IsValidFeedUrl(hintTextBoxInterfaceUrl.Text))
            {
                hintTextBoxInterfaceUrl.ForeColor = Color.Green;

            } else
            {
                hintTextBoxInterfaceUrl.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Checks if the text of <see cref="hintTextBoxHomepage"/> is a valid feed url and sets the the text to <see cref="Color.Green"/> if true or to <see cref="Color.Red"/> if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void textHomepage_TextChanged(object sender, EventArgs e)
        {
            hintTextBoxHomepage.ForeColor = ControlHelpers.IsValidFeedUrl(hintTextBoxHomepage.Text) ? Color.Green : Color.Red;
        }

        #endregion

        #region Feed Tab

        #region add buttons clicked methods

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Group" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnAddGroupClick(object sender, EventArgs e)
        {
            AddFeedStructureObject(new Group());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Implementation" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnAddImplementationClick(object sender, EventArgs e)
        {
            AddFeedStructureObject(new Implementation());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Package Implementation" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnAddPackageImplementationClick(object sender, EventArgs e)
        {
            AddFeedStructureObject(new PackageImplementation());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Dependency" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnAddDependencyClick(object sender, EventArgs e)
        {
            AddFeedStructureObject(new Dependency());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Environment Binding" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnAddEnvironmentBindingClick(object sender, EventArgs e)
        {
            AddFeedStructureObject(new EnvironmentBinding());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Overlay Binding" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnAddOverlayBindingClick(object sender, EventArgs e)
        {
            AddFeedStructureObject(new OverlayBinding());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Archive" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonAddArchiveClick(object sender, EventArgs e)
        {
            AddFeedStructureObject(new Archive());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Recipe" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonAddRecipeClick(object sender, EventArgs e)
        {
            AddFeedStructureObject(new Recipe());
        }

        /// <summary>
        /// Gets the selected <see cref="TreeNode"/> from <see cref="treeViewFeedStructure"/> and adds (if allowed) <paramref name="feedStructureObject"/> to the container stored
        /// in the Tag of the selected <see cref="TreeNode"/>. Then <see cref="treeViewFeedStructure"/> will be rebuild from <see cref="_feedToEdit"/>.
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
                var feed = (Feed)selectedNode.Tag;

                if(feedStructureObject is Group)
                {
                    feed.Groups.Add((Group)feedStructureObject);
                } else if (feedStructureObject is Implementation)
                {
                    feed.Implementations.Add((Implementation) feedStructureObject);
                }
                else if (feedStructureObject is PackageImplementation)
                {
                    feed.PackageImplementations.Add((PackageImplementation) feedStructureObject);
                }
            }
            else if (selectedNode.Tag is Group)
            {
                var group = (Group) selectedNode.Tag;

                if(feedStructureObject is Dependency)
                {
                    group.Dependencies.Add((Dependency)feedStructureObject);
                }
                else if(feedStructureObject is EnvironmentBinding)
                {
                    group.EnvironmentBindings.Add((EnvironmentBinding)feedStructureObject);
                }
                else if(feedStructureObject is Group)
                {
                    group.Groups.Add((Group)feedStructureObject);
                    
                } else if(feedStructureObject is Implementation)
                {
                    group.Implementations.Add((Implementation)feedStructureObject);
                } else if (feedStructureObject is OverlayBinding)
                {
                    group.OverlayBindings.Add((OverlayBinding)feedStructureObject);
                }
                else if(feedStructureObject is PackageImplementation)
                {
                    group.PackageImplementations.Add((PackageImplementation)feedStructureObject);
                }
            }
            else if (selectedNode.Tag is Implementation)
            {
                var implementation = (Implementation) selectedNode.Tag;

                if(feedStructureObject is Archive)
                {
                    implementation.Archives.Add((Archive) feedStructureObject);
                }
                else if (feedStructureObject is Dependency)
                {
                    implementation.Dependencies.Add((Dependency) feedStructureObject);
                } else if(feedStructureObject is EnvironmentBinding)
                {
                    implementation.EnvironmentBindings.Add((EnvironmentBinding) feedStructureObject);
                } else if(feedStructureObject is OverlayBinding)
                {
                    implementation.OverlayBindings.Add((OverlayBinding)feedStructureObject);
                } else if(feedStructureObject is Recipe)
                {
                    implementation.Recipes.Add((Recipe) feedStructureObject);
                }
            }
            else if (selectedNode.Tag is PackageImplementation)
            {
                var packageImplementation = (PackageImplementation) selectedNode.Tag;
                if(feedStructureObject is Dependency)
                {
                    packageImplementation.Dependencies.Add((Dependency) feedStructureObject);
                } else if(feedStructureObject is EnvironmentBinding)
                {
                    packageImplementation.EnvironmentBindings.Add((EnvironmentBinding) feedStructureObject);
                }
                else if (feedStructureObject is OverlayBinding)
                {
                    packageImplementation.OverlayBindings.Add((OverlayBinding) feedStructureObject);
                }
            }
            else if (selectedNode.Tag is Dependency)
            {
                var dependecy = (Dependency)selectedNode.Tag;
                if (feedStructureObject is EnvironmentBinding)
                {
                    dependecy.EnvironmentBindings.Add((EnvironmentBinding)feedStructureObject);
                }
                else if (feedStructureObject is OverlayBinding)
                {
                    dependecy.OverlayBindings.Add((OverlayBinding)feedStructureObject);
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
            var selectedNode = treeViewFeedStructure.SelectedNode;

            Button[] addButtons = { btnAddGroup, btnAddImplementation, buttonAddArchive, buttonAddRecipe,
                                      btnAddPackageImplementation, btnAddDependency, btnAddEnvironmentBinding, btnAddOverlayBinding };
            Button[] enableAddButtons = null;
            // disable all addButtons
            foreach (var button in addButtons)
            {
                button.Enabled = false;
            }
            
            // mark the addButtons that can be selected
            if (selectedNode == treeViewFeedStructure.Nodes[0])
            {
                enableAddButtons = new Button[] { btnAddGroup, btnAddImplementation };
            }
            else if (selectedNode.Text.StartsWith("Group"))
            {
                enableAddButtons = new Button[] { btnAddGroup, btnAddImplementation, btnAddPackageImplementation, btnAddDependency, btnAddEnvironmentBinding, btnAddOverlayBinding };
            }
            else if (selectedNode.Text.StartsWith("Implementation"))
            {
                enableAddButtons = new Button[] { buttonAddArchive, buttonAddRecipe, btnAddDependency, btnAddEnvironmentBinding, btnAddOverlayBinding };
            }
            else if (selectedNode.Text.StartsWith("Dependency"))
            {
                enableAddButtons = new Button[] { btnAddEnvironmentBinding, btnAddOverlayBinding };
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
            if (selectedNode.Tag is Group) (new GroupForm { Group = (Group) selectedNode.Tag}).ShowDialog();
            else if (selectedNode.Tag is Implementation) (new ImplementationForm { Implementation = (Implementation)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is Archive)
            {
                var feedStructurForm = new ArchiveForm
                                           {
                                               Archive = (Archive) selectedNode.Tag,
                                               Readonly = !IsEmpty((Archive) selectedNode.Tag)
                                           };

                if (feedStructurForm.ShowDialog() != DialogResult.OK) return;

                if(selectedNode.Parent.FirstNode.Tag is ManifestDigest)
                {
                    if(!CompareManifestDigests((ManifestDigest)selectedNode.Parent.FirstNode.Tag, feedStructurForm.ManifestDigest))
                    {
                        MessageBox.Show("The manifest digest of this archive is not the same as the manifest digest of the other archives. The archive was disacared,");
                        selectedNode.Tag = new Archive();
                        return;
                    }
                } else
                {
                    var manifestDigestNode = new TreeNode("Manifest digest") {Tag = feedStructurForm.ManifestDigest};
                    selectedNode.Parent.Nodes.Insert(0, manifestDigestNode);
                }
            }
            else if (selectedNode.Tag is Recipe) throw new NotImplementedException();
            else if (selectedNode.Tag is PackageImplementation) (new PackageImplementationForm { PackageImplementation = (PackageImplementation)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is Dependency) (new DependencyForm { Dependency = (Dependency)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is EnvironmentBinding) (new EnvironmentBindingForm { EnvironmentBinding = (EnvironmentBinding)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is OverlayBinding) (new OverlayBindingForm { OverlayBinding = (OverlayBinding)selectedNode.Tag }).ShowDialog();
            else if(selectedNode.Tag is ManifestDigest) (new ManifestDigestForm((ManifestDigest) selectedNode.Tag)).ShowDialog();
            else throw new InvalidOperationException("Not an object to change.");
        }

        private static bool IsEmpty(Archive toCheck)
        {
            return (toCheck.Extract == default(String) && toCheck.Location == default(Uri) && toCheck.MimeType == default(String) && toCheck.Size == default(long) && toCheck.StartOffset == default(long));
        }

        private static bool CompareManifestDigests(ManifestDigest manifestDigest1, ManifestDigest manifestDigest2)
        {
            if (manifestDigest1.Sha256 != String.Empty && manifestDigest2.Sha256 != String.Empty)
            {
                if (manifestDigest1.Sha256 != manifestDigest2.Sha256) return false;
            }
            else if (manifestDigest1.Sha1New != String.Empty && manifestDigest2.Sha1New != String.Empty)
            {
                if (manifestDigest1.Sha1New != manifestDigest2.Sha1New) return false;
            }
            else if (manifestDigest1.Sha1Old != String.Empty && manifestDigest2.Sha1Old != String.Empty)
            {
                if (manifestDigest1.Sha1Old != manifestDigest2.Sha1Old) return false;
            }
            return true;
        }

        /// <summary>
        /// Removes the selected <see cref="TreeNode"/> and all of its subnodes.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnRemoveFeedStructureObjectClick(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.Nodes[0]) return;
            RemoveObjectFromFeedStructure(selectedNode.Parent.Tag, selectedNode.Tag);
            FillFeedTab();
        }

        private void RemoveObjectFromFeedStructure(object container, object toRemove)
        {
            if (toRemove is Group) ((IGroupContainer)container).Groups.Remove((Group)toRemove);
            else if (toRemove is Implementation) ((IGroupContainer)container).Implementations.Remove((Implementation)toRemove);
            else if (toRemove is PackageImplementation) ((IGroupContainer)container).PackageImplementations.Remove((PackageImplementation)toRemove);
            else if (toRemove is Dependency) ((ImplementationBase) container).Dependencies.Remove((Dependency) toRemove);
            else if (toRemove is EnvironmentBinding) ((IBindingContainer)container).EnvironmentBindings.Remove((EnvironmentBinding)toRemove);
            else if (toRemove is OverlayBinding) ((IBindingContainer)container).OverlayBindings.Remove((OverlayBinding)toRemove);
            else if (toRemove is Archive) ((Implementation)container).Archives.Remove((Archive)toRemove);
            else if (toRemove is Recipe) ((Implementation)container).Recipes.Remove((Recipe)toRemove);
        }

        /// <summary>
        /// Clears <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonClearListClick(object sender, EventArgs e)
        {
            _feedToEdit.Groups.Clear();
            _feedToEdit.Implementations.Clear();
            _feedToEdit.PackageImplementations.Clear();
            FillFeedTab();
        }

        private void BuildGroupTreeNodes(ICollectionValue<Group> groups, TreeNode parentNode)
        {
            if (groups == null) throw new ArgumentNullException("groups");

            if (groups.IsEmpty) return;
            foreach (var group in groups)
            {
                var groupNode = new TreeNode("Group") {Tag = group};
                parentNode.Nodes.Add(groupNode);
                BuildGroupTreeNodes(group.Groups, groupNode);
                BuildImplementationTreeNodes(group.Implementations, groupNode);
                BuildPackageImplementationTreeNodes(group.PackageImplementations, groupNode);
                BuildDependencyTreeNodes(group.Dependencies, groupNode);
                BuildEnvironmentBindingTreeNodes(group.EnvironmentBindings, groupNode);
                BuildOverlayBindingTreeNodes(group.OverlayBindings, groupNode);
            }
        }

        private void BuildPackageImplementationTreeNodes(ICollectionValue<PackageImplementation> packageImplementations, TreeNode parentNode)
        {
            if (packageImplementations == null) throw new ArgumentNullException("packageImplementations");
            
            if (packageImplementations.IsEmpty) return;
            foreach (var packageImplementation in packageImplementations)
            {
                var packageImplementationNode = new TreeNode("Package implementation") {Tag = packageImplementation};
                parentNode.Nodes.Add(packageImplementationNode);
                BuildDependencyTreeNodes(packageImplementation.Dependencies, parentNode);
                BuildEnvironmentBindingTreeNodes(packageImplementation.EnvironmentBindings, parentNode);
                BuildOverlayBindingTreeNodes(packageImplementation.OverlayBindings, parentNode);
            }
        }

        private void BuildOverlayBindingTreeNodes(ICollectionValue<OverlayBinding> overlayBindings, TreeNode parentNode)
        {
            if (overlayBindings == null) throw new ArgumentNullException("overlayBindings");

            if (overlayBindings.IsEmpty) return;
            foreach (var overlayBinding in overlayBindings)
            {
                var overlayBindingNode = new TreeNode("Overlay binding") {Tag = overlayBinding};
                parentNode.Nodes.Add(overlayBindingNode);
            }
        }

        private void BuildImplementationTreeNodes(ICollectionValue<Implementation> implementations, TreeNode parentNode)
        {
            if (implementations == null) throw new ArgumentNullException("implementations");

            if (implementations.IsEmpty) return;
            foreach (var implementation in implementations)
            {
                var implementationNode = new TreeNode("Implementation") {Tag = implementation};
                parentNode.Nodes.Add(implementationNode);
                BuildManifestDigestTreeNode(implementation.ManifestDigest, implementationNode);
                BuildArchivTreeNodes(implementation.Archives, implementationNode);
                BuildRecipeTreeNodes(implementation.Recipes, implementationNode);
                BuildDependencyTreeNodes(implementation.Dependencies, implementationNode);
                BuildEnvironmentBindingTreeNodes(implementation.EnvironmentBindings, implementationNode);
                BuildOverlayBindingTreeNodes(implementation.OverlayBindings, implementationNode);
            }
        }

        private void BuildManifestDigestTreeNode(ManifestDigest manifestDigest, TreeNode parentNode)
        {
            var manifestDigestNode = new TreeNode("Manifest digest") {Tag = manifestDigest};
            parentNode.Nodes.Insert(0, manifestDigestNode);
        }

        private void BuildRecipeTreeNodes(ICollectionValue<Recipe> recipes, TreeNode parentNode)
        {
            if (recipes == null) throw new ArgumentNullException("recipes");

            if (recipes.IsEmpty) return;
            foreach (var recipe in recipes)
            {
                var recipeNode = new TreeNode("Recipe") {Tag = recipe};
                parentNode.Nodes.Add(recipeNode);
            }
        }

        private void BuildArchivTreeNodes(ICollectionValue<Archive> archives, TreeNode parentNode)
        {
            if (archives == null) throw new ArgumentNullException("archives");
            
            if (archives.IsEmpty) return;
            foreach (var archive in archives)
            {
                var archiveNode = new TreeNode("Archive") {Tag = archive};
                parentNode.Nodes.Add(archiveNode);
            }
        }

        private void BuildEnvironmentBindingTreeNodes(ICollectionValue<EnvironmentBinding> environmentBindings, TreeNode parentNode)
        {
            if (environmentBindings == null) throw new ArgumentNullException("environmentBindings");

            if (environmentBindings.IsEmpty) return;
            foreach (var environmentBinding in environmentBindings)
            {
                var environmentBindingNode = new TreeNode("Environment binding") {Tag = environmentBinding};
                parentNode.Nodes.Add(environmentBindingNode);
            }
        }

        private void BuildDependencyTreeNodes(ICollectionValue<Dependency> dependencies, TreeNode parentNode)
        {
            if (dependencies == null) throw new ArgumentNullException("dependencies");

            if (dependencies.IsEmpty) return;
            foreach (var dependency in dependencies)
            {
                var dependencyNode = new TreeNode("Dependency") {Tag = dependency};
                parentNode.Nodes.Add(dependencyNode);
            }
        }

        #endregion

        #endregion

        #region Advanced Tab

        #region External Feeds Group

        /// <summary>
        /// Adds a clone of the <see cref="feedReferenceControl"/> to <see cref="listBoxExternalFeeds"/> if no equal object is in the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnExtFeedsAdd_Click(object sender, EventArgs e)
        {
            var feedReference = feedReferenceControl.FeedReference.CloneFeedReference();
            if (string.IsNullOrEmpty(feedReference.Source)) return;
            if (!listBoxExternalFeeds.Items.Contains(feedReference))
            {
                listBoxExternalFeeds.Items.Add(feedReference);
            }
        }

        /// <summary>
        /// Removes the selected <see cref="FeedReference"/> from <see cref="listBoxExternalFeeds"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnExtFeedsRemove_Click(object sender, EventArgs e)
        {
            var selectedItem = listBoxExternalFeeds.SelectedItem;
            if (selectedItem == null) return;
            listBoxExternalFeeds.Items.Remove(selectedItem);
        }

        /// <summary>
        /// Loads a clone of the selected <see cref="FeedReference"/> into <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void listBoxExtFeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (FeedReference)listBoxExternalFeeds.SelectedItem;
            if (selectedItem == null) return;
            feedReferenceControl.FeedReference = selectedItem.CloneFeedReference();
        }

        /// <summary>
        /// Updates the selected <see cref="FeedReference"/> in <see cref="listBoxExternalFeeds"/> with the new values from <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnExtFeedUpdate_Click(object sender, EventArgs e)
        {
            var selectedFeedReferenceIndex = listBoxExternalFeeds.SelectedIndex;
            var feedReference = feedReferenceControl.FeedReference;
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
        private void btnFeedForAdd_Click(object sender, EventArgs e)
        {
            Uri uri;
            if (!ControlHelpers.IsValidFeedUrl(hintTextBoxFeedFor.Text, out uri)) return;
            var interfaceReference = new InterfaceReference();
            interfaceReference.Target = uri;
            listBoxFeedFor.Items.Add(interfaceReference);
        }

        /// <summary>
        /// Removes the selected entry from <see cref="listBoxFeedFor"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnFeedForRemove_Click(object sender, EventArgs e)
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
        private void btnFeedForClear_Click(object sender, EventArgs e)
        {
            listBoxFeedFor.Items.Clear();
        }

        #endregion

        #endregion

        #endregion
    }
}
