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

using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.Dialogs
{
    public partial class ManifestDigestDialog : Form
    {
        public ManifestDigestDialog(ManifestDigest manifestDigest)
        {
            InitializeComponent();
            hintTextBoxSha1Old.Text = manifestDigest.Sha1Old;
            hintTextBoxSha1New.Text = manifestDigest.Sha1New;
            hintTextBoxSha256.Text = manifestDigest.Sha256;
        }
    }
}
