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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZeroInstall.Model;
using System.IO;
using ZeroInstall.Store.Implementation;
using Common.Storage;
using Common.Download;
using Common.Helpers;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class ArchiveForm : Form
    {
        #region Attributes

        /// <summary>
        /// The <see cref="Archive" /> to be displayed and modified by this form.
        /// </summary>
        private Archive _archive = new Archive();

        /// <summary>
        /// The <see cref="ManifestDigest"/> of the <see cref="Archive"/> edited by this form. Is not set if no <see cref="Archive"/> is available.
        /// </summary>
        private ManifestDigest _manifestDigest;

        /// <summary>
        /// Folder to store the downloaded, extracted archive.
        /// </summary>
        private TemporaryDirectory _extractedArchive;

        private string _downloadDirectory;

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="Archive" /> to be displayed and modified by this form. If <see langword="null"/>, the controls will be reset.
        /// </summary>
        public Archive Archive
        {
            get { return _archive; }
            set
            {
                _archive = value ?? new Archive();
                UpdateFormControls();
            }
        }

        /// <summary>
        /// The <see cref="ManifestDigest"/> of the <see cref="Archive"/> edited by this form. Is not set if no <see cref="Archive"/> is available.
        /// </summary>
        public ManifestDigest ManifestDigest
        {
            get { return _manifestDigest; }
            set { _manifestDigest = value; }
        }

        #endregion

        #region Initialization

        public ArchiveForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Control management

        /// <summary>
        /// Clear all controls on this form and set them (if needed) to the default values.
        /// </summary>
        private void ClearFormControls()
        {
            comboBoxArchiveFormat.SelectedIndex = 0;
            hintTextBoxArchiveUrl.Text = String.Empty;
            hintTextBoxLocalArchive.Text = String.Empty;
            hintTextBoxLocalArchive.Enabled = false;
            hintTextBoxStartOffset.Text = String.Empty;
            treeViewExtract.Nodes.Clear();
            treeViewExtract.Enabled = false;
        }

        /// <summary>
        /// Clear all controls on this form and fill them with the values from <see cref="_archive"/>.
        /// </summary>
        private void UpdateFormControls()
        {
            ClearFormControls();
            if(!String.IsNullOrEmpty(_archive.MimeType)) comboBoxArchiveFormat.SelectedText = _archive.MimeType;
            if (!String.IsNullOrEmpty(_archive.LocationString)) hintTextBoxArchiveUrl.Text = _archive.LocationString;
            if (_archive.StartOffset != default(long)) hintTextBoxStartOffset.Text = _archive.StartOffset.ToString();
        }

        #endregion

        /// <summary>
        /// Downloads a archive from <see cref="hintTextBoxArchiveUrl"/>.Text .
        /// If the archive is self-extracted the method searchs for its <see cref="_archive"/>.StartOffset and sets it in <see cref="hintTextBoxStartOffset"/>.Text .
        /// Sets <see cref="hintTextBoxLocalArchive"/> when downloaded.
        /// Shows the folder structure of the archive in <see cref="treeViewExtract"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonArchiveDownload_Click(object sender, EventArgs e)
        {
            _downloadDirectory = FileHelper.GetTempDirectory();

            Uri url = new Uri(hintTextBoxArchiveUrl.Text);
            string fileName = url.Segments[url.Segments.Length - 1];
            string fullFilePath = Path.Combine(_downloadDirectory, fileName);
            setArchiveFormat(fullFilePath);
            
            

            /*
            downloadProgressBarArchive.Download = new DownloadFile(url, filepath);
            downloadProgressBarArchive.UseTaskbar = true;
            downloadProgressBarArchive.Download.Start();*/
            
            /*_extractedArchive = new TemporaryDirectory();

            using(var tmpDownloadFolder = new TemporaryDirectory()) {
                DownloadFile archive = new DownloadFile(hintTextBoxArchiveUrl.Text, tmpDownloadFolder.Path);
                archive.Start();
                MessageBox.Show(tmpDownloadFolder.Path);
                }
            

            //TODO Download archive
            //TODO Add EventHandler to DownloadFile to know when the file is downloaded completely.
            //TODO Check if archive is self-extracted and set start-offset

            //TODO check for IOException
            //TODO check for UnauthorizedAccessException
            treeViewExtract.Nodes.Clear();
            var root = new DirectoryInfo(_extractedArchive.Path);
            treeViewExtract.Nodes.Add(root.Name);
            treeViewExtract.ExpandAll();*/

            //throw new NotImplementedException();
        }

        /// <summary>
        /// Auto detects the archive format of a file on the basis of its extension and set it into <see cref="comboBoxArchiveFormat"/>
        /// </summary>
        /// <param name="filePath">Path to the file to get the extension from.</param>
        private void setArchiveFormat(string filePath)
        {
            string fileExtension = new DirectoryInfo(filePath).Extension;
            string format;

            switch (fileExtension)
            {
                case ".rpm": format = "application/x-rpm"; break;
                case ".deb": format = "application/x-deb"; break;
                case ".tar": format = "application/x-tar"; break;
                case ".tar.bz2": format = "application/x-bzip-compressed-tar"; break;
                case ".tar.lzma": format = "application/x-lzma-compressed-tar"; break;
                case ".tar.gz":
                case ".tgz": format = "application/x-compressed-tar"; break;
                case ".zip": format = "application/zip"; break;
                case ".cab": format = "application/vnd.ms-cab-compressed"; break;
                default: format = "(auto detect)"; break;
            }

            comboBoxArchiveFormat.Text = format;
            //MessageBox.Show(format);
        }

        /// <summary>
        /// Does a depth search for every subdirectory of <paramref name="rootDirectory"/> and adds new <see cref="TreeNode"/>s as subnodes for <paramref name="rootNode"/> for it. 
        /// </summary>
        /// <param name="rootDirectory">Directory to start depth search..</param>
        /// <param name="rootNode">Node to add the new <see cref="TreeNode"/>s.</param>
        private void fillTreeViewExtract(DirectoryInfo rootDirectory, TreeNode rootNode)
        {
            if (rootDirectory.GetDirectories().Length == 0)
                return;
            else
            {
                foreach (var directory in rootDirectory.GetDirectories())
                {
                    var treeNode = new TreeNode(directory.Name);
                    rootNode.Nodes.Add(treeNode);
                    fillTreeViewExtract(directory, treeNode);
                }
            }
        }

        /// <summary>
        /// Saves the values from the filled controls to <see cref="_archive"/> and closes the window.
        /// Calculates the <see cref="ManifestDigest"/> from the archive.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            long startOffset;

            if (!String.IsNullOrEmpty(comboBoxArchiveFormat.Text)) _archive.MimeType = comboBoxArchiveFormat.Text;
            //TODO isUrl check
            if (!String.IsNullOrEmpty(hintTextBoxArchiveUrl.Text)) _archive.LocationString = hintTextBoxArchiveUrl.Text;

            // set _manifestDigest
            _manifestDigest = new ManifestDigest(
                Manifest.Generate(_extractedArchive.Path, ManifestFormat.Sha1Old).CalculateDigest(),
                Manifest.Generate(_extractedArchive.Path, ManifestFormat.Sha1New).CalculateDigest(),
                Manifest.Generate(_extractedArchive.Path, ManifestFormat.Sha256).CalculateDigest());
            
            if (long.TryParse(hintTextBoxStartOffset.Text, out startOffset)) _archive.StartOffset = startOffset;

            // follow the selected node from treeViewExtract up to the parent node and remember the passed directories to fill _archive.Extract .
            if (treeViewExtract.SelectedNode != null)
            {
                var selectedNode = treeViewExtract.SelectedNode;
                var extractPath = new StringBuilder();

                do
                {
                    extractPath.Insert(0, selectedNode.Text + "/");
                    selectedNode = selectedNode.Parent;
                } while (selectedNode != null);
                _archive.Extract = extractPath.ToString();
            }
            buttonCancel_Click(null, null);
        }

        /// <summary>
        /// Closes the window WITHOUT saving the values from the controls.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
            Dispose();
        }

        /// <summary>
        /// Changes the forecolor of <see cref="hintTextBoxArchiveUrl"/> to <see cref="Color.Green"/> if it's text is a valid internet url, or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxArchiveUrl_TextChanged(object sender, EventArgs e)
        {
            Uri uri;
            if (Uri.TryCreate(hintTextBoxArchiveUrl.Text, UriKind.Absolute, out uri))
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeFtp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    hintTextBoxArchiveUrl.ForeColor = Color.Green;
                    buttonArchiveDownload.Enabled = true;
                }
                else
                {
                    hintTextBoxArchiveUrl.ForeColor = Color.Red;
                    buttonArchiveDownload.Enabled = false;
                }
            }
            else
            {
                hintTextBoxArchiveUrl.ForeColor = Color.Red;
                buttonArchiveDownload.Enabled = false;
            }
        }
    }
}
