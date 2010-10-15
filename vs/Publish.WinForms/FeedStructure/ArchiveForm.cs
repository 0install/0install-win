/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    /// <summary>
    /// Form to create a new <see cref="Archive"/> object.
    /// The OK button is only enabled if the user set all controls of this form with right
    /// values.
    /// </summary>
    public partial class ArchiveForm : OKCancelDialog
    {
        #region Properties

        /// <summary>
        /// The <see cref="Archive" /> to be displayed and modified by this form.
        /// </summary>
        public Archive Archive
        {
            get { return archiveControl.Archive; }
            set
            {
                archiveControl.Archive = value;
            }
        }

        /// <summary>
        /// The <see cref="ManifestDigest"/> of the <see cref="Archive"/> edited by this form.
        /// </summary>
        public ManifestDigest ManifestDigest
        {
            get { return archiveControl.ManifestDigest; }
        }

        /// <summary>
        /// Path to the extracted archive. <see langword="null"/> when archive isn't extracted yet.
        /// </summary>
        public string ExtractedArchivePath
        {
            get { return archiveControl.ExtractedArchivePath; }
        }

        #endregion
        
        #region Initialization

        public ArchiveForm()
        {
            InitializeComponent();
            archiveControl.NoValidArchive += delegate { buttonOK.Enabled = false; };
            archiveControl.ValidArchive += delegate { buttonOK.Enabled = true; };
        }

        #endregion
    }
}