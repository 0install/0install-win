/*
 * Copyright 2011-2013 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Net;
using System.Windows.Forms;
using Common.Controls;
using ZeroInstall.Publish.WinForms.Properties;
using Icon = ZeroInstall.Model.Icon;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// Edits <see cref="Icon"/> instances.
    /// </summary>
    public partial class IconEditor : UserControl, IEditorControl<Icon>
    {
        #region Properties
        private Icon _target;

        /// <inheritdoc/>
        public Icon Target
        {
            get { return _target; }
            set
            {
                _target = value;
                Refresh();
            }
        }

        /// <inheritdoc/>
        public Common.Undo.ICommandExecutor CommandExecutor { get; set; }
        #endregion

        #region Contructor
        public IconEditor()
        {
            InitializeComponent();

            // ReSharper disable CoVariantArrayConversion
            comboBoxMimeType.Items.AddRange(Icon.KnownMimeTypes);
            // ReSharper restore CoVariantArrayConversion
        }
        #endregion

        #region Preview
        /// <summary>
        /// Tries to download the image with the url from <see cref="uriTextBoxUrl"/> and shows it in <see cref="pictureBoxPreview"/>.
        /// Sets the right <see cref="ImageFormat"/> from the downloaded image in <see cref="comboBoxMimeType"/>.
        /// Error messages will be shown in <see cref="lableStatus"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonPreview_Click(object sender, EventArgs e)
        {
            if (!uriTextBoxUrl.IsValid || uriTextBoxUrl.Uri == null) return;

            pictureBoxPreview.Image = null;
            ShowStatusMessage(SystemColors.ControlText, Resources.DownloadingPeviewImage);

            Image icon;
            try
            {
                icon = GetImageFromUrl(uriTextBoxUrl.Uri);
            }
                #region Error handling
            catch (WebException ex)
            {
                ShowStatusMessage(Color.Red, ex.Message);
                return;
            }
            catch (InvalidDataException ex)
            {
                ShowStatusMessage(Color.Red, ex.Message);
                return;
            }
            #endregion

            if (icon.RawFormat.Equals(ImageFormat.Png))
            {
                if (string.IsNullOrEmpty(comboBoxMimeType.Text)) comboBoxMimeType.Text = Icon.MimeTypePng;
                else if (comboBoxMimeType.Text != Icon.MimeTypePng) ShowStatusMessage(Color.Red, string.Format(Resources.WrongMimeType, Icon.MimeTypePng));
                else ShowStatusMessage(Color.Green, "OK");
            }
            else if (icon.RawFormat.Equals(ImageFormat.Icon))
            {
                if (string.IsNullOrEmpty(comboBoxMimeType.Text)) comboBoxMimeType.Text = Icon.MimeTypeIco;
                else if (comboBoxMimeType.Text != Icon.MimeTypeIco) ShowStatusMessage(Color.Red, string.Format(Resources.WrongMimeType, Icon.MimeTypeIco));
                else ShowStatusMessage(Color.Green, "OK");
            }
            else ShowStatusMessage(Color.Red, Resources.ImageFormatNotSupported);

            pictureBoxPreview.Image = icon;
        }

        /// <summary>
        /// Downloads an <see cref="Image"/> from a specific url.
        /// </summary>
        /// <param name="url">To an <see cref="Image"/>.</param>
        /// <returns>The downloaded <see cref="Image"/>.</returns>
        /// <exception cref="WebException">The image file could not be downloaded.</exception>
        /// <exception cref="InvalidDataException">The downloaded data is not a valid image files</exception>
        private static Image GetImageFromUrl(Uri url)
        {
            try
            {
                using (var imageStream = WebRequest.Create(url).GetResponse().GetResponseStream())
                    return (imageStream == null) ? null : Image.FromStream(imageStream);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            #endregion
        }

        private void ShowStatusMessage(Color color, string message)
        {
            lableStatus.Text = message;
            lableStatus.ForeColor = color;
            lableStatus.Visible = true;
        }
        #endregion

        #region Event handlers
        private void comboBoxMimeType_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (comboBoxMimeType.Text == Target.MimeType) return;

            if (CommandExecutor == null) Target.MimeType = comboBoxMimeType.Text;
            else CommandExecutor.Execute(new Common.Undo.SetValueCommand<string>(() => Target.MimeType, value => Target.MimeType = value, comboBoxMimeType.Text));
        }

        private void uriTextBoxUrl_Validated(object sender, EventArgs e)
        {
            if (!uriTextBoxUrl.IsValid || uriTextBoxUrl.Uri == Target.Href) return;

            if (CommandExecutor == null) Target.Href = uriTextBoxUrl.Uri;
            else CommandExecutor.Execute(new Common.Undo.SetValueCommand<Uri>(() => Target.Href, value => Target.Href = value, uriTextBoxUrl.Uri));
        }

        public override void Refresh()
        {
            comboBoxMimeType.Text = _target.MimeType;
            uriTextBoxUrl.Uri = _target.Href;
            base.Refresh();
        }
        #endregion
    }
}
