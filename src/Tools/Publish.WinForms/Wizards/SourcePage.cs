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

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class SourcePage : UserControl
    {
        /// <summary>
        /// Raised if an <see cref="Archive"/> was chosen as the implementation source.
        /// </summary>
        public event Action Archive;

        /// <summary>
        /// Raised if a <see cref="SingleFile"/> was chosen as the implementation source.
        /// </summary>
        public event Action SingleFile;

        /// <summary>
        /// Raised if a Setup EXE/MSI/... was chosen as the implementation source.
        /// </summary>
        public event Action Setup;

        public SourcePage()
        {
            InitializeComponent();
        }

        private void buttonArchive_Click(object sender, EventArgs e)
        {
            Archive();
        }

        private void buttonSingleFile_Click(object sender, EventArgs e)
        {
            SingleFile();
        }

        private void buttonSetup_Click(object sender, EventArgs e)
        {
            Setup();
        }
    }
}
