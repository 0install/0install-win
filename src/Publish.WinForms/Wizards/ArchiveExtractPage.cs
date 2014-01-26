﻿/*
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
using Common;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class ArchiveExtractPage : UserControl, IWizardPage
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        public ArchiveExtractPage(FeedBuilder feedBuilder)
        {
            _feedBuilder = feedBuilder;
            InitializeComponent();
        }

        private Archive _archive;

        public void OnPageShow()
        {
            _archive = (Archive)_feedBuilder.RetrievalMethod;

            comboBoxExtract.BeginUpdate();
            comboBoxExtract.Items.Clear();

            var baseDirectory = new DirectoryInfo(_feedBuilder.TemporaryDirectory);
            baseDirectory.Walk(
                dir => comboBoxExtract.Items.Add(dir.RelativeTo(baseDirectory).Replace(Path.DirectorySeparatorChar, '/')));

            comboBoxExtract.EndUpdate();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (FileUtils.IsBreakoutPath(comboBoxExtract.Text))
            {
                Msg.Inform(this, Resources.ArchiveBreakoutPath, MsgSeverity.Error);
                return;
            }

            _archive.Extract = comboBoxExtract.Text;
            string path = Path.Combine(_feedBuilder.TemporaryDirectory, FileUtils.UnifySlashes(_archive.Extract));

            try
            {
                _feedBuilder.ImplementationDirectory = path;
                Next();
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
    }
}
