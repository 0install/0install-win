using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Common.Controls;
using ZeroInstall.Model;
using System.IO;
using Common.Archive;
using Common.Download;
using Common.Helpers;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    /// <summary>
    /// Form to create a new <see cref="Archive"/> object.
    /// The OK button is only enabled if the user set all controls of this form with right
    /// values.
    /// </summary>
    public partial class ArchiveForm : OKCancelDialog
    {
        #region Attributes

        /// <summary>
        /// The <see cref="Archive" /> to be displayed and modified by this form.
        /// </summary>
        private Archive _archive = new Archive();

        /// <summary>
        /// The <see cref="ManifestDigest"/> of the <see cref="Archive"/> edited by this form. Only set if a valid <see cref="Archive"/> is available.
        /// </summary>
        private ManifestDigest _manifestDigest;

        /// <summary>
        /// List with supported archive types by 0install. If more types become supported, add them to this list.
        /// </summary>
        private List<string> _supportedMimeTypes = new List<string> { "application/zip" };

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="Archive" /> to be displayed and modified by this form. If this
        /// property is setted, the form will be readonly.
        /// </summary>
        public Archive Archive
        {
            get { return _archive; }
            set
            {
                setControlsReadonly();
                buttonOK.Enabled = false;
                _archive = value ?? new Archive();
                UpdateFormControls();
            }
        }

        /// <summary>
        /// The <see cref="ManifestDigest"/> of the <see cref="Archive"/> edited by this form.
        /// Only set if a valid <see cref="Archive"/> is available.
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
            comboBoxArchiveFormat.SelectedIndex = 0;
        }

        #endregion

        #region Control management

        /// <summary>
        /// Clear all controls on this form and set them (if needed) to the default values.
        /// </summary>
        private void ClearFormControls()
        {
            comboBoxArchiveFormat.SelectedText = "(auto detect)";
            hintTextBoxStartOffset.Text = String.Empty;
            hintTextBoxArchiveUrl.Text = String.Empty;
            labelArchiveDownloadMessages.Text = String.Empty;
            hintTextBoxLocalArchive.Text = String.Empty;
            treeViewExtract.Nodes.Clear();
            treeViewExtract.Nodes.Add("Top Folder");
            labelExtractArchiveMessage.Text = String.Empty;
        }

        /// <summary>
        /// Clear all controls on this form and fill them with the values from <see cref="_archive"/>.
        /// </summary>
        private void UpdateFormControls()
        {
            ClearFormControls();
            if (!String.IsNullOrEmpty(_archive.MimeType)) comboBoxArchiveFormat.Text = _archive.MimeType;
            if (_archive.StartOffset != default(long)) hintTextBoxStartOffset.Text = _archive.StartOffset.ToString();
            if (!String.IsNullOrEmpty(_archive.LocationString)) hintTextBoxArchiveUrl.Text = _archive.LocationString;
            if (!String.IsNullOrEmpty(_archive.Extract))
            {
                var currentNode = treeViewExtract.TopNode;
                var splittedPath = _archive.Extract.Split('/');
                foreach (var folder in splittedPath)
                    currentNode = currentNode.Nodes.Add(folder);
            }
        }

        /// <summary>
        /// Sets all controls as readonly, that the user can not modify them.
        /// </summary>
        private void setControlsReadonly()
        {
            comboBoxArchiveFormat.Enabled = false;
            hintTextBoxStartOffset.Enabled = false;
            hintTextBoxArchiveUrl.Enabled = false;
            treeViewExtract.Enabled = false;

            buttonArchiveDownload.Enabled = false;
            buttonChooseArchive.Enabled = false;
            buttonExtractArchive.Enabled = false;
        }

        #endregion

        #region Control events

        /// <summary>
        /// Saves the values from the filled controls to <see cref="_archive"/>.
        /// Calculates the <see cref="ManifestDigest"/> from the archive.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            Uri uri;
            long startOffset;

            if (getValidStartOffset(out startOffset)) _archive.StartOffset = startOffset;
            if (comboBoxArchiveFormat.Text != "(auto detect)") _archive.MimeType = comboBoxArchiveFormat.Text;
            if (isValidArchiveUrl(hintTextBoxArchiveUrl.Text, out uri)) _archive.Location = uri;
            _archive.Size = new FileInfo(hintTextBoxLocalArchive.Text).Length;
            _archive.Extract = treeViewExtract.SelectedNode.FullPath.Substring("Top folder".Length);    
            
            var manifestDigestPath = hintTextBoxLocalArchive.Text + "_extracted" + StringHelper.UnifySlashes(_archive.Extract);
            _manifestDigest = Manifest.CreateDigest(manifestDigestPath);
            //TODO: catch something like a access not possible excepetion
        }

        /// <summary>
        /// Closes the window WITHOUT saving the values from the controls and stops a the
        /// downloading of a archive.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (downloadProgressBarArchive.Download != null) downloadProgressBarArchive.Download.Cancel(false);
        }

        /// <summary>
        /// Stops the downloading of a archive on closing the form.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Noy used.</param>
        private void ArchiveForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (downloadProgressBarArchive.Download != null) downloadProgressBarArchive.Download.Cancel(false);
        }

        /// <summary>
        /// Opens a dialog to ask the user where to download the archive from
        /// <see cref="hintTextBoxArchiveUrl"/>.Text, downloads the archive and sets
        /// <see cref="hintTextBoxLocalArchive"/>.<br/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonArchiveDownload_Click(object sender, EventArgs e)
        {
            var url = new Uri(hintTextBoxArchiveUrl.Text);

            // detect original file name
            string fileName = url.Segments[url.Segments.Length - 1];
            // show dialog to choose download folder
            if (folderBrowserDialogDownloadPath.ShowDialog() == DialogResult.Cancel) return;
            string absoluteFilePath = Path.Combine(folderBrowserDialogDownloadPath.SelectedPath, fileName);
            if (!setArchiveMimeType(fileName)) return;

            downloadProgressBarArchive.Download = new DownloadFile(url, absoluteFilePath);
            downloadProgressBarArchive.UseTaskbar = true;
            downloadProgressBarArchive.Download.StateChanged += ArchiveDownloadStateChanged;

            // disable all buttons, because while downloading no user interaction shall be
            // possible.
            buttonArchiveDownload.Enabled = false;
            buttonChooseArchive.Enabled = false;
            buttonExtractArchive.Enabled = false;
            buttonOK.Enabled = false;

            downloadProgressBarArchive.Download.Start();
        }

        /// <summary>
        /// Shows a dialog where the user can choose a local archive and sets the the text of
        /// <see cref="hintTextBoxLocalArchive"/> with the chosen archive path.
        /// Enables <see cref="buttonExtractArchive"/> and disables <see cref="buttonOK"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonChooseArchive_Click(object sender, EventArgs e)
        {
            if (openFileDialogLocalArchive.ShowDialog() == DialogResult.Cancel) return;
            if (!setArchiveMimeType(openFileDialogLocalArchive.FileName)) return;
            hintTextBoxLocalArchive.Text = openFileDialogLocalArchive.FileName;
            
            buttonExtractArchive.Enabled = true;
            buttonOK.Enabled = false;
            hintTextBoxArchiveUrl.Enabled = false;
        }

        /// <summary>
        /// Extracts the archive from <see cref="hintTextBoxLocalArchive"/> and fills
        /// <see cref="treeViewExtract"/> with the folders in the archive.
        /// Shows a <see cref="MessageBox"/> with a error message if extracting is not
        /// possible.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonExtractArchive_Click(object sender, EventArgs e)
        {
            string archive = hintTextBoxLocalArchive.Text;
            long startOffset;
            if (!getValidStartOffset(out startOffset)) startOffset = 0;

            string extractedArchivePath = Path.Combine(Path.GetDirectoryName(archive), Path.GetFileName(archive) + "_extracted");

            try
            {
                //TODO add progress bar?
                labelExtractArchiveMessage.Text = "Extracting...";
                var archiveExtractor = Extractor.CreateExtractor(comboBoxArchiveFormat.Text, archive, startOffset);
                archiveExtractor.Extract(extractedArchivePath, null);
                buttonExtractArchive.Enabled = false;
            }
            catch (IOException err)
            {
                MessageBox.Show(err.Message);
                labelExtractArchiveMessage.Text = "Error!";
                return;
            }
            catch (AccessViolationException err)
            {
                MessageBox.Show(err.Message);
                labelExtractArchiveMessage.Text = "Error!";
                return;
            }
            treeViewExtract.Nodes.Clear();
            treeViewExtract.Nodes.Add("Top folder");
            fillTreeViewExtract(new DirectoryInfo(extractedArchivePath), treeViewExtract.TopNode);
            treeViewExtract.ExpandAll();
            labelExtractArchiveMessage.Text = "Archive extracted.";

            if(hintTextBoxArchiveUrl.Text != String.Empty) buttonOK.Enabled = true;
        }

        /// <summary>
        /// Checks if the text of <see cref="hintTextBoxArchiveUrl"/> is a valid internet url.
        /// If the url is valid, the forecolor of the text will be changed to
        /// <see cref="Color.Green"/> and <see cref="buttonArchiveDownload"/> will be ENABLED.
        /// If the url is NOT valid, the forecolor of the text will be changed to
        /// <see cref="Color.Red"/> and the <see cref="buttonArchiveDownload"/> will be DISABLED.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxArchiveUrl_TextChanged(object sender, EventArgs e)
        {
            Uri uri;
            if (isValidArchiveUrl(hintTextBoxArchiveUrl.Text, out uri))
            {
                hintTextBoxArchiveUrl.ForeColor = Color.Green;
                buttonArchiveDownload.Enabled = true;
                buttonChooseArchive.Enabled = true;
            }
            else
            {
                hintTextBoxArchiveUrl.ForeColor = Color.Red;
                buttonArchiveDownload.Enabled = false;
                buttonChooseArchive.Enabled = false;
            }
        }

        /// <summary>
        /// Disables the <see cref="buttonOK"/>.
        /// Disables the buttons <see cref="buttonArchiveDownload"/> and
        /// <see cref="buttonExtractArchive"/> and sets the forcolor to <see cref="Color.Red"/>
        /// if the text in <see cref="hintTextBoxStartOffset"/> is NOT valid.
        /// Enables the buttons and sets the forecolor to <see cref="Color.Green"/> if the text
        /// in <see cref="hintTextBoxStartOffset"/> IS valid
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxStartOffset_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = false;
            long startOffset;
            if (getValidStartOffset(out startOffset))
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

        #endregion

        /// <summary>
        /// If in <see cref="comboBoxArchiveFormat"/> is chosen "(auto detect)", the mime type
        /// of <paramref name="fileName"/> will be guessed and setted into 
        /// <see cref="comboBoxArchiveType"/>.
        /// If the mime type is not supported by 0install, a <see cref="MessageBox"/> with an
        /// error message will be shown to the user.
        /// </summary>
        /// <param name="fileName">File to guess the mime type for.</param>
        /// <returns><see langword="true"/>, if mime type is supported by 0install, else
        /// <see langword="false"/></returns>
        private bool setArchiveMimeType(string fileName)
        {
            if (comboBoxArchiveFormat.SelectedIndex == 0)
            {
                var archiveMimeType = Extractor.GuessMimeType(fileName);
                string errorMessage = null;
                if (archiveMimeType == null)
                {
                    errorMessage = "The archive type of this file is not supported by " +
                        "0install. Downloading archive abort.";
                }
                else if (!_supportedMimeTypes.Contains(archiveMimeType))
                {
                    errorMessage = String.Format("The extraction of the {0} format is not" +
                        " yet supported. Please extract the file yourself (e.g with 7zip) and" +
                        " set the path to the extracted archive in the \"Locale archive\"" + 
                        " text box.\nATTENTION: DO THIS ONLY AND REALLY ONLY, IF THE FILES"+
                        " IN YOUR ARCHIVE DOESN'T USE UNIX X-BIT!", archiveMimeType);
                }
                if (errorMessage == null)
                {
                    comboBoxArchiveFormat.Text = archiveMimeType;
                    return true;
                } else
                {
                    MessageBox.Show(errorMessage);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Inserts a folder tree in a node of <see cref="treeViewExtract"/>.
        /// </summary>
        /// <param name="extractedDirectory">Top folder of a folder tree</param>
        /// <param name="currentNode">Node to insert the folder tree</param>
        private void fillTreeViewExtract(DirectoryInfo extractedDirectory, TreeNode currentNode)
        {
            var folderList = extractedDirectory.GetDirectories();
            if (folderList.Length == 0) return;
            foreach (var folder in folderList)
            {
                fillTreeViewExtract(folder, currentNode.Nodes.Add(folder.Name));
            }
        }

        /// <summary>
        /// Checks if <paramref name="prooveUrl"/> is a valid internet url and creates a new
        /// <see cref="Uri"/> object.
        /// </summary>
        /// <param name="prooveUrl">Url to check.</param>
        /// <param name="uri">New <see cref="Uri"/> object with <paramref name="prooveUrl"/>
        /// </param>
        /// <returns><see langword="true"/> if <paramref name="prooveUrl"/> is a valid internet
        /// url, else <see langword="false"/></returns>
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
        /// Sets <see cref="labelArchiveDownloadMessages"/> with the current download state.
        /// If there was an error while downloading, the buttons
        /// <see cref="buttonArchiveDownload"/> and <see cref="buttonChooseArchive"/> will be
        /// enabled and a <see cref="MessageBox"/> with a error message will be shown. If the
        /// download has completed, the button <see cref="buttonExtractArchive"/> will be
        /// enabled, too.
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
                        
                        buttonArchiveDownload.Enabled = true;
                        buttonChooseArchive.Enabled = true;
                        buttonExtractArchive.Enabled = true;
                        hintTextBoxArchiveUrl.Enabled = false;
                        break;
                }
            });
        }

        /// <summary>
        /// Checks if <see cref="hintTextBoxStartOffset"/> contains a valid start offset
        /// (is a number >= 0) or <see cref="String.Empty"/>.
        /// </summary>
        /// <param name="startOffset">Variable where to store the startOffset.</param>
        /// <returns><see langword="true"/> if the text in <see cref="hintTextBoxStartOffset"/>
        /// is a number >= 0, else <see langword="false"/>.</returns>
        private bool getValidStartOffset(out long startOffset)
        {
            if (hintTextBoxStartOffset.Text == String.Empty)
            {
                startOffset = 0;
                return true;
            }
            return (long.TryParse(hintTextBoxStartOffset.Text, out startOffset)
                && startOffset >= 0);
        }
    }
}