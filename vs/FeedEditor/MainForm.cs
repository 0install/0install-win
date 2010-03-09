using System;
using System.Globalization;
using System.Windows.Forms;
using Common.Storage;
using ZeroInstall.Model;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using Icon = ZeroInstall.Model.Icon;

namespace ZeroInstall.FeedEditor
{
    public partial class MainForm : Form
    {
        private string _openInterfacePath;
        public MainForm()
        {
            _openInterfacePath = null;
            InitializeComponent();
            InitializeSaveFileDialog();
            InitializeComboBoxExtFeedCpu();
            InitializeComboBoxExtFeedOs();
            InitializeComboBoxExtFeedLanguage();
        }

        private void InitializeComboBoxExtFeedLanguage()
        {
            foreach (var lang in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
            {
                comboBoxExtFeedLanguage.Items.Add(lang);
            }
            comboBoxExtFeedLanguage.SelectedIndex = 0;
        }

        private void InitializeComboBoxExtFeedOs()
        {
            foreach (var os in Enum.GetValues(typeof(OS)))
            {
                comboBoxExtFeedOS.Items.Add(os);
            }
            comboBoxExtFeedOS.SelectedIndex = (int)OS.All;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ZeroInstall")]
        private void InitializeSaveFileDialog()
        {
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = @"ZeroInstall Feed (*.xml)|*.xml";
        }

        private void InitializeComboBoxExtFeedCpu()
        {
            foreach (var cpu in Enum.GetValues(typeof(Cpu)))
            {
                comboBoxExtFeedCPU.Items.Add(cpu);
            }
            comboBoxExtFeedCPU.SelectedIndex = (int)Cpu.All;
        }

        private void ToolStripButtonNewClick(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void ToolStripButtonOpenClick(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog(this);
        }

        private void ToolStripButtonSaveClick(object sender, EventArgs e)
        {
            saveFileDialog.InitialDirectory = _openInterfacePath;
            saveFileDialog.ShowDialog(this);
        }

        private void OpenFileDialogFileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _openInterfacePath = openFileDialog.FileName;
            var zeroInterface = XmlStorage.Load<Interface>(_openInterfacePath);
            ResetForm();
            FillForm(zeroInterface);
        }

        //TODO show error messages
        private void SaveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var zeroInterface = new Interface { Name = textName.Text, Summary = textSummary.Text };
            Uri url;
            /* General Tab */
            // save categories
            foreach (var category in checkedListCategory.CheckedItems)
            {
                //TODO complete setting attribute type(required: understanding of the category system)
                zeroInterface.Categories.Add(category.ToString());
            }
            // save icon urls
            foreach (Icon icon in listIconsUrls.Items)
            {
                zeroInterface.Icons.Add(icon);
            }
            if (!String.IsNullOrEmpty(textDescription.Text))
            {
                zeroInterface.Description = textDescription.Text;
            }
            if (Uri.TryCreate(textInterfaceURL.Text, UriKind.Absolute, out url))
            {
                zeroInterface.Uri = url;
            }
            if (Uri.TryCreate(textHomepage.Text, UriKind.Absolute, out url))
            {
                zeroInterface.Homepage = url;
            }
            zeroInterface.NeedsTerminal = checkBoxNeedsTerminal.Checked;

            /* Advanced Tab */
            foreach (var feed in listBoxExtFeeds.Items)
            {
                zeroInterface.Feeds.Add((FeedReference)feed);
            }
            //TODO hier weitermachen
            foreach (var feedFor in listBoxFeedFor.Items)
            {
                //zeroInterface.FeedFor.Add(feedFor.ToString());
            }

            XmlStorage.Save(saveFileDialog.FileName, zeroInterface);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "png")]
        private void BtnIconPreviewClick(object sender, EventArgs e)
        {
            Uri iconUrl;
            Image icon;
            lblIconUrlError.ForeColor = Color.Red;
            // check url
            try
            {
                iconUrl = new Uri(textIconUrl.Text);
            }
            catch (UriFormatException)
            {
                lblIconUrlError.Text = "Invalid URL";
                return;
            }
            // check protocol
            if (!(iconUrl.Scheme == "http" || iconUrl.Scheme == "https"))
            {
                lblIconUrlError.Text = "URL must begin with \"http://\" or \"https://\"";
                return;
            }

            // try downloading image
            try
            {
                icon = GetImageFromUrl(iconUrl);
            }
            catch (WebException ex)
            {
                switch (ex.Status)
                {
                    case WebExceptionStatus.ConnectFailure:
                        lblIconUrlError.Text = "File not found";
                        break;
                    default:
                        lblIconUrlError.Text = "Couldn't download image";
                        break;
                }
                return;
            }
            catch (ArgumentException)
            {
                lblIconUrlError.Text = "URL does not describe an image";
                return;
            }
            // check if icon format is png
            if (!icon.RawFormat.Equals(ImageFormat.Png))
            {
                lblIconUrlError.Text = "Image format must be png";
                return;
            }
            iconBox.Image = icon;

            lblIconUrlError.ForeColor = Color.Green;
            lblIconUrlError.Text = "Valid URL";
        }


