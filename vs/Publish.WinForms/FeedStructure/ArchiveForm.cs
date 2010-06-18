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
using System.Runtime.InteropServices;
using Common.Archive;

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
        /// List with supported archive types by 0install. If more types become supported, add them to this list.
        /// </summary>
        private List<string> _supportedMimeTypes = new List<string> { "application/zip" };

        /// <summary>
        /// Path to the downloaded and extracted archive.
        /// </summary>
        private string _extractedArchivePath;

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
        /// Opens a dialog to ask the user where to download the archive from <see cref="hintTextBoxArchiveUrl"/>.Text, downloads the archive and sets <see cref="hintTextBoxLocalArchive"/>.<br \>
        /// When the user clicks "cancel" the archive will not be downloaded.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonArchiveDownload_Click(object sender, EventArgs e)
        {
            var url = new Uri(hintTextBoxArchiveUrl.Text);

            // detect original file name
            string fileName = url.Segments[url.Segments.Length - 1];
            if (folderBrowserDialogDownloadPath.ShowDialog() == DialogResult.Cancel) return;
            string fullFilePath = Path.Combine(folderBrowserDialogDownloadPath.SelectedPath, fileName);

            // check archive mime types
            if (comboBoxArchiveFormat.Text == "(auto detect)")
            {
                var archiveMimeType = Extractor.GuessMimeType(fileName);
                if (archiveMimeType == null)
                {
                    MessageBox.Show("The archive type of this file is not supported by 0install. Downloading archive abort.");
                    return;
                }
                else if (!_supportedMimeTypes.Contains(archiveMimeType))
                {
                    MessageBox.Show(String.Format("The extraction of the {0} format is not yet supported. Please extract the file yourself (e.g with 7zip) and set the path " +
                        "to the extracted archive in the \"Locale archive\" text box.", archiveMimeType));
                    return;
                } else
                {
                    comboBoxArchiveFormat.Text = archiveMimeType;
                }
            }

            buttonArchiveDownload.Enabled = false;
            downloadProgressBarArchive.Download = new DownloadFile(url, fullFilePath);
            downloadProgressBarArchive.UseTaskbar = true;
            downloadProgressBarArchive.Download.StateChanged += ArchiveDownloadStateChanged;
            downloadProgressBarArchive.Download.Start();
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
            /*_manifestDigest = new ManifestDigest(
                Manifest.Generate(_extractedArchivePath.Path, ManifestFormat.Sha1Old).CalculateDigest(),
                Manifest.Generate(_extractedArchivePath.Path, ManifestFormat.Sha1New).CalculateDigest(),
                Manifest.Generate(_extractedArchivePath.Path, ManifestFormat.Sha256).CalculateDigest());
            */
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
            Owner.Enabled = true;
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

        /// <summary>
        /// Sets <see cref="labelArchiveDownloadMessages"/> with the current downloading/extracing state.
        /// After the download completed, the archive will be extracted.
        /// Shows the folder structure of the archive in <see cref="treeViewExtract"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        private void ArchiveDownloadStateChanged(DownloadFile sender)
        {
            //TODO: If the archive is self-extracted the method searchs for its <see cref="_archive"/>.StartOffset and sets it in <see cref="hintTextBoxStartOffset"/>.Text .
            Invoke((SimpleEventHandler) delegate
            {
                switch (sender.State)
                {
                    case DownloadState.Started: labelArchiveDownloadMessages.Text = "Started"; break;
                    case DownloadState.Ready: labelArchiveDownloadMessages.Text = "Ready"; break;
                    case DownloadState.GettingHeaders: labelArchiveDownloadMessages.Text = "Getting headers"; break;
                    case DownloadState.GettingData: labelArchiveDownloadMessages.Text = "Getting data"; break;
                    case DownloadState.IOError:
                    case DownloadState.WebError: labelArchiveDownloadMessages.Text = sender.ErrorMessage; break;
                    case DownloadState.Complete:
                        hintTextBoxLocalArchive.Text = sender.Target;

                        labelArchiveDownloadMessages.Text = "Extracting archive...";
                        try {
                            var archiveExtractor = Extractor.CreateExtractor(comboBoxArchiveFormat.Text, sender.Target, 0, null);
                            var archiveContentList = (List<string>) archiveExtractor.ListContent();
                            treeViewExtract.Nodes.Clear();
                            
                            // TODO fill treeView

                            /*foreach (var entry in archiveContentList)
                            {
                                treeViewExtract.Nodes.Add(new TreeNode(entry));
                            }*/
                            treeViewExtract.ExpandAll();
                            _extractedArchivePath = Path.Combine(folderBrowserDialogDownloadPath.SelectedPath, Path.GetFileName(sender.Target) + "_extracted");
                            archiveExtractor.Extract(_extractedArchivePath);

                        } catch(IOException e) {
                            MessageBox.Show(e.Message);
                            return;
                        } catch(AccessViolationException e) {
                            MessageBox.Show(e.Message);
                            return;
                        }

                        break;
                }
            });
        }

        private void ArchiveForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Owner.Enabled = true;
        }

        /*private void button1_Click(object sender, EventArgs e)
        {
            using (var archiveExtractor = Extractor.CreateExtractor("application/zip", @"C:\Users\Simon\Downloads\bla.zip", 0, null))
            {
                var archiveContentList = (List<string>)archiveExtractor.ListContent();
                treeViewExtract.Nodes.Clear();
                treeViewExtract.Nodes.Add(new TreeNode("Archive"));
                var currentNode = treeViewExtract.TopNode;
                foreach (var entry in archiveContentList)
                {
                    var splitEntry = entry.Split(Path.DirectorySeparatorChar);
                    for (int i = 0; i < splitEntry.Length; i++)
                    {
                        //var newNode = new TreeNode(splitEntry[i]);
                        if (currentNode.Nodes.ContainsKey(splitEntry[i]))
                            currentNode = currentNode.Nodes[splitEntry[i]];
                        else 
                            currentNode = currentNode.Nodes.Add(entry, splitEntry[i]);
                        //currentNode = newNode;
                    }
                    currentNode = treeViewExtract.TopNode;
                }
                treeViewExtract.ExpandAll();
                treeViewExtract.Enabled = true;
            }
        }*/
    }
}
