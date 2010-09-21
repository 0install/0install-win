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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using C5;
using Common;
using Common.Controls;
using ZeroInstall.Model;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using ZeroInstall.Publish.WinForms.FeedStructure;
using ZeroInstall.Store.Feed;
using Binding = ZeroInstall.Model.Binding;

namespace ZeroInstall.Publish.WinForms
{
    public partial class MainForm : Form
    {
        #region Attributes

        /// <summary>
        /// The path of the file a <see cref="Feed"/> was loaded from.
        /// </summary>
        private string _pathToOpenedFeed;

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
            InitializeComponentsExtended();
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
            InitializeComboBoxGpg();
        }

        /// <summary>
        /// Initializes the <see cref="saveFileDialog"/> with a file filter for .xml files.
        /// </summary>
        private void InitializeSaveFileDialog()
        {
            if (_pathToOpenedFeed != null) saveFileDialog.InitialDirectory = _pathToOpenedFeed;
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = "ZeroInstall Feed (*.xml)|*.xml|All Files|*.*";
        }

        /// <summary>
        /// Initializes the <see cref="openFileDialog"/> with a file filter for .xml files.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ZeroInstall")]
        private void InitializeLoadFileDialog()
        {
            if (_pathToOpenedFeed != null) openFileDialog.InitialDirectory = _pathToOpenedFeed;
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = "ZeroInstall Feed (*.xml)|*.xml|All Files|*.*";
        }

        /// <summary>
        /// Adds the supported <see cref="ImageFormat"/>s to <see cref="comboBoxIconType"/>.
        /// </summary>
        private void InitializeComboBoxIconType()
        {
            comboBoxIconType.Items.AddRange(new[] { ImageFormat.Png, ImageFormat.Icon });
        }

        /// <summary>
        /// Adds <see cref="_feedToEdit"/> to the Tag of the first <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>.
        /// </summary>
        private void InitializeTreeViewFeedStructure()
        {
            treeViewFeedStructure.Nodes[0].Tag = _feedToEdit;
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
            btnAddPackageImplementation.Tag = new PackageImplementation();
            btnAddImplementation.Tag = new Implementation();
            buttonAddArchive.Tag = new Archive();
            buttonAddRecipe.Tag = new Recipe();
        }

        /// <summary>
        /// Adds a list of secret gpg keys of the user to comboBoxGpg.
        /// </summary>
        private void InitializeComboBoxGpg()
        {
            var gpg = new GnuPG();
            var bla = gpg.ListSecretKeys();
            foreach (var secretKey in bla)
            {
                toolStripComboBoxGpg.Items.Add(secretKey);
            }
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
            if (_pathToOpenedFeed != null)  saveFileDialog.InitialDirectory = _pathToOpenedFeed;
            MainForm_FormClosing(null, null);
        }

        #endregion

        #region Save and open

        /// <summary>
        /// Opens the in <see cref="openFileDialog"/> chosen feed file and fills <see cref="MainForm"/> with its values.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            _pathToOpenedFeed = openFileDialog.FileName;
            _feedToEdit = Feed.Load(openFileDialog.FileName);