        private void FillForm(Interface zeroInterface)
        {
            /* General Tab */
            textName.Text = zeroInterface.Name;
            textSummary.Text = zeroInterface.Summary;
            textDescription.Text = zeroInterface.Description;
            textHomepage.Text = zeroInterface.HomepageString;
            textInterfaceURL.Text = zeroInterface.UriString;
            // fill icons list box
            listIconsUrls.BeginUpdate();
            listIconsUrls.Items.Clear();
            foreach (var icon in zeroInterface.Icons)
            {
                listIconsUrls.Items.Add(icon);
            }
            listIconsUrls.EndUpdate();
            // fill category list
            foreach (var category in zeroInterface.Categories)
            {
                if (checkedListCategory.Items.Contains(category))
                {
                    checkedListCategory.SetItemChecked(checkedListCategory.Items.IndexOf(category), true);
                }
            }
            checkBoxNeedsTerminal.Checked = zeroInterface.NeedsTerminal;

            /* Advanced Tab */
            foreach (var feed in zeroInterface.Feeds)
            {
                listBoxFeedFor.Items.Add(feed);
            }
            foreach (var feedFor in zeroInterface.FeedFor)
            {
                listBoxFeedFor.Items.Add(feedFor);
            }
            comboBoxMinInjectorVersion.Text = zeroInterface.MinInjectorVersion;
        }

        // clears all form entries
        private void ResetForm()
        {
            /* General Tab */
            textName.ResetText();
            textSummary.ResetText();
            textDescription.ResetText();
            textHomepage.ResetText();
            textInterfaceURL.ResetText();
            textIconUrl.ResetText();
            listIconsUrls.Items.Clear();
            foreach (int categoryIndex in checkedListCategory.CheckedIndices)
            {
                checkedListCategory.SetItemChecked(categoryIndex, false);
            }
            checkBoxNeedsTerminal.Checked = false;

            /* Advanced Tab */
            listBoxExtFeeds.Items.Clear();
            textExtFeedURL.Clear();
            comboBoxExtFeedLanguage.SelectedIndex = 0;
            listBoxExtFeedLanguages.Items.Clear();
            comboBoxExtFeedCPU.SelectedIndex = 0;
            comboBoxExtFeedOS.SelectedIndex = 0;
            textFeedFor.Clear();
            listBoxFeedFor.Items.Clear();
            comboBoxMinInjectorVersion.SelectedIndex = 0;
        }

        private void btnIconListAdd_Click(object sender, EventArgs e)
        {
            var icon = new Model.Icon();
            Uri uri;
            if (!IsValidFeedURL(textIconUrl.Text, out uri)) return;

            icon.Location = uri;
            // set mime type
            switch (comboIconType.Text)
            {
                case "PNG":
                    icon.MimeType = "image/png";
                    break;
                case "ICO":
                    icon.MimeType = "image/vnd-microsoft-icon";
                    break;
                default:
                    throw new InvalidOperationException("Invalid MIME-Type");
            }

            // add icon object to list box
            if (!listIconsUrls.Items.Contains(icon)) listIconsUrls.Items.Add(icon);
        }

