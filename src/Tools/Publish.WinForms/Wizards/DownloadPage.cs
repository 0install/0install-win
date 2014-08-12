/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.IO;
using System.Net;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class DownloadPage : UserControl
    {
        /// <summary>
        /// Raised if an <see cref="Archive"/> was chosen as the implementation source.
        /// </summary>
        public event Action Archive;

        /// <summary>
        /// Raised if a <see cref="SingleFile"/> was chosen as the implementation source.
        /// </summary>
        public event Action SingleFile;

        /// <summary>
        /// Raised if a Installer EXE was chosen as the implementation source.
        /// </summary>
        public event Action Installer;

        private readonly FeedBuilder _feedBuilder;

        public DownloadPage(FeedBuilder feedBuilder)
        {
            _feedBuilder = feedBuilder;
            InitializeComponent();
        }

        private void ToggleControls(object sender, EventArgs e)
        {
            groupLocalCopy.Enabled = checkLocalCopy.Checked;

            buttonNext.Enabled =
                (textBoxUrl.Text.Length > 0) && textBoxUrl.IsValid &&
                (!checkLocalCopy.Checked || textBoxLocalPath.Text.Length > 0);
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textBoxLocalPath.Text;
            openFileDialog.ShowDialog(this);
        }

        private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            textBoxLocalPath.Text = openFileDialog.FileName;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            var fileName = checkLocalCopy.Checked ? textBoxLocalPath.Text : textBoxUrl.Text;

            try
            {
                if (fileName.EndsWithIgnoreCase(".exe"))
                {
                    switch (Msg.YesNoCancel(this, Resources.AskInstallerEXE, MsgSeverity.Info, Resources.YesInstallerExe, Resources.NoSingleExecutable))
                    {
                        case DialogResult.Yes:
                            Installer();
                            break;
                        case DialogResult.No:
                            OnSingleFile();
                            break;
                        case DialogResult.Cancel:
                            return;
                    }
                }
                else if (Store.Model.Archive.GuessMimeType(fileName) == null) OnSingleFile();
                else OnArchive();
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (ArgumentException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            #endregion
        }

        private void OnSingleFile()
        {
            var handler = new GuiTaskHandler(this);

            Retrieve<SingleFile>();
            _feedBuilder.SetImplementationDirectory(_feedBuilder.TemporaryDirectory, handler);
            if (_feedBuilder.MainCandidate == null) Msg.Inform(this, Resources.NoEntryPointsFound, MsgSeverity.Warn);
            else SingleFile();
        }

        private void OnArchive()
        {
            Retrieve<Archive>();
            Archive();
        }

        private void Retrieve<T>() where T : DownloadRetrievalMethod, new()
        {
            var retrievalMethod = new T {Href = textBoxUrl.Uri};
            var temporaryDirectory = checkLocalCopy.Checked
                ? retrievalMethod.LocalApply(textBoxLocalPath.Text, new GuiTaskHandler(this))
                : retrievalMethod.DownloadAndApply(new GuiTaskHandler(this));

            _feedBuilder.RetrievalMethod = retrievalMethod;
            _feedBuilder.TemporaryDirectory = temporaryDirectory;
        }
    }
}