            FillForm();
        }

        /// <summary>
        /// Saves the values from the filled controls on the <see cref="MainForm"/> in the feed file chosen by <see cref="openFileDialog"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void SaveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            SaveFeed(saveFileDialog.FileName);
        }

        /// <summary>
        /// Saves feed to a specific path as xml.
        /// </summary>
        /// <param name="toPath">Path to save.</param>
        private void SaveFeed(string toPath)
        {
            SaveGeneralTab();
            SaveFeedTab();
            SaveAdvancedTab();

            _feedToEdit.Save(toPath);
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageGeneral"/>.
        /// </summary>
        private void SaveGeneralTab()
        {
            _feedToEdit.Name = hintTextBoxProgramName.Text;

            _feedToEdit.Categories.Clear();
            //TODO complete setting attribute type(required: understanding of the category system)
            foreach (var category in checkedListBoxCategories.CheckedItems) _feedToEdit.Categories.Add(category.ToString());

            _feedToEdit.Icons.Clear();
            foreach (Model.Icon icon in listBoxIconsUrls.Items) _feedToEdit.Icons.Add(icon);

            _feedToEdit.Summaries.Clear();
            if (!String.IsNullOrEmpty(hintTextBoxSummary.Text)) _feedToEdit.Summaries.Add(hintTextBoxSummary.Text);

            _feedToEdit.Descriptions.Clear();
            if (!String.IsNullOrEmpty(hintTextBoxDescription.Text)) _feedToEdit.Descriptions.Add(hintTextBoxDescription.Text);

            _feedToEdit.Uri = null;
            Uri url;
            if (Uri.TryCreate(hintTextBoxInterfaceUrl.Text, UriKind.Absolute, out url)) _feedToEdit.Uri = url;

            _feedToEdit.Homepage = null;
            if (Uri.TryCreate(hintTextBoxHomepage.Text, UriKind.Absolute, out url)) _feedToEdit.Homepage = url;

            _feedToEdit.NeedsTerminal = checkBoxNeedsTerminal.Checked; 
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageFeed"/>.
        /// </summary>
        private static void SaveFeedTab()
        {
            // the feed structure objects will be saved directly into _feedToEdit => no extra saving needed.
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageAdvanced"/>.
        /// </summary>
        private void SaveAdvancedTab()
        {
            _feedToEdit.Feeds.Clear();
            foreach (var feed in listBoxExternalFeeds.Items) _feedToEdit.Feeds.Add((FeedReference)feed);
            
            _feedToEdit.FeedFor.Clear();
            foreach (InterfaceReference feedFor in listBoxFeedFor.Items) _feedToEdit.FeedFor.Add(feedFor);
            
            _feedToEdit.MinInjectorVersion = null;
            if (!String.IsNullOrEmpty(comboBoxMinInjectorVersion.SelectedText)) _feedToEdit.MinInjectorVersion = comboBoxMinInjectorVersion.SelectedText;
        }

        /// <summary>
        /// Shows a save dialog.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            switch (Msg.Choose(this, "Do you want to save the changes you made?", MsgSeverity.Information,
                true, "&Save\nSave the file and then close", "&Don't save\nIgnore the unsaved changes"))
            {
                case DialogResult.Yes:
                    if (!String.IsNullOrEmpty(_pathToOpenedFeed)) saveFileDialog.InitialDirectory = _pathToOpenedFeed;
                    if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        SaveFeed(saveFileDialog.FileName);
                        CreateFeedSignature(saveFileDialog.FileName);
                    }
                    break;
                case DialogResult.No: break;
                case DialogResult.Cancel:
                    if(e != null) e.Cancel = true;
                    break;
            }
        }

        #region feed signing
        /// <summary>
        /// Asks the user for his/hers gpg passphrase and adds the base64 coded signature of the given file to the end of it.
        /// </summary>
        /// <param name="file"></param>
        private void CreateFeedSignature(string file)
        {
            if(toolStripComboBoxGpg.SelectedItem == null) return;
            CreateSignatureFile(file);
            string base64Signature = ConvertSignatureToBase64(file);
            AddSignatureToFeed(file, base64Signature);
        }

        /// <summary>
        /// Asks the user for his/hers gpg passphrase and creates a signature file for the given file.
        /// The signature file is the filename with ".sig" at the end.
        /// </summary>
        /// <param name="file">File to create signature for.</param>
        private void CreateSignatureFile(string file)
        {
            bool wrongPassphrase = false;
            string gpgOwner = ((GpgSecretKey) toolStripComboBoxGpg.SelectedItem).Owner;
            string gpgOwnerEmail = ((GpgSecretKey) toolStripComboBoxGpg.SelectedItem).EmailAdress;
            do
            {
                try
                {
                    string passphrase;
                    if(wrongPassphrase) {
                        passphrase = InputBox.Show("Wrong passphrase entered.\nPlease retry entering the gpg passphrase for " + gpgOwner + " <" + gpgOwnerEmail + "> :", "GnuPG passphrase", String.Empty, true);
                    } else
                    {
                        passphrase = InputBox.Show("Please enter the gpg passphrase for " + gpgOwner + " <" + gpgOwnerEmail + "> :", "GnuPG passphrase", String.Empty, true);
                    }
                    if (passphrase == null) return;
                    File.Delete(file + ".sig");
                    (new GnuPG()).DetachSign(file, ((GpgSecretKey) toolStripComboBoxGpg.SelectedItem).MainSigningKey, passphrase);
                    wrongPassphrase = false;
                }
                catch (WrongPassphraseException)
                {
                    wrongPassphrase = true;
                }
            } while (wrongPassphrase);
        }

        /// <summary>
        /// Converts the signature file of <paramref name="file"/> to a base64 encoded <see langword="string"/>.
        /// </summary>
        /// <param name="file">File which signature shall be converted.</param>
        /// <returns>The base64 encoded signature.</returns>
        private static string ConvertSignatureToBase64(string file)
        {
            string signatureFile = file + ".sig";
            
            FileStream fs = File.OpenRead(signatureFile);
            byte[] bytes = new byte[fs.Length];
            try { fs.Read(bytes, 0, Convert.ToInt32(fs.Length)); }
            finally { fs.Close(); }

            string base64ConvertedSignature = Convert.ToBase64String(bytes);
            File.Delete(signatureFile);

            return base64ConvertedSignature;
        }

        /// <summary>
        /// Adds <paramref name="base64Signature"/> as a comment to the end of the feed defined by <paramref name="file"/>.
        /// </summary>
        /// <param name="file">Path to the feed to add the signature.</param>
        /// <param name="base64Signature">The base64 encoded signature.</param>
        private static void AddSignatureToFeed(string file, string base64Signature)
        {
            StreamWriter sw = new StreamWriter(file, true) { NewLine = "\n"};
            sw.WriteLine("<!-- Base64 Signature");
            sw.WriteLine(base64Signature);
            sw.WriteLine(String.Empty);
            sw.Write("-->");
            sw.Close();
        }
        #endregion
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
                foreach (var icon in _feedToEdit.Icons) listBoxIconsUrls.Items.Add(icon);
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
                treeViewFeedStructure.Nodes[0].Tag = _feedToEdit;
                BuildElementsTreeNodes(_feedToEdit.Elements, treeViewFeedStructure.Nodes[0]);
            treeViewFeedStructure.EndUpdate();

            treeViewFeedStructure.ExpandAll();
        }

        /// <summary>
        /// Fills the <see cref="tabPageAdvanced"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        private void FillAdvancedTab()
        {
            foreach (var feed in _feedToEdit.Feeds) listBoxExternalFeeds.Items.Add(feed);
            foreach (var feedFor in _feedToEdit.FeedFor) listBoxFeedFor.Items.Add(feedFor);
            comboBoxMinInjectorVersion.Text = _feedToEdit.MinInjectorVersion;
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
            if(!ControlHelpers.IsValidFeedUrl(hintTextBoxIconUrl.Text, out iconUrl)) return;

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

            if(!IsIconFormatSupported(icon.RawFormat))
            {
                lblIconUrlError.ForeColor = Color.Red;
                lblIconUrlError.Text = "Image format not supported by 0install.";
                return;
            }

            comboBoxIconType.SelectedItem = icon.RawFormat;
            pictureBoxIconPreview.Image = icon;
        }

        private static Image GetImageFromUrl(Uri url)
        {
            var fileRequest = (HttpWebRequest)WebRequest.Create(url);
            var fileReponse = (HttpWebResponse)fileRequest.GetResponse();
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
            var supportedImageFormats = new HashSet<ImageFormat>() { ImageFormat.Png, ImageFormat.Icon };
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
            if (!ControlHelpers.IsValidFeedUrl(hintTextBoxIconUrl.Text, out uri)) return;
            var icon = new Model.Icon {Location = uri};

            var imageMimeTypes = new HashDictionary<Guid, string>() {{ImageFormat.Png.Guid, "image/png"}, {ImageFormat.Icon.Guid, "image/vnd-microsoft-icon"}};
            icon.MimeType = imageMimeTypes[((ImageFormat)comboBoxIconType.SelectedItem).Guid];

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

            var icon = (Model.Icon)listBoxIconsUrls.SelectedItem;
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

        #region onChanged Events

        /// <summary>
        /// Checks if the text of <see cref="hintTextBoxInterfaceUrl"/> is a valid feed url and sets the the text to <see cref="Color.Green"/> if true or to <see cref="Color.Red"/> if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TextInterfaceUrlTextChanged(object sender, EventArgs e)
        {
            hintTextBoxInterfaceUrl.ForeColor = (ControlHelpers.IsValidFeedUrl(hintTextBoxInterfaceUrl.Text) ? Color.Green : Color.Red);
        }

        /// <summary>
        /// Checks if the text of <see cref="hintTextBoxHomepage"/> is a valid feed url and sets the the text to <see cref="Color.Green"/> if true or to <see cref="Color.Red"/> if false.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TextHomepageTextChanged(object sender, EventArgs e)
        {
            hintTextBoxHomepage.ForeColor = ControlHelpers.IsValidFeedUrl(hintTextBoxHomepage.Text) ? Color.Green : Color.Red;
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
            if (((Button)sender).Tag is Group) AddFeedStructureObject(new Group());
            else if (((Button)sender).Tag is Dependency) AddFeedStructureObject(new Dependency());
            else if (((Button)sender).Tag is EnvironmentBinding) AddFeedStructureObject(new EnvironmentBinding());
            else if (((Button)sender).Tag is OverlayBinding) AddFeedStructureObject(new OverlayBinding());
            else if (((Button)sender).Tag is PackageImplementation) AddFeedStructureObject(new PackageImplementation());
            else if (((Button)sender).Tag is Implementation) AddFeedStructureObject(new Implementation());
            else if (((Button)sender).Tag is Archive) AddFeedStructureObject(new Archive());
            else if (((Button)sender).Tag is Recipe) AddFeedStructureObject(new Recipe());
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
                    feed.Elements.Add((Group)feedStructureObject);
                } else if (feedStructureObject is Implementation)
                {
                    feed.Elements.Add((Implementation)feedStructureObject);
                }
                else if (feedStructureObject is PackageImplementation)
                {
                    feed.Elements.Add((PackageImplementation)feedStructureObject);
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
                    group.Bindings.Add((EnvironmentBinding)feedStructureObject);
                }
                else if(feedStructureObject is Group)
                {
                    group.Elements.Add((Group)feedStructureObject);
                    
                } else if(feedStructureObject is Implementation)
                {
                    group.Elements.Add((Implementation)feedStructureObject);
                } else if (feedStructureObject is OverlayBinding)
                {
                    group.Bindings.Add((OverlayBinding)feedStructureObject);
                }
                else if(feedStructureObject is PackageImplementation)
                {
                    group.Elements.Add((PackageImplementation)feedStructureObject);
                }
            }
            else if (selectedNode.Tag is Implementation)
            {
                var implementation = (Implementation) selectedNode.Tag;

                if(feedStructureObject is Archive)
                {
                    implementation.RetrievalMethods.Add((Archive)feedStructureObject);
                }
                else if (feedStructureObject is Recipe)
                {
                    implementation.RetrievalMethods.Add((Recipe)feedStructureObject);
                }
                else if (feedStructureObject is Dependency)
                {
                    implementation.Dependencies.Add((Dependency)feedStructureObject);
                } else if(feedStructureObject is EnvironmentBinding)
                {
                    implementation.Bindings.Add((EnvironmentBinding)feedStructureObject);
                } else if(feedStructureObject is OverlayBinding)
                {
                    implementation.Bindings.Add((OverlayBinding)feedStructureObject);
                }
            }
            else if (selectedNode.Tag is PackageImplementation)
            {
                var packageImplementation = (PackageImplementation) selectedNode.Tag;
                if(feedStructureObject is Dependency)
                {
                    packageImplementation.Dependencies.Add((Dependency)feedStructureObject);
                } else if(feedStructureObject is EnvironmentBinding)
                {
                    packageImplementation.Bindings.Add((EnvironmentBinding)feedStructureObject);
                }
                else if (feedStructureObject is OverlayBinding)
                {
                    packageImplementation.Bindings.Add((OverlayBinding)feedStructureObject);
                }
            }
            else if (selectedNode.Tag is Dependency)
            {
                var dependecy = (Dependency)selectedNode.Tag;
                if (feedStructureObject is EnvironmentBinding)
                {
                    dependecy.Bindings.Add((EnvironmentBinding)feedStructureObject);
                }
                else if (feedStructureObject is OverlayBinding)
                {
                    dependecy.Bindings.Add((OverlayBinding)feedStructureObject);
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
            if (selectedNode.Tag is Feed)
            {
                enableAddButtons = new[] { btnAddGroup, btnAddImplementation };
            }
            else if (selectedNode.Tag is Group)
            {
                enableAddButtons = new[] { btnAddGroup, btnAddImplementation, btnAddPackageImplementation, btnAddDependency, btnAddEnvironmentBinding, btnAddOverlayBinding };
            }
            else if (selectedNode.Tag is Implementation)
            {
                enableAddButtons = new[] { buttonAddArchive, buttonAddRecipe, btnAddDependency, btnAddEnvironmentBinding, btnAddOverlayBinding };
            }
            else if (selectedNode.Tag is Dependency)
            {
                enableAddButtons = new[] { btnAddEnvironmentBinding, btnAddOverlayBinding };
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
            if (selectedNode.Tag is Group) (new GroupForm { Group = (Group)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is Implementation) (new ImplementationForm { Implementation = (Implementation)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is Archive)
            {
                var archiveForm = new ArchiveForm { Archive = (Archive)selectedNode.Tag };
                if (archiveForm.ShowDialog() != DialogResult.OK) return;

                var manifestDigestFromArchive = archiveForm.ManifestDigest;
                var implementationNode = selectedNode.Parent;

                if (implementationNode.FirstNode.Tag is ManifestDigest)
                {
                    var existingManifestDigest = (ManifestDigest)implementationNode.FirstNode.Tag;
                    if (ControlHelpers.IsEmpty(existingManifestDigest))
                    {
                        implementationNode.FirstNode.Tag = manifestDigestFromArchive;
                        ((Implementation)implementationNode.Tag).ManifestDigest = manifestDigestFromArchive;
                    }
                    else if (!ControlHelpers.CompareManifestDigests(existingManifestDigest, manifestDigestFromArchive))
                    {
                        MessageBox.Show("The manifest digest of this archive is not the same as the manifest digest of the other archives. The archive was discarded.");
                        selectedNode.Tag = new Archive();
                        return;
                    }
                }
                else
                {
                    InsertManifestDigestNode(implementationNode, manifestDigestFromArchive);
                }
                if (String.IsNullOrEmpty(((Implementation)implementationNode.Tag).ID))
                {
                    ((Implementation) implementationNode.Tag).ID = "sha1new=" + manifestDigestFromArchive.Sha1New;
                }
            }
            else if (selectedNode.Tag is Recipe)
            {
                var recipeForm = new RecipeForm { Recipe = (Recipe) selectedNode.Tag };
                if(recipeForm.ShowDialog() != DialogResult.OK) return;

                var manifestDigestFromRecipe = recipeForm.ManifestDigest;
                var implementationNode = selectedNode.Parent;

                if (implementationNode.FirstNode.Tag is ManifestDigest)
                {
                    var existingManifestDigest = (ManifestDigest)implementationNode.FirstNode.Tag;
                    if (ControlHelpers.IsEmpty(existingManifestDigest))
                    {
                        implementationNode.FirstNode.Tag = manifestDigestFromRecipe;
                        ((Implementation)implementationNode.Tag).ManifestDigest = manifestDigestFromRecipe;
                    }
                    else if (!ControlHelpers.CompareManifestDigests(existingManifestDigest, manifestDigestFromRecipe))
                    {
                        MessageBox.Show("The manifest digest of this recipe is not the same as the manifest digest of the other retrieval methods. The recipe was discarded.");
                        selectedNode.Tag = new Recipe { Steps = {new Archive()} };
                        return;
                    }
                }
                else
                {
                    InsertManifestDigestNode(selectedNode.Parent, manifestDigestFromRecipe);
                }
            }
            else if (selectedNode.Tag is PackageImplementation) (new PackageImplementationForm { PackageImplementation = (PackageImplementation)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is Dependency) (new DependencyForm { Dependency = (Dependency)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is EnvironmentBinding) (new EnvironmentBindingForm { EnvironmentBinding = (EnvironmentBinding)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is OverlayBinding) (new OverlayBindingForm { OverlayBinding = (OverlayBinding)selectedNode.Tag }).ShowDialog();
            else if (selectedNode.Tag is ManifestDigest) (new ManifestDigestForm((ManifestDigest)selectedNode.Tag)).ShowDialog();
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
            var manifestDigestNode = new TreeNode("Manifest digest") { Tag = toAddToTag };
            ((Implementation) insertInto.Tag).ManifestDigest = toAddToTag;
            insertInto.Nodes.Insert(0, manifestDigestNode);
        }

        /// <summary>
        /// Removes the Tag of the selected <see cref="TreeNode"/> from <see cref="_feedToEdit"/> and rebuilds <see cref="treeViewFeedStructure"/>.
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
        /// Removes an feed structure object from <see cref="_feedToEdit"/>.
        /// </summary>
        /// <param name="container">Not used.</param>
        /// <param name="toRemove">Not used.</param>
        private static void RemoveObjectFromFeedStructure(object container, object toRemove)
        {
            if (toRemove is Element) ((IElementContainer)container).Elements.Remove((Element)toRemove);
            else if (toRemove is Dependency) ((ImplementationBase)container).Dependencies.Remove((Dependency)toRemove);
            else if (toRemove is Binding) ((IBindingContainer)container).Bindings.Remove((Binding)toRemove);
            else if (toRemove is RetrievalMethod)
            {
                var implementationContainer = (Implementation) container;
                implementationContainer.RetrievalMethods.Remove((RetrievalMethod)toRemove);
                if(implementationContainer.RetrievalMethods.Count == 0) implementationContainer.ManifestDigest = new ManifestDigest();
            }
        }

        /// <summary>
        /// Removes all feed structure objects from <see cref="_feedToEdit"/> and rebuilds <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonClearListClick(object sender, EventArgs e)
        {
            _feedToEdit.Elements.Clear();
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
                    var groupNode = new TreeNode("Group " + group.ToString()) {Tag = group};
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
                        var implementationNode = new TreeNode("Implementation " + implementation.ToString()) { Tag = implementation };
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
                            var packageImplementationNode = new TreeNode("Package implementation " + packageImplementation.ToString()) { Tag = packageImplementation };
                            parentNode.Nodes.Add(packageImplementationNode);
                            BuildDependencyTreeNodes(packageImplementation.Dependencies, parentNode);
                            BuildBindingTreeNodes(packageImplementation.Bindings, parentNode);
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
                string bindingType = "Unknown binding";
                if (binding is EnvironmentBinding) bindingType = "Environment binding ";
                else if (binding is OverlayBinding) bindingType = "Overlay binding ";
                var bindingNode = new TreeNode(bindingType + binding.ToString()) {Tag = binding};
                parentNode.Nodes.Add(bindingNode);
            }
        }

        private static void BuildManifestDigestTreeNode(ManifestDigest manifestDigest, TreeNode parentNode)
        {
            if(ControlHelpers.IsEmpty(manifestDigest)) return;
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
                var dependencyNode = new TreeNode("Dependency " + dependency.ToString()) { Tag = dependency };
                parentNode.Nodes.Add(dependencyNode);
                BuildBindingTreeNodes(dependency.Bindings, dependencyNode);
            }
        }

        private static void BuildRetrievalMethodsTreeNodes(IEnumerable<RetrievalMethod> retrievalMethods, TreeNode parentNode)
        {
            #region Sanity checks
            if (retrievalMethods == null) throw new ArgumentNullException("retrievalMethods");
            #endregion
            
            foreach (var retrievalMethod in retrievalMethods)
            {
                string retrievalMethodType = "Unknown retrieval method";
                if (retrievalMethod is Archive) retrievalMethodType = "Archive ";
                else if (retrievalMethod is Recipe) retrievalMethodType = "Recipe ";

                var retrievalMethodNode = new TreeNode(retrievalMethodType + retrievalMethod.ToString()) { Tag = retrievalMethod };
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
            var feedReference = feedReferenceControl.FeedReference.CloneReference();
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
            var selectedItem = (FeedReference)listBoxExternalFeeds.SelectedItem;
            if (selectedItem == null) return;
            feedReferenceControl.FeedReference = selectedItem.CloneReference();
        }

        /// <summary>
        /// Updates the selected <see cref="FeedReference"/> in <see cref="listBoxExternalFeeds"/> with the new values from <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnExtFeedUpdateClick(object sender, EventArgs e)
        {
            var selectedFeedReferenceIndex = listBoxExternalFeeds.SelectedIndex;
            var feedReference = feedReferenceControl.FeedReference.CloneReference();
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
            if (!ControlHelpers.IsValidFeedUrl(hintTextBoxFeedFor.Text, out uri)) return;
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
