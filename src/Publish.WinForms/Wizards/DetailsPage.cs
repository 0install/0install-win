﻿/*
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
using System.Windows.Forms;
using Common.Controls;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class DetailsPage : UserControl, IWizardPage
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        public DetailsPage(FeedBuilder feedBuilder)
        {
            _feedBuilder = feedBuilder;
            InitializeComponent();
        }

        public void OnPageShow()
        {
            propertyGridCandidate.SelectedObject = _feedBuilder.Candidate;
            buttonNext.Enabled = ValidateInput();
        }

        private void propertyGridCandidate_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            buttonNext.Enabled = ValidateInput();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (ValidateInput()) Next();
        }

        private bool ValidateInput()
        {
            return !string.IsNullOrEmpty(_feedBuilder.Candidate.Name) && !string.IsNullOrEmpty(_feedBuilder.Candidate.Summary) && _feedBuilder.Candidate.Version != null;
        }
    }
}
