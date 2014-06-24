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
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.EntryPoints;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class EntryPointPage : UserControl, IWizardPage
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        public EntryPointPage(FeedBuilder feedBuilder)
        {
            _feedBuilder = feedBuilder;
            InitializeComponent();
        }

        public void OnPageShow()
        {
            try
            {
                comboBoxEntryPoint.Items.Clear();
                // ReSharper disable once CoVariantArrayConversion
                comboBoxEntryPoint.Items.AddRange(GetCandidates(_feedBuilder.ImplementationDirectory));
                comboBoxEntryPoint.SelectedIndex = 0;

                buttonNext.Enabled = true;
            }
            catch (OperationCanceledException)
            {
                buttonNext.Enabled = false;
            }
        }

        private Candidate[] GetCandidates(string workingDirectory)
        {
            Candidate[] candidates = null;
            try
            {
                var task = new SimpleTask("Searching for executable files", () =>
                    candidates = Detection.ListCandidates(new DirectoryInfo(workingDirectory)).ToArray());
                using (var handler = new GuiTaskHandler(this)) handler.RunTask(task);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                throw new OperationCanceledException();
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                throw new OperationCanceledException();
            }
            #endregion

            if (candidates == null || candidates.Length == 0)
            {
                Msg.Inform(this, Resources.NoEntryPointsFound, MsgSeverity.Warn);
                throw new OperationCanceledException();
            }

            return candidates;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            _feedBuilder.Candidate = comboBoxEntryPoint.SelectedItem as Candidate;
            if (_feedBuilder.Candidate != null) Next();
        }
    }
}
