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
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class InstallerCaptureDiffPage : UserControl
    {
        public Action AsArchive;
        public Action AltSource;

        private readonly InstallerCapture _installerCapture;
        private readonly FeedBuilder _feedBuilder;

        public InstallerCaptureDiffPage([NotNull] InstallerCapture installerCapture, [NotNull] FeedBuilder feedBuilder)
        {
            InitializeComponent();

            _installerCapture = installerCapture;
            _feedBuilder = feedBuilder;
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = textBoxInstallationDir.Text;
            folderBrowserDialog.ShowDialog(this);
            textBoxInstallationDir.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            var session = _installerCapture.CaptureSession;
            if (session == null) return;

            try
            {
                session.InstallationDir = textBoxInstallationDir.Text;
                using (var handler = new GuiTaskHandler(this))
                    session.Diff(handler);
            }
                #region Error handling
            catch (InvalidOperationException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            #endregion

            try
            {
                using (var handler = new GuiTaskHandler(this))
                    _installerCapture.ExtractInstallerAsArchive(_feedBuilder, handler);

                AsArchive();
            }
            catch (IOException)
            {
                Msg.Inform(this, Resources.InstallerExtractFailed + Environment.NewLine + Resources.InstallerNeedAltSource, MsgSeverity.Info);
                AltSource();
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
    }
}
