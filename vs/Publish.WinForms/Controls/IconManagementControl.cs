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
                return _iconUrls;
            }
            set
            {
                ClearControls();
                _iconUrls = value;
                InitializeIconUrls();
                foreach (var iconUrl in _iconUrls)
                {
                    listBoxIconsUrls.Items.Add(iconUrl);
                }
            }
        }

        #endregion
        
        #region Constants

        private readonly HashSet<ImageFormat> _supportedImageFormats = new HashSet<ImageFormat> { ImageFormat.Png, ImageFormat.Icon };
        #endregion

        #region Attributes

        private ArrayList<Icon> _iconUrls = new ArrayList<Icon>();

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
        /// Adds wrapping delegates for adding, removing and clearing to <see cref="_iconUrls"/> that wrap the <see cref="listBoxIconsUrls"/> methods.
        /// </summary>
        private void InitializeIconUrls()
        {
            _iconUrls.ItemsAdded += (sender, eventArgs) => listBoxIconsUrls.Items.Add(eventArgs.Item);
            _iconUrls.ItemsRemoved += (sender, eventArgs) => listBoxIconsUrls.Items.Remove(eventArgs.Item);
            _iconUrls.CollectionCleared += (sender, eventArgs) => listBoxIconsUrls.Items.Clear();
        }

        #endregion

        #region Control Management

        /// <summary>
        /// Sets the embedded controls to their initial state.
        /// </summary>
        private void ClearControls()
        {
            _iconUrls.Clear();
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
            Image icon;

            ChangeTextOfMessageLabel("Downloading image for preview...", SystemColors.ControlText);

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
                                : comboBoxIconType.SelectedItem);
            var icon = new Icon { Location = uriTextBoxIconUrl.Uri };
            if (mimeType != null) icon.MimeType = mimeType.ToString();

            // if nothing is selected add to the end of the list, else insert behind the selected item.
            _iconUrls.Add(icon);
        }

        /// <summary>
        /// Removes chosen image url in <see cref="listBoxIconsUrls"/> from the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonRemoveClick(object sender, EventArgs e)
        {
            var icon = (Icon) listBoxIconsUrls.SelectedItem;
            if (listBoxIconsUrls.SelectedItem == null) return;
            _iconUrls.Remove(icon);
        }

        /// <summary>
        /// Replaces the text and the icon type of <see cref="uriTextBoxIconUrl"/> and <see cref="comboBoxIconType"/> with the text and the icon type from the chosen entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ListIconsUrlsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxIconsUrls.SelectedIndex < 0) return;

            var icon = _iconUrls[listBoxIconsUrls.SelectedIndex];
            uriTextBoxIconUrl.Text = icon.LocationString;

            foreach (var supportedImageFormat in _supportedImageFormats)
            {
                if (icon.MimeType != supportedImageFormat.ToString()) continue;
                comboBoxIconType.SelectedItem = supportedImageFormat;
                break;
            }
        }

        #endregion
    }
}
