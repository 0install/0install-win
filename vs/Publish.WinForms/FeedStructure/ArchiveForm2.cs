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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZeroInstall.Model;
using System.IO;
using Common.Download;
using Common.Helpers;
using Common.Archive;
using ZeroInstall.Store.Implementation;
using Common.Controls;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class ArchiveForm2 : OKCancelDialog
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
                //TODO if archive isn't a completely new Archive, the controls musst be readonly.
                _archive = value ?? new Archive();
                buttonOK.Enabled = false;
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

        public ArchiveForm2()
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
            hintTextBoxStartOffset.Text = String.Empty;
            treeViewExtract.Nodes.Clear();
        }

        /// <summary>
        /// Clear all controls on this form and fill them with the values from <see cref="_archive"/>.
        /// </summary>
        private void UpdateFormControls()
        {
            ClearFormControls();
            if(!String.IsNullOrEmpty(_archive.MimeType)) comboBoxArchiveFormat.Text = _archive.MimeType;
            if (!String.IsNullOrEmpty(_archive.LocationString)) hintTextBoxArchiveUrl.Text = _archive.LocationString;
            if (_archive.StartOffset != default(long)) hintTextBoxStartOffset.Text = _archive.StartOffset.ToString();
        }

        #endregion

        public event EventHandler ButtonOKClick;

        /// <summary>
        /// Opens a dialog to ask the user where to download the archive from <see cref="hintTextBoxArchiveUrl"/>.Text, downloads the archive and sets <see cref="hintTextBoxLocalArchive"/>.<br/>
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

            if (!setArchiveMimeType(fileName)) return;
            buttonArchiveDownload.Enabled = false;
            downloadProgressBarArchive.Download = new DownloadFile(url, fullFilePath);
            downloadProgressBarArchive.UseTaskbar = true;
            downloadProgressBarArchive.Download.StateChanged += ArchiveDownloadStateChanged;
            downloadProgressBarArchive.Download.Start();

            buttonArchiveDownload.Enabled = false;
            buttonChooseArchive.Enabled = false;
            buttonExtractArchive.Enabled = false;
            buttonOK.Enabled = false;
        }

        /// <summary>
        /// Guesses the mime type of <paramref name="fileName"/> if in <see cref="comboBoxArchiveFormat"/> is chose "(auto detect)".
        /// Sets the guessed mime type in <see cref="comboBoxArchiveType"/>.
        /// Shows a <see cref="MessageBox"/> if mime type is not supported.
        /// </summary>
        /// <param name="fileName">file to guess the mime type for.</param>
        /// <returns>true, if mime type is supported by 0install.</returns>
        private bool setArchiveMimeType(string fileName)
        {
            if (comboBoxArchiveFormat.Text == "(auto detect)")
            {
                var archiveMimeType = Extractor.GuessMimeType(fileName);
                if (archiveMimeType == null)
                {
                    MessageBox.Show("The archive type of this file is not supported by 0install. Downloading archive abort.");
                    return false;
                }
                else if (!_supportedMimeTypes.Contains(archiveMimeType))
                {
                    MessageBox.Show(String.Format("The extraction of the {0} format is not yet supported. Please extract the file yourself (e.g with 7zip) and set the path " +
                        "to the extracted archive in the \"Locale archive\" text box.", archiveMimeType));
                    return false;
                }
                else
                {
                    comboBoxArchiveFormat.Text = archiveMimeType;
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the directory structur of the archive in <param name="archivePath" /> and shows it in <see cref="treeViewExtract"/>.
        /// </summary>
        private void fillTreeViewExtract(string archivePath)
        {
            treeViewExtract.Nodes.Clear();
            long startOffset;
            long.TryParse(hintTextBoxStartOffset.Text, out startOffset);
            if (startOffset < 0) startOffset = 0;
            using (var archiveExtractor = Extractor.CreateExtractor(comboBoxArchiveFormat.Text, archivePath, startOffset))
            {
                var archiveContentList = (List<string>)archiveExtractor.ListDirectories();
                treeViewExtract.Nodes.Add(new TreeNode("Archive"));
                var currentNode = treeViewExtract.TopNode;
                foreach (var entry in archiveContentList)
                {
                    var splitEntry = entry.Split(Path.DirectorySeparatorChar);
                    for (int i = 0; i < splitEntry.Length; i++)
                    {
                        if (splitEntry[i] == String.Empty) continue;
                        if (currentNode.Nodes.ContainsKey(splitEntry[i]))
                            currentNode = currentNode.Nodes[splitEntry[i]];
                        else
                            currentNode = currentNode.Nodes.Add(splitEntry[i], splitEntry[i]);
                    }
                    currentNode = treeViewExtract.TopNode;
                }
            }
            treeViewExtract.ExpandAll();
        }

        /// <summary>
        /// Saves the values from the filled controls to <see cref="_archive"/> and closes the window.
        /// Calculates the <see cref="ManifestDigest"/> from the archive.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            Uri uri;

            long startOffset = getValidStartOffset();
            if(startOffset > 0) _archive.StartOffset = startOffset;

            if (comboBoxArchiveFormat.Text != "(auto detect)") _archive.MimeType = comboBoxArchiveFormat.Text;
            if (isValidArchiveUrl(hintTextBoxArchiveUrl.Text, out uri)) _archive.Location = uri;

            _archive.Size = new FileInfo(hintTextBoxLocalArchive.Text).Length;
            // trigger event
            ButtonOKClick(sender, e);
            
            buttonCancel_Click(null, null);
        }

        /// <summary>
        /// Closes the window WITHOUT saving the values from the controls.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if(downloadProgressBarArchive.Download != null) downloadProgressBarArchive.Download.Cancel(false);
            if(Owner != null) Owner.Enabled = true;
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
            if(isValidArchiveUrl(hintTextBoxArchiveUrl.Text, out uri))
            {
                hintTextBoxArchiveUrl.ForeColor = Color.Green;
                buttonArchiveDownload.Enabled = true;
            } else {
                hintTextBoxArchiveUrl.ForeColor = Color.Red;
                buttonArchiveDownload.Enabled = false;
            }
        }

        private bool isValidArchiveUrl(string prooveUrl, out Uri uri)
        {
            if (Uri.TryCreate(prooveUrl, UriKind.Absolute, out uri))
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeFtp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    return true;
                }
                return false;
            }
            return false;
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
                    case DownloadState.WebError:
                        labelArchiveDownloadMessages.Text = "Error!";
                        MessageBox.Show(sender.ErrorMessage);
                        buttonArchiveDownload.Enabled = true;
                        buttonChooseArchive.Enabled = true;
                        break;
                    case DownloadState.Complete:
                        hintTextBoxLocalArchive.Text = sender.Target;
                        fillTreeViewExtract(sender.Target);
                        
                        buttonArchiveDownload.Enabled = true;
                        buttonChooseArchive.Enabled = true;
                        buttonExtractArchive.Enabled = true;
                        break;
                }
            });
        }

        private void ArchiveForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(downloadProgressBarArchive.Download != null) downloadProgressBarArchive.Download.Cancel(false);
            Owner.Enabled = true;
        }

        private void hintTextBoxStartOffset_TextChanged(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(hintTextBoxStartOffset.Text)) return;
            if (getValidStartOffset() >= 0)
            {
                buttonArchiveDownload.Enabled = true;
                buttonExtractArchive.Enabled = true;

                hintTextBoxStartOffset.ForeColor = Color.Green;
            }
            else
            {
                buttonArchiveDownload.Enabled = false;
                buttonExtractArchive.Enabled = false;

                hintTextBoxStartOffset.ForeColor = Color.Red;
            }
            buttonOK.Enabled = false;
        }

        private void buttonChooseArchive_Click(object sender, EventArgs e)
        {
            if (openFileDialogLocalArchive.ShowDialog() == DialogResult.Cancel) return;
            if (!setArchiveMimeType(openFileDialogLocalArchive.FileName)) return;
            hintTextBoxLocalArchive.Text = openFileDialogLocalArchive.FileName;
            fillTreeViewExtract(hintTextBoxLocalArchive.Text);
            
            if (getValidStartOffset() < 0) return;
            buttonExtractArchive.Enabled = true;
            buttonOK.Enabled = false;
        }

        private void buttonExtractArchive_Click(object sender, EventArgs e)
        {
            string archive = hintTextBoxLocalArchive.Text;
            long startOffset = getValidStartOffset();
            if (startOffset < 0) startOffset = 0;

            treeViewExtract.PathSeparator = Path.DirectorySeparatorChar.ToString();
            string extract = treeViewExtract.SelectedNode.FullPath.Substring(8);
            treeViewExtract.PathSeparator = "/";

            string extractedArchivePath = Path.Combine(Path.GetDirectoryName(archive), Path.GetFileName(archive) + "_extracted");

            labelExtractArchive.Text = "Extracting...";
            try
            {
                var archiveExtractor = Extractor.CreateExtractor(comboBoxArchiveFormat.Text, archive, startOffset);
                archiveExtractor.Extract(extractedArchivePath, extract);
            }
            catch (IOException err)
            {
                MessageBox.Show(err.Message);
                labelExtractArchive.Text = "Error!";
                return;
            }
            catch (AccessViolationException err)
            {
                MessageBox.Show(err.Message);
                labelExtractArchive.Text = "Error!";
                return;
            }

            _manifestDigest = Manifest.CreateDigest(extractedArchivePath);
            _archive.Extract = treeViewExtract.SelectedNode.FullPath.Substring(7);

            labelExtractArchive.Text = "Archive extracted.";
            
            buttonOK.Enabled = true;
        }

        /// <summary>
        /// Checks if <see cref="hintTextBoxStartOffset"/> contains a valid start offset
        /// (is a number >= 0).
        /// </summary>
        /// <returns>The start offset if valid. negative number, if not.</returns>
        private long getValidStartOffset()
        {
            long startOffset = -1;
            long.TryParse(hintTextBoxStartOffset.Text, out startOffset);
            return startOffset;
        }
    }
}
