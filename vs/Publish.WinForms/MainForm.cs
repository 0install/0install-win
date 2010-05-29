﻿/*
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

namespace ZeroInstall.Publish.WinForms
{
    public partial class MainForm : Form
    {
        private string _openInterfacePath;

        public MainForm()
        {
            InitializeComponent();
            _openInterfacePath = null;
            InitializeSaveFileDialog();
            InitializeLoadFileDialog();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ZeroInstall")]
        private void InitializeSaveFileDialog()
        {
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = "ZeroInstall Feed (*.xml)|*.xml|All Files|*.*";
        }

        private void InitializeLoadFileDialog()
        {
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = "ZeroInstall Feed (*.xml)|*.xml |All Files|*.*";
        }

        private void ToolStripButtonNew_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog(this);
        }

        private void ToolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.InitialDirectory = _openInterfacePath;
            saveFileDialog.ShowDialog(this);
        }

        private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _openInterfacePath = openFileDialog.FileName;
            var zeroInterface = Interface.Load(_openInterfacePath);
            ResetForm();
            FillForm(zeroInterface);
        }

        //TODO show error messages
        private void SaveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var zeroInterface = new Interface { Name = textName.Text, Summary = textSummary.Text };
            Uri url;
            /* General Tab */
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

            /* Advanced Tab */
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

            /* Feeds Tab */
            // Feed Structure          

            /* save file */
            zeroInterface.Save(saveFileDialog.FileName);
        }

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


        private void FillForm(Interface zeroInterface)
        {
            /* General Tab */
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

            /* Advanced Tab */
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

        // clears all form entries
        private void ResetForm()
        {
            /* General Tab */
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

            /* Advanced Tab */
            listBoxExtFeeds.Items.Clear();
            textFeedFor.Clear();
            listBoxFeedFor.Items.Clear();
            feedReferenceControl.FeedReference = null;
            comboBoxMinInjectorVersion.SelectedIndex = 0;
        }

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

        private void btnIconListRemove_Click(object sender, EventArgs e)
        {
            var icon = listIconsUrls.SelectedItem;
            if (listIconsUrls.SelectedItem == null) return;
            listIconsUrls.Items.Remove(icon);
        }

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

        private void btnExtFeedsAdd_Click(object sender, EventArgs e)
        {
            var feedReference = feedReferenceControl.FeedReference.CloneFeedReference();
            if (String.IsNullOrEmpty(feedReference.TargetString)) return;
            if (!listBoxExtFeeds.Items.Contains(feedReference))
            {
                listBoxExtFeeds.Items.Add(feedReference);
            }
        }

        private void btnExtFeedsRemove_Click(object sender, EventArgs e)
        {
            var selectedItem = listBoxExtFeeds.SelectedItem;
            if (selectedItem == null) return;
            listBoxExtFeeds.Items.Remove(selectedItem);
        }

        private void listBoxExtFeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (FeedReference)listBoxExtFeeds.SelectedItem;
            if (selectedItem == null) return;
            feedReferenceControl.FeedReference = selectedItem;
        }

        private void btnFeedForAdd_Click(object sender, EventArgs e)
        {
            Uri uri;
            if (!ControlHelpers.IsValidFeedUrl(textFeedFor.Text, out uri)) return;
            var interfaceReference = new InterfaceReference();
            interfaceReference.Target = uri;
            listBoxFeedFor.Items.Add(interfaceReference);
        }

        private void btnFeedForRemove_Click(object sender, EventArgs e)
        {
            var feedFor = listBoxFeedFor.SelectedItem;
            if (feedFor == null) return;
            listBoxFeedFor.Items.Remove(feedFor);
        }

        private void btnFeedForClear_Click(object sender, EventArgs e)
        {
            listBoxFeedFor.Items.Clear();
        }

        private void btnAddGroup_Click(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode ?? treeViewFeedStructure.TopNode;

            var treeNode = new TreeNode("Group");
            treeNode.Tag = new Group();

            selectedNode.Nodes.Add(treeNode);
            selectedNode.Expand();

        }

        private void treeViewFeedStructure_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            // disable all buttons
            btnAddGroup.Enabled = false;
            btnAddImplementation.Enabled = false;
            buttonRetrievalMethode.Enabled = false;
            btnAddPackageImplementation.Enabled = false;
            btnAddDependency.Enabled = false;
            btnAddEnvironmentBinding.Enabled = false;
            btnAddOverlayBinding.Enabled = false;
            // enable possible buttons
            if (selectedNode == treeViewFeedStructure.TopNode)
            {
                btnAddGroup.Enabled = true;
                btnAddImplementation.Enabled = true;
            }
            else if (selectedNode.Tag is Group)
            {
                btnAddGroup.Enabled = true;
                btnAddImplementation.Enabled = true;
                btnAddPackageImplementation.Enabled = true;
                btnAddDependency.Enabled = true;
                btnAddEnvironmentBinding.Enabled = true;
                btnAddOverlayBinding.Enabled = true;
            }
            else if (selectedNode.Tag is Implementation) {
                buttonRetrievalMethode.Enabled = true;
                btnAddDependency.Enabled = true;
                btnAddEnvironmentBinding.Enabled = true;
                btnAddOverlayBinding.Enabled = true;
            }
            else if (selectedNode.Tag is Dependency)
            {
                btnAddEnvironmentBinding.Enabled = true;
                btnAddOverlayBinding.Enabled = true;
            }
        }

        private void btnAddImplementation_Click(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode ?? treeViewFeedStructure.TopNode;
            
            var treeNode = new TreeNode("Implementation") {Tag = new Implementation()};

            selectedNode.Nodes.Add(treeNode);
            selectedNode.Expand();
        }

        private void btnAddPackageImplementation_Click(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null) return;
            
            var treeNode = new TreeNode("Package Implementation") {Tag = new PackageImplementation()};

            selectedNode.Nodes.Add(treeNode);
            selectedNode.Expand();
        }

        private void btnAddDependency_Click(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null) return;
            
            var treeNode = new TreeNode("Dependency") {Tag = new Dependency()};

            selectedNode.Nodes.Add(treeNode);
            selectedNode.Expand();
        }

        private void btnAddEnvironmentBinding_Click(object sender, EventArgs e)
        {
            var treeNode = new TreeNode("Environment binding");
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null) return;
            treeNode.Tag = new EnvironmentBinding();
            selectedNode.Nodes.Add(treeNode);
            selectedNode.Expand();
        }

        private void btnAddOverlayBinding_Click(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null) return;

            var treeNode = new TreeNode("Overlay binding");
            treeNode.Tag = new OverlayBinding();

            selectedNode.Nodes.Add(treeNode);
            selectedNode.Expand();
        }

        private void btnRemoveFeedStructureObject_Click(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.TopNode) return;
            treeViewFeedStructure.Nodes.Remove(selectedNode);
        }

        private void btnExtFeedUpdate_Click(object sender, EventArgs e)
        {
            var selectedFeedReferenceIndex = listBoxExtFeeds.SelectedIndex;
            var feedReference = feedReferenceControl.FeedReference;
            if (selectedFeedReferenceIndex < 0) return;
            if (String.IsNullOrEmpty(feedReference.TargetString)) return;
            listBoxExtFeeds.Items[selectedFeedReferenceIndex] = feedReference;
        }

        private void textInterfaceURL_TextChanged(object sender, EventArgs e)
        {
            textInterfaceURL.ForeColor = ControlHelpers.IsValidFeedUrl(textInterfaceURL.Text) ? Color.Green : Color.Red;
        }

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

        private void textHomepage_TextChanged(object sender, EventArgs e)
        {
            textHomepage.ForeColor = ControlHelpers.IsValidFeedUrl(textHomepage.Text) ? Color.Green : Color.Red;
        }

        private void comboBoxMinInjectorVersion_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonClearList_Click(object sender, EventArgs e)
        {
            treeViewFeedStructure.TopNode.Nodes.Clear();
        }

        private void buttonRetrievalMethode_Click(object sender, EventArgs e)
        {
            RetrievalMethod retrievalMethod = null;
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null) return;

            var treeNode = new TreeNode("Retrieval methode");
            treeNode.Tag = retrievalMethod;

            selectedNode.Nodes.Add(treeNode);
            selectedNode.Expand();
        }

        private void treeViewFeedStructure_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Form window;
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.TopNode) return;
            
            // open a new window to change the selected object
            if(selectedNode.Tag is Group)
            {
                window = new GroupForm {Group = (Group)selectedNode.Tag};
                window.Show(this);
            } else if(selectedNode.Tag is Implementation)
            {
                window = new ImplementationForm {Implementation = (Implementation)selectedNode.Tag};
                window.Show();
            } else if (selectedNode.Tag is RetrievalMethod)
            {
                
            } else if(selectedNode.Tag is PackageImplementation)
            {
                window = new PackageImplementationForm { PackageImplementation = (PackageImplementation)selectedNode.Tag };
                window.Show();
            } else if(selectedNode.Tag is Dependency)
            {
                window = new DependencyForm { Dependency = (Dependency)selectedNode.Tag };
                window.Show();
            } else if(selectedNode.Tag is EnvironmentBinding)
            {
                window = new EnvironmentBindingForm {EnvironmentBinding = (EnvironmentBinding)selectedNode.Tag};
                window.Show();
            } else if(selectedNode.Tag is OverlayBinding)
            {
                window = new OverlayBindingForm {OverlayBinding = (OverlayBinding)selectedNode.Tag};
                window.Show();
            } else
            {
                throw new InvalidOperationException("Not an object to change.");
            }
            
        }
    }
}
