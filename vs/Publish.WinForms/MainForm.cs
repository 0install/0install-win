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
using ZeroInstall.Model;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using ZeroInstall.Publish.WinForms.FeedStructure;

namespace ZeroInstall.Publish.WinForms
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// The path of the file the <see cref="Feed"/> was loaded from.
        /// </summary>
        private string _openInterfacePath;

        #region Initialization

        /// <summary>
        /// Creats a new <see cref="MainForm"/> object.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            _openInterfacePath = null;
            InitializeSaveFileDialog();
            InitializeLoadFileDialog();
        }

        /// <summary>
        /// Initializes the <see cref="saveFileDialog"/> with a file filter for .xml files.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ZeroInstall")]
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
            ResetForm();
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
            saveFileDialog.InitialDirectory = _openInterfacePath;
            saveFileDialog.ShowDialog(this);
        }

        #endregion

        #region Saving and opening dialogs

        /// <summary>
        /// Opens the in <see cref="openFileDialog"/> chosen feed file and fills <see cref="MainForm"/> with its values.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _openInterfacePath = openFileDialog.FileName;
            var feed = Feed.Load(_openInterfacePath);
            FillForm(feed);
        }

        /// <summary>
        /// Saves the values from the filled controls on the <see cref="MainForm"/> in the feed file chosen by <see cref="openFileDialog"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void SaveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var zeroInterface = new Feed { Name = textName.Text, Summary = textSummary.Text };

            SaveGeneralTab(zeroInterface);
            SaveFeedTab(zeroInterface);
            SaveAdvancedTab(zeroInterface);     

            // Save to file
            zeroInterface.Save(saveFileDialog.FileName);
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageGeneral"/>.
        /// </summary>
        /// <param name="zeroInterface"><see cref="Feed"/> where to save values.</param>
        private void SaveGeneralTab(Feed zeroInterface)
        {
            Uri url;

            // save categories
            foreach (var category in checkedListCategory.CheckedItems)
            {
                //TODO complete setting attribute type(required: understanding of the category system)
                zeroInterface.Categories.Add(category.ToString());
            }
            // save icon urls
            foreach (Model.Icon icon in listIconsUrls.Items)
            {
                zeroInterface.Icons.Add(icon);
            }
            if (!String.IsNullOrEmpty(textDescription.Text))
            {
                zeroInterface.Description = textDescription.Text;
            }
            if (Uri.TryCreate(textInterfaceURL.Text, UriKind.Absolute, out url))
            {
                zeroInterface.Uri = url;
            }
            if (Uri.TryCreate(textHomepage.Text, UriKind.Absolute, out url))
            {
                zeroInterface.Homepage = url;
            }
            zeroInterface.NeedsTerminal = checkBoxNeedsTerminal.Checked; 
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageFeed"/>.
        /// </summary>
        /// <param name="zeroInterface"><see cref="Feed"/> where to save values.</param>
        private void SaveFeedTab(Feed zeroInterface)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageAdvanced"/>.
        /// </summary>
        /// <param name="zeroInterface"><see cref="Feed"/> where to save values.</param>
        private void SaveAdvancedTab(Feed zeroInterface)
        {
            foreach (var feed in listBoxExtFeeds.Items)
            {
                zeroInterface.Feeds.Add((FeedReference)feed);
            }
            foreach (InterfaceReference feedFor in listBoxFeedFor.Items)
            {
                zeroInterface.FeedFor.Add(feedFor);
            }
            if (!String.IsNullOrEmpty(comboBoxMinInjectorVersion.SelectedText))
            {
                zeroInterface.MinInjectorVersion = comboBoxMinInjectorVersion.SelectedText;
            }
        }

        /// <summary>
        /// Sets all controls on the <see cref="MainForm"/> to default values.
        /// </summary>
        private void ResetForm()
        {
            ResetGeneralTab();
            ResetFeedTab();
            ResetAdvancedTab();
        }

        /// <summary>
        /// Sets all controls on <see cref="tabPageGeneral"/> to default values.
        /// </summary>
        private void ResetGeneralTab()
        {
            textName.ResetText();
            textSummary.ResetText();
            textDescription.ResetText();
            textHomepage.ResetText();
            textInterfaceURL.ResetText();
            textIconUrl.ResetText();
            listIconsUrls.Items.Clear();
            foreach (int categoryIndex in checkedListCategory.CheckedIndices)
            {
                checkedListCategory.SetItemChecked(categoryIndex, false);
            }
            checkBoxNeedsTerminal.Checked = false;
        }

        /// <summary>
        /// Sets all controls on <see cref="tabPageFeed"/> to default values.
        /// </summary>
        private void ResetFeedTab()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets all controls on <see cref="tabPageAdvanced"/> to default values.
        /// </summary>
        private void ResetAdvancedTab()
        {
            listBoxExtFeeds.Items.Clear();
            textFeedFor.Clear();
            listBoxFeedFor.Items.Clear();
            feedReferenceControl.FeedReference = null;
            comboBoxMinInjectorVersion.SelectedIndex = 0;
        }

        /// <summary>
        /// Fills the <see cref="MainForm"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        /// <param name="zeroInterface">The <see cref="ZeroInstall.Model.Feed"/> to use for fill the <see cref="MainForm"/>.</param>
        private void FillForm(Feed zeroInterface)
        {
            ResetForm();
            FillGeneralTab(zeroInterface);
            FillFeedTab(zeroInterface);
            FillAdvancedTab(zeroInterface);
        }

        /// <summary>
        /// Fills the <see cref="tabPageGeneral"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        /// <param name="zeroInterface">The <see cref="ZeroInstall.Model.Feed"/> to use for fill the <see cref="tabPageGeneral"/>.</param>
        private void FillGeneralTab(Feed zeroInterface)
        {
            textName.Text = zeroInterface.Name;
            textSummary.Text = zeroInterface.Summary;
            textDescription.Text = zeroInterface.Description;
            textHomepage.Text = zeroInterface.HomepageString;
            textInterfaceURL.Text = zeroInterface.UriString;
            // fill icons list box
            listIconsUrls.BeginUpdate();
            listIconsUrls.Items.Clear();
            foreach (var icon in zeroInterface.Icons)
            {
                listIconsUrls.Items.Add(icon);
            }
            listIconsUrls.EndUpdate();
            // fill category list
            foreach (var category in zeroInterface.Categories)
            {
                if (checkedListCategory.Items.Contains(category))
                {
                    checkedListCategory.SetItemChecked(checkedListCategory.Items.IndexOf(category), true);
                }
            }
            checkBoxNeedsTerminal.Checked = zeroInterface.NeedsTerminal;
        }

        /// <summary>
        /// Fills the <see cref="tabPageFeed"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        /// <param name="zeroInterface">The <see cref="ZeroInstall.Model.Feed"/> to use for fill the <see cref="tabPageFeed"/>.</param>
        private void FillFeedTab(Feed zeroInterface)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fills the <see cref="tabPageAdvanced"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        /// <param name="zeroInterface">The <see cref="ZeroInstall.Model.Feed"/> to use for fill the <see cref="tabPageAdvanced"/>.</param>
        private void FillAdvancedTab(Feed zeroInterface)
        {
            foreach (var feed in zeroInterface.Feeds)
            {
                listBoxFeedFor.Items.Add(feed);
            }
            foreach (var feedFor in zeroInterface.FeedFor)
            {
                listBoxFeedFor.Items.Add(feedFor);
            }
            comboBoxMinInjectorVersion.Text = zeroInterface.MinInjectorVersion;
        }      

        #endregion

        #region General Tab

        #region Icon Group

        /// <summary>
        /// Tries to download the image with the url from <see cref="textIconUrl"/> and shows it in <see cref="iconBox"/>.
        /// Sets the right <see cref="ImageFormat"/> from the downloaded image in <see cref="comboIconType"/>.
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
            if(!ControlHelpers.IsValidFeedUrl(textIconUrl.Text, out iconUrl)) return;

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
                comboIconType.SelectedIndex = 0;
            }
            else if (icon.RawFormat.Equals(ImageFormat.Icon))
            {
                comboIconType.SelectedIndex = 1;
            }
            else
            {
                lblIconUrlError.Text = "Image format must be png or ico";
                return;
            }

            iconBox.Image = icon;
        }

        /// <summary>
        /// Adds the url from <see cref="textIconUrl"/> and the chosen mime type in <see cref="comboIconType"/> to <see cref="listIconsUrls"/> if the url is a valid url.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnIconListAdd_Click(object sender, EventArgs e)
        {
            var icon = new Model.Icon();
            Uri uri;
            if (!ControlHelpers.IsValidFeedUrl(textIconUrl.Text, out uri)) return;

            icon.Location = uri;
            // set mime type
            switch (comboIconType.Text)
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
            if (!listIconsUrls.Items.Contains(icon)) listIconsUrls.Items.Add(icon);
        }

        /// <summary>
        /// Removes chosen image url in <see cref="listIconsUrls"/> from the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnIconListRemove_Click(object sender, EventArgs e)
        {
            var icon = listIconsUrls.SelectedItem;
            if (listIconsUrls.SelectedItem == null) return;
            listIconsUrls.Items.Remove(icon);
        }

        /// <summary>
        /// Replaces the text and the icon type from <see cref="textIconUrl"/> and <see cref="comboIconType"/> with the text and the icon type of the chosen entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void listIconsUrls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listIconsUrls.SelectedItem == null) return;

            var icon = (Model.Icon)listIconsUrls.SelectedItem;
            textIconUrl.Text = icon.LocationString;
            switch (icon.MimeType)
            {
                case null:
                    comboIconType.Text = String.Empty;
                    break;
                case "image/png":
                    comboIconType.Text = "PNG";
                    break;
                case "image/vnd-microsoft-icon":
                    comboIconType.Text = "ICO";
                    break;
                default:
                    throw new InvalidOperationException("Invalid MIME-Type");
            }
        }

        /// <summary>
        /// Checks if the text in <see cref="textIconUrl"/> is a valid url and enables the <see cref="btnIconPreview"/> if true and disables it if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void textIconUrl_TextChanged(object sender, EventArgs e)
        {
            if (ControlHelpers.IsValidFeedUrl(textIconUrl.Text))
            {
                textIconUrl.ForeColor = Color.Green;
                btnIconPreview.Enabled = true;
            }
            else
            {
                textIconUrl.ForeColor = Color.Red;
                btnIconPreview.Enabled = false;
            }
        }

        #endregion

        /// <summary>
        /// Checks if the text of <see cref="textInterfaceURL"/> is a valid feed url and sets the the text to <see cref="Color.Green"/> if true or to <see cref="Color.Red"/> if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void textInterfaceURL_TextChanged(object sender, EventArgs e)
        {
            textInterfaceURL.ForeColor = ControlHelpers.IsValidFeedUrl(textInterfaceURL.Text) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Checks if the text of <see cref="textHomepage"/> is a valid feed url and sets the the text to <see cref="Color.Green"/> if true or to <see cref="Color.Red"/> if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void textHomepage_TextChanged(object sender, EventArgs e)
        {
            textHomepage.ForeColor = ControlHelpers.IsValidFeedUrl(textHomepage.Text) ? Color.Green : Color.Red;
        }

        #endregion

        #region Feed Tab

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text of <paramref name="text"/> to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tag"></param>
        private void addTreeNode(string text, object tag)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode ?? treeViewFeedStructure.TopNode;
            var treeNode = new TreeNode(text) {Tag = tag};
            selectedNode.Nodes.Add(treeNode);
            treeViewFeedStructure.SelectedNode = treeNode;
            selectedNode.Expand();
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Group" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnAddGroup_Click(object sender, EventArgs e)
        {
            addTreeNode("Group", new Group());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Implementation" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnAddImplementation_Click(object sender, EventArgs e)
        {
            addTreeNode("Implementation", new Implementation());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Package Implementation" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnAddPackageImplementation_Click(object sender, EventArgs e)
        {
            addTreeNode("Package Implementation", new PackageImplementation());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Dependency" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnAddDependency_Click(object sender, EventArgs e)
        {
            addTreeNode("Dependency", new Dependency());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Environment Binding" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnAddEnvironmentBinding_Click(object sender, EventArgs e)
        {
            addTreeNode("Environment Binding", new EnvironmentBinding());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Overlay Binding" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnAddOverlayBinding_Click(object sender, EventArgs e)
        {
            addTreeNode("Overlay Binding", new OverlayBinding());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Archive" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonAddArchive_Click(object sender, EventArgs e)
        {
            addTreeNode("Archive", new Archive());
        }

        /// <summary>
        /// Adds a new <see cref="TreeNode"/> with text "Recipe" to the selected <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonAddRecipe_Click(object sender, EventArgs e)
        {
            addTreeNode("Recipe", new Recipe());
        }

        /// <summary>
        /// Enables the buttons which allow the user to add specific new <see cref="TreeNode"/>s in subject to the selected <see cref="TreeNode"/>.
        /// For example: The user selected a "Dependency"-node. Now only the buttons <see cref="btnAddEnvironmentBinding"/> and <see cref="btnAddOverlayBinding"/> will be enabled.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void treeViewFeedStructure_AfterSelect(object sender, TreeViewEventArgs e)
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
            if (selectedNode == treeViewFeedStructure.TopNode)
            {
                enableAddButtons = new Button[] { btnAddGroup, btnAddImplementation };
            }
            else if (selectedNode.Tag is Group)
            {
                enableAddButtons = new Button[] { btnAddGroup, btnAddImplementation, btnAddPackageImplementation, btnAddDependency, btnAddEnvironmentBinding, btnAddOverlayBinding };
            }
            else if (selectedNode.Tag is Implementation)
            {
                enableAddButtons = new Button[] { buttonAddArchive, buttonAddRecipe, btnAddDependency, btnAddEnvironmentBinding, btnAddOverlayBinding };
            }
            else if (selectedNode.Tag is Dependency)
            {
                enableAddButtons = new Button[] { btnAddEnvironmentBinding, btnAddOverlayBinding };
            }

            // enable marked buttons
            if (enableAddButtons == null) return;
            for (int i = 0; i < enableAddButtons.Length; i++)
                enableAddButtons[i].Enabled = true;
        }

        /// <summary>
        /// Removes the selected <see cref="TreeNode"/> and all of its subnodes.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnRemoveFeedStructureObject_Click(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.TopNode) return;
            treeViewFeedStructure.Nodes.Remove(selectedNode);
        }

        /// <summary>
        /// Clears <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonClearList_Click(object sender, EventArgs e)
        {
            treeViewFeedStructure.TopNode.Nodes.Clear();
        }

        /// <summary>
        /// Opens a new window to edit the selected entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void treeViewFeedStructure_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Form window;
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.TopNode) return;

            selectedNode.Toggle();

            // open a new window to change the selected object
            if (selectedNode.Tag is Group)
            {
                window = new GroupForm { Group = (Group)selectedNode.Tag };
            }
            else if (selectedNode.Tag is Implementation)
            {
                window = new ImplementationForm { Implementation = (Implementation)selectedNode.Tag };
            }
            else if (selectedNode.Tag is Archive)
            {
                window = new ArchiveForm { Archive = (Archive)selectedNode.Tag};
            }
            else if (selectedNode.Tag is Recipe)
            {
                throw new NotImplementedException();
            }
            else if (selectedNode.Tag is PackageImplementation)
            {
                window = new PackageImplementationForm { PackageImplementation = (PackageImplementation)selectedNode.Tag };
            }
            else if (selectedNode.Tag is Dependency)
            {
                window = new DependencyForm { Dependency = (Dependency)selectedNode.Tag };
            }
            else if (selectedNode.Tag is EnvironmentBinding)
            {
                window = new EnvironmentBindingForm { EnvironmentBinding = (EnvironmentBinding)selectedNode.Tag };
            }
            else if (selectedNode.Tag is OverlayBinding)
            {
                window = new OverlayBindingForm { OverlayBinding = (OverlayBinding)selectedNode.Tag };
            }
            else
            {
                throw new InvalidOperationException("Not an object to change.");
            }

            window.Owner = this;
            Enabled = false;
            window.Show();
        }

        #endregion

        #region Advanced Tab

        #region External Feeds Group

        /// <summary>
        /// Adds a clone of the <see cref="feedReferenceControl"/> to <see cref="listBoxExtFeeds"/> if no equal object is in the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnExtFeedsAdd_Click(object sender, EventArgs e)
        {
            var feedReference = feedReferenceControl.FeedReference.CloneFeedReference();
            if (string.IsNullOrEmpty(feedReference.Source)) return;
            if (!listBoxExtFeeds.Items.Contains(feedReference))
            {
                listBoxExtFeeds.Items.Add(feedReference);
            }
        }

        /// <summary>
        /// Removes the selected <see cref="FeedReference"/> from <see cref="listBoxExtFeeds"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnExtFeedsRemove_Click(object sender, EventArgs e)
        {
            var selectedItem = listBoxExtFeeds.SelectedItem;
            if (selectedItem == null) return;
            listBoxExtFeeds.Items.Remove(selectedItem);
        }

        /// <summary>
        /// Loads a clone of the selected <see cref="FeedReference"/> into <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void listBoxExtFeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (FeedReference)listBoxExtFeeds.SelectedItem;
            if (selectedItem == null) return;
            feedReferenceControl.FeedReference = selectedItem.CloneFeedReference();
        }

        /// <summary>
        /// Updates the selected <see cref="FeedReference"/> in <see cref="listBoxExtFeeds"/> with the new values from <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnExtFeedUpdate_Click(object sender, EventArgs e)
        {
            var selectedFeedReferenceIndex = listBoxExtFeeds.SelectedIndex;
            var feedReference = feedReferenceControl.FeedReference;
            if (selectedFeedReferenceIndex < 0) return;
            if (String.IsNullOrEmpty(feedReference.Source)) return;
            listBoxExtFeeds.Items[selectedFeedReferenceIndex] = feedReference;
        }

        #endregion

        #region Feed For Group

        /// <summary>
        /// Adds a new <see cref="InterfaceReference"/> with the Uri from <see cref="textFeedFor"/> to <see cref="listBoxFeedFor"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnFeedForAdd_Click(object sender, EventArgs e)
        {
            Uri uri;
            if (!ControlHelpers.IsValidFeedUrl(textFeedFor.Text, out uri)) return;
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
    }
}
