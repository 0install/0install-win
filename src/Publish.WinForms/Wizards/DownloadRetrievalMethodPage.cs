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
using System.Windows.Forms;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class DownloadRetrievalMethodPage : UserControl
    {
        /// <summary>
        /// Raised if an online <see cref="DownloadRetrievalMethod"/> was selected.
        /// </summary>
        public event Action Online;

        /// <summary>
        /// Raised if a locally stored <see cref="DownloadRetrievalMethod"/> was selected.
        /// </summary>
        public event Action Local;

        public DownloadRetrievalMethodPage(string retrievalMethodType)
        {
            InitializeComponent();

            labelTitle.Text = string.Format(labelTitle.Text, retrievalMethodType);
            labelQuestion.Text = string.Format(labelQuestion.Text, retrievalMethodType);
        }

        private void buttonOnline_Click(object sender, EventArgs e)
        {
            Online();
        }

        private void buttonLocal_Click(object sender, EventArgs e)
        {
            Local();
        }
    }
}
