/*
 * Copyright 2011 Simon E. Silva Lauinger
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
using System.Net;
using System.Security;
using System.Windows.Forms;
using C5;
using Common.Utils;
using ZeroInstall.Publish.WinForms.Properties;
using Icon = ZeroInstall.Model.Icon;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class IconManagementControl : UserControl
    {
        #region Properties
        public ArrayList<Icon> IconUrls
        {
            get
            {
                return _icons;
            }
            set
            {
                ClearControls();
                _icons.AddAll(value);
            }
        }
        #endregion
        
        #region Constants
        private readonly HashSet<ImageFormat> _supportedImageFormats = new HashSet<ImageFormat> { ImageFormat.Png, ImageFormat.Icon };

        private readonly HashDictionary<ImageFormat, String> _mimeTypeTranslator =
            new HashDictionary<ImageFormat, String> { { ImageFormat.Png, "image/png" }, { ImageFormat.Icon, "image/vnd.microsoft.icon" } };
        #endregion

        #region Attributes
        private readonly ArrayList<Icon> _icons = new ArrayList<Icon>();
        #endregion

        #region Initialization
        public IconManagementControl()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            InitializeComboBoxIconTypes();
            InitializeIconUrls();
        }

        /// <summary>
        /// Adds the supported image types and "(auto detect)" to <see cref="comboBoxIconType"/>. Sets "(auto detect)" as selected entry.
        /// </summary>
        private void InitializeComboBoxIconTypes()
        {
            comboBoxIconType.Items.Add(Resources.AutoDetect);
            comboBoxIconType.Items.AddRange(_supportedImageFormats.ToArray());
            comboBoxIconType.SelectedIndex = 0;
        }

        /// <summary>
        /// Adds wrapping delegates for adding, removing and clearing to <see cref="_icons"/> that wrap the <see cref="listBoxIconsUrls"/> methods.
        /// </summary>
        private void InitializeIconUrls()
        {
            _icons.ItemsAdded += (sender, eventArgs) => listBoxIconsUrls.Items.Add(eventArgs.Item);
            _icons.ItemsRemoved += (sender, eventArgs) => listBoxIconsUrls.Items.Remove(eventArgs.Item);
            _icons.CollectionCleared += (sender, eventArgs) => listBoxIconsUrls.Items.Clear();
        }
        #endregion

        #region Control Management
        /// <summary>
        /// Sets the embedded controls to their initial state.
        /// </summary>
        private void ClearControls()
        {
            _icons.Clear();
            uriTextBoxIconUrl.Text = string.Empty;
            pictureBoxPreview.Image = null;
            comboBoxIconType.SelectedIndex = 0;
        }
        #endregion

        #region Icon Preview
        /// <summary>
        /// Tries to download the image with the url from <see cref="uriTextBoxIconUrl"/> and shows it in <see cref="pictureBoxPreview"/>.
        /// Sets the right <see cref="ImageFormat"/> from the downloaded image in <see cref="comboBoxIconType"/>.
        /// Error messages will be shown in <see cref="lableIconUrlError"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonPreviewClick(object sender, EventArgs e)
        {
            if (uriTextBoxIconUrl.Uri == null) return;

            Image icon;

            ChangeTextOfMessageLabel(Resources.Downloading_image_for_preview, SystemColors.ControlText);

            // try downloading image
            try
            {
                icon = GetImageFromUrl(uriTextBoxIconUrl.Uri);
            }
            catch (Exception exception)
            {
                ChangeTextOfMessageLabel(exception.Message, Color.Red);
                return;
            }


            if (!_supportedImageFormats.Contains(icon.RawFormat))
            {
                ChangeTextOfMessageLabel(Resources.ImageFormatNotSupported, Color.Red);
                return;
            }

            ChangeTextOfMessageLabel(string.Empty, Color.Green);

            if (comboBoxIconType.Text == Resources.AutoDetect)
                comboBoxIconType.SelectedItem = icon.RawFormat;

            pictureBoxPreview.Image = icon;
        }

        /// <summary>
        /// Downloads an <see cref="Image"/> from a specific url.
        /// </summary>
        /// <param name="url">To an <see cref="Image"/>.</param>
        /// <returns>The downloaded <see cref="Image"/>.</returns>
        /// <exception cref="SecurityException">The caller does not have permission to connect to the requested URI or a URI that the request is redirected to.</exception>
        private static Image GetImageFromUrl(Uri url)
        {
            //TODO Exeptions dokumentieren
            var imageStream = HttpWebRequest.Create(url).GetResponse().GetResponseStream();
            return Image.FromStream(imageStream);
        }

        private void ChangeTextOfMessageLabel(string textForLabel, Color colorForLabel)
        {
            lableIconUrlError.Text = textForLabel;
            lableIconUrlError.ForeColor = colorForLabel;
        }
        #endregion

        #region Icon list controls
        /// <summary>
        /// Adds the url from <see cref="uriTextBoxIconUrl"/> and the chosen mime type in <see cref="comboBoxIconType"/>
        /// to <see cref="listBoxIconsUrls"/> if the url is a valid url.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonAddClick(object sender, EventArgs e)
        {
            if (uriTextBoxIconUrl.Uri == null) return;

            // get the icon mime type (guess or use selected)
            var mimeType = (comboBoxIconType.Text == Resources.AutoDetect
                                ? ImageUtils.GuessImageFormat(uriTextBoxIconUrl.Uri.ToString())
                                : (ImageFormat) comboBoxIconType.SelectedItem);
            var icon = new Icon { Location = uriTextBoxIconUrl.Uri };
            if (mimeType != null) icon.MimeType = _mimeTypeTranslator[mimeType];

            // if nothing is selected add to the end of the list, else insert behind the selected item.
            _icons.Add(icon);
        }

        /// <summary>
        /// Removes chosen image url in <see cref="listBoxIconsUrls"/> from the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonRemoveClick(object sender, EventArgs e)
        {
            if (listBoxIconsUrls.SelectedItem == null) return;
            var icon = (Icon) listBoxIconsUrls.SelectedItem;
            _icons.Remove(icon);
        }

        /// <summary>
        /// Replaces the text and the icon type of <see cref="uriTextBoxIconUrl"/> and <see cref="comboBoxIconType"/> with the text and the icon type from the chosen entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ListIconsUrlsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxIconsUrls.SelectedIndex < 0) return;

            var icon = _icons[listBoxIconsUrls.SelectedIndex];
            uriTextBoxIconUrl.Text = icon.LocationString;

            foreach (var supportedImageFormat in _supportedImageFormats)
            {
                if (icon.MimeType != _mimeTypeTranslator[supportedImageFormat]) continue;
                comboBoxIconType.SelectedItem = supportedImageFormat;
                break;
            }
        }
        #endregion
    }
}
