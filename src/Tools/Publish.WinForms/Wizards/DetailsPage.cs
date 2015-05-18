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
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common.Controls;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class DetailsPage : UserControl, IWizardPage
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        public DetailsPage([NotNull] FeedBuilder feedBuilder)
        {
            InitializeComponent();

            _feedBuilder = feedBuilder;
        }

        public void OnPageShow()
        {
            propertyGridCandidate.SelectedObject = _feedBuilder.MainCandidate;
            buttonNext.Enabled = ValidateInput();
        }

        private void propertyGridCandidate_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            buttonNext.Enabled = ValidateInput();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            Next();
        }

        private bool ValidateInput()
        {
            return !string.IsNullOrEmpty(_feedBuilder.MainCandidate.Name) && !string.IsNullOrEmpty(_feedBuilder.MainCandidate.Summary) && _feedBuilder.MainCandidate.Version != null;
        }
    }
}