        private void btnIconListRemove_Click(object sender, EventArgs e)
        {
            var icon = listIconsUrls.SelectedItem;
            if (listIconsUrls.SelectedItem == null) return;
            listIconsUrls.Items.Remove(icon);
        }

        private void listIconsUrls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listIconsUrls.SelectedItem == null) return;

            var icon = (Icon)listIconsUrls.SelectedItem;
            textIconUrl.Text = icon.LocationString;
            switch (icon.MimeType)
            {
                case null:
                    comboIconType.Text = String.Empty;
                    break;
                case "image/png":
                    comboIconType.Text = "PNG";
                    break;
                case "image/vnd-microsoft-icon":
                    comboIconType.Text = "ICO";
                    break;
                default:
                    throw new InvalidOperationException("Invalid MIME-Type");
            }
        }

        private void btnExtFeedsAdd_Click(object sender, EventArgs e)
        {
            var feed = new FeedReference();
            Uri uri;
            if (!IsValidFeedURL(textExtFeedURL.Text, out uri)) return;
            feed.Target = uri;
            var arch = new Architecture { Cpu = (Cpu)comboBoxExtFeedCPU.SelectedItem, OS = (OS)comboBoxExtFeedOS.SelectedIndex };
            feed.Architecture = arch;
            foreach (var lang in listBoxExtFeedLanguages.Items)
            {
                feed.Languages.Add((CultureInfo)lang);
            }

            if (!listBoxExtFeeds.Items.Contains(feed))
            {
                listBoxExtFeeds.Items.Add(feed);
            }
        }

        // check if url is a valid url (begins with http or https and has the right format)
        // and shows a message if not.
        private bool IsValidFeedURL(string url, out Uri uri)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    MessageBox.Show(string.Format("{0}\nhas to begin with \"http\" or \"https\"", url));
                    return false;
                }
                return true;
            }
            MessageBox.Show(string.Format("{0}\nis no URL", url));
            return false;
        }

        private void btnExtFeedsRemove_Click(object sender, EventArgs e)
        {
            var selectedItem = listBoxExtFeeds.SelectedItem;
            if (selectedItem == null) return;
            listBoxExtFeeds.Items.Remove(selectedItem);
        }

        private void btnExtFeedLanguageAdd_Click_1(object sender, EventArgs e)
        {
            var c = (CultureInfo)comboBoxExtFeedLanguage.SelectedItem;
            if (listBoxExtFeedLanguages.Items.Contains(c)) return;
            listBoxExtFeedLanguages.Items.Add(c);
        }

        private void btnExtFeedLanguageRemove_Click(object sender, EventArgs e)
        {
            var c = (CultureInfo)listBoxExtFeedLanguages.SelectedItem;
            if (c == null) return;
            listBoxExtFeedLanguages.Items.Remove(c);
        }

        private void listBoxExtFeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (FeedReference)listBoxExtFeeds.SelectedItem;
            if (selectedItem == null) return;
            textExtFeedURL.Text = selectedItem.TargetString;
            listBoxExtFeedLanguages.Items.Clear();
            foreach (var lang in selectedItem.Languages)
            {
                listBoxExtFeedLanguages.Items.Add(lang);
            }
            comboBoxExtFeedCPU.SelectedItem = selectedItem.Architecture.Cpu;
            comboBoxExtFeedOS.SelectedItem = selectedItem.Architecture.OS;
        }

        private void btnExtFeedLanguageClear_Click(object sender, EventArgs e)
        {
            listBoxExtFeedLanguages.Items.Clear();
        }

        private void btnFeedForAdd_Click(object sender, EventArgs e)
        {
            Uri uri;
            if (IsValidFeedURL(textFeedFor.Text, out uri))
            {
                listBoxFeedFor.Items.Add(uri);
            }
        }

        private void btnFeedForRemove_Click(object sender, EventArgs e)
        {
            var feedFor = listBoxFeedFor.SelectedItem;
            if (feedFor == null) return;
            listBoxFeedFor.Items.Remove(feedFor);
        }

        private void btnFeedForClear_Click(object sender, EventArgs e)
        {
            listBoxFeedFor.Items.Clear();
        }
    }
}
