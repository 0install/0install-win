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
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common.Controls;
using ZeroInstall.Publish.EntryPoints;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class EntryPointPage : UserControl, IWizardPage
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        public EntryPointPage([NotNull] FeedBuilder feedBuilder)
        {
            InitializeComponent();

            _feedBuilder = feedBuilder;
        }

        public void OnPageShow()
        {
            comboBoxEntryPoint.Items.Clear();
            comboBoxEntryPoint.Items.AddRange(_feedBuilder.Candidates.Cast<object>().ToArray());
            comboBoxEntryPoint.SelectedItem = _feedBuilder.MainCandidate;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            _feedBuilder.MainCandidate = comboBoxEntryPoint.SelectedItem as Candidate;
            if (_feedBuilder.MainCandidate == null) return;

            Next();
        }
    }
}
