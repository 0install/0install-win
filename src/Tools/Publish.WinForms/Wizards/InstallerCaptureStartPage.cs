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
using ZeroInstall.Publish.Capture;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class InstallerCaptureStartPage : UserControl
    {
        public Action Next;
        public Action Skip;

        private readonly InstallerCapture _installerCapture;
        private readonly FeedBuilder _feedBuilder;

        public InstallerCaptureStartPage([NotNull] InstallerCapture installerCapture, [NotNull] FeedBuilder feedBuilder)
        {
            InitializeComponent();

            _installerCapture = installerCapture;
            _feedBuilder = feedBuilder;
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            try
            {
                _installerCapture.CaptureSession = CaptureSession.Start(_feedBuilder);

                using (var handler = new GuiTaskHandler(this))
                    _installerCapture.RunInstaller(handler);
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
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (InvalidOperationException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            Next();
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            if (!Msg.YesNo(this, Resources.AskSkipCapture, MsgSeverity.Info)) return;

            _installerCapture.CaptureSession = null;

            try
            {
                using (var handler = new GuiTaskHandler(this))
                    _installerCapture.ExtractInstallerAsArchive(_feedBuilder, handler);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.InstallerExtractFailed + Environment.NewLine + ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            Skip();
        }
    }
}
