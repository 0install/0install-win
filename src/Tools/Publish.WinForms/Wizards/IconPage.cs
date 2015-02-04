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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using ZeroInstall.Publish.EntryPoints;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class IconPage : UserControl, IWizardPage
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        public IconPage(FeedBuilder feedBuilder)
        {
            _feedBuilder = feedBuilder;
            InitializeComponent();
        }

        #region Export icon from EXE
        private Icon _icon;

        public void OnPageShow()
        {
            var windowsExe = _feedBuilder.MainCandidate as WindowsExe;
            if (windowsExe == null)
                pictureBoxIcon.Visible = buttonSaveIco.Enabled = buttonSavePng.Enabled = false;
            else
            {
                _icon = windowsExe.ExtractIcon();
                pictureBoxIcon.Image = _icon.ToBitmap();
                pictureBoxIcon.Visible = buttonSaveIco.Enabled = buttonSavePng.Enabled = true;
            }
        }

        private void buttonSaveIco_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog {Filter = "Windows Icon files|*.ico|All files|*.*"})
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (var stream = File.Create(saveFileDialog.FileName))
                        _icon.Save(stream);
                }
            }
        }

        private void buttonSavePng_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog {Filter = "PNG image files|*.png|All files|*.*"})
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                    _icon.ToBitmap().Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }
        #endregion

        private void buttonNext_Click(object sender, EventArgs e)
        {
            _feedBuilder.Icons.Clear();
            try
            {
                if (textBoxHrefIco.Uri != null) _feedBuilder.Icons.Add(new Store.Model.Icon {Href = textBoxHrefIco.Uri, MimeType = Store.Model.Icon.MimeTypeIco});
                if (textBoxHrefPng.Uri != null) _feedBuilder.Icons.Add(new Store.Model.Icon {Href = textBoxHrefPng.Uri, MimeType = Store.Model.Icon.MimeTypePng});
            }
            catch (UriFormatException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }

            if (_feedBuilder.Icons.Count != 2)
                if (!Msg.YesNo(this, Resources.AskSkipIcon, MsgSeverity.Info)) return;
            Next();
        }
    }
}
