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
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class ArchiveExtractPage : UserControl, IWizardPage
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        [CanBeNull]
        private readonly InstallerCapture _installerCapture;

        public ArchiveExtractPage([NotNull] FeedBuilder feedBuilder, [CanBeNull] InstallerCapture installerCapture = null)
        {
            InitializeComponent();

            _feedBuilder = feedBuilder;
            _installerCapture = installerCapture;
        }

        private Archive _archive;

        public void OnPageShow()
        {
            _archive = (Archive)_feedBuilder.RetrievalMethod;

            comboBoxExtract.BeginUpdate();
            comboBoxExtract.Items.Clear();

            var baseDirectory = new DirectoryInfo(_feedBuilder.TemporaryDirectory);
            baseDirectory.Walk(dir => comboBoxExtract.Items.Add(dir.RelativeTo(baseDirectory)));
            comboBoxExtract.SelectedItem = baseDirectory.WalkThroughPrefix().RelativeTo(baseDirectory);

            comboBoxExtract.EndUpdate();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            using (var handler = new GuiTaskHandler(this))
            {
                if (FileUtils.IsBreakoutPath(comboBoxExtract.Text))
                {
                    Msg.Inform(this, Resources.ArchiveBreakoutPath, MsgSeverity.Error);
                    return;
                }

                _archive.Extract = comboBoxExtract.Text ?? "";
                _feedBuilder.ImplementationDirectory = Path.Combine(_feedBuilder.TemporaryDirectory, FileUtils.UnifySlashes(_archive.Extract));

                try
                {
                    // Candidate detection is handled differently when captuing an installer
                    if (_installerCapture == null) _feedBuilder.DetectCandidates(handler);

                    _feedBuilder.CalculateDigest(handler);
                }
                    #region Error handling
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ArgumentException ex)
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
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    return;
                }
                #endregion
            }

            if (_feedBuilder.ManifestDigest.PartialEquals(ManifestDigest.Empty))
            {
                Msg.Inform(this, Resources.EmptyImplementation, MsgSeverity.Warn);
                return;
            }
            if (_feedBuilder.MainCandidate == null)
            {
                Msg.Inform(this, Resources.NoEntryPointsFound, MsgSeverity.Warn);
                return;
            }

            Next();
        }
    }
}
