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
using System.Windows.Forms;
using ZeroInstall.Publish.EntryPoints;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    public partial class DetailsPage : UserControl
    {
        private Candidate _candidate;
        public event Action Continue;

        public DetailsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Injects the selected candidate.
        /// </summary>
        public void SetCandidate(Candidate candidate)
        {
            propertyGridCandidate.SelectedObject = _candidate = candidate;
            buttonContinue.Enabled = ValidateInput();
        }

        private void propertyGridCandidate_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            buttonContinue.Enabled = ValidateInput();
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            if (ValidateInput()) Continue();
        }

        private bool ValidateInput()
        {
            return !string.IsNullOrEmpty(_candidate.Name) && !string.IsNullOrEmpty(_candidate.Description) && _candidate.Version != null;
        }
    }
}
