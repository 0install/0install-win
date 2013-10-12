/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class ArchiveExtractPage : UserControl
    {
        /// <summary>
        /// Raised with the selected <see cref="Archive.Extract"/> value.
        /// </summary>
        public event Action<string> Continue;

        public ArchiveExtractPage()
        {
            InitializeComponent();
        }

        #region Archive
        private Archive _archive;
        private TemporaryDirectory _tempDirectory;

        /// <summary>
        /// Injects the selected archive and an extracted image of it.
        /// </summary>
        public void SetArchive(Archive archive, TemporaryDirectory tempDirectory)
        {
            _archive = archive;
            _tempDirectory = tempDirectory;

            comboBoxExtract.BeginUpdate();
            comboBoxExtract.Items.Clear();

            var baseDirectory = new DirectoryInfo(tempDirectory);
            baseDirectory.Walk(
                dir => comboBoxExtract.Items.Add(dir.RelativeTo(baseDirectory).Replace(Path.DirectorySeparatorChar, '/')));

            comboBoxExtract.EndUpdate();
        }
        #endregion

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            if (FileUtils.IsBreakoutPath(comboBoxExtract.Text))
            {
                Msg.Inform(this, Resources.ArchiveBreakoutPath, MsgSeverity.Error);
                return;
            }

            _archive.Extract = comboBoxExtract.Text;
            string path = Path.Combine(_tempDirectory, FileUtils.UnifySlashes(_archive.Extract));

            try
            {
                Continue(path);
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
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
    }
}
