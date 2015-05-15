/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class InstallerCollectFilesPage : UserControl
    {
        public event Action Next;
        public event Action ExistingArchive;

        private readonly InstallerCapture _installerCapture;

        public InstallerCollectFilesPage([NotNull] InstallerCapture installerCapture)
        {
            InitializeComponent();

            _installerCapture = installerCapture;
        }

        private void ToggleControls(object sender, EventArgs e)
        {
            buttonCreateArchive.Enabled = (textBoxUrl.Text.Length > 0) && textBoxUrl.IsValid && (textBoxLocalPath.Text.Length > 0);
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = textBoxLocalPath.Text;
            saveFileDialog.ShowDialog(this);
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            textBoxLocalPath.Text = saveFileDialog.FileName;
        }

        private void buttonCreateArchive_Click(object sender, EventArgs e)
        {
            try
            {
                using (var handler = new GuiTaskHandler(this))
                    _installerCapture.CaptureSession.CollectFiles(textBoxLocalPath.Text, textBoxUrl.Uri, handler);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            Next();
        }

        private void buttonExistingArchive_Click(object sender, EventArgs e)
        {
            ExistingArchive();
        }
    }
}
