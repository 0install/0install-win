using System;
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
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog(this);
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.InitialDirectory = _openInterfacePath;
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = "ZeroInstall Feed (*.xml)|*.xml";
            saveFileDialog.ShowDialog(this);
        }

        private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _openInterfacePath = openFileDialog.FileName;
            var zeroInterface = XmlStorage.Load<Interface>(_openInterfacePath);
            ResetForm();
            FillForm(zeroInterface);
        }

        //TODO show error messages
        private void SaveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var zeroInterface = new Interface {Name = textName.Text, Summary = textSummary.Text};
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

            //ToDo Bastian fragen, wie überprüfung im Interface
            if (textDescription.Text != String.Empty)
            {
                zeroInterface.Description = textDescription.Text;
            }
            if(Uri.IsWellFormedUriString(textInterfaceURL.Text, UriKind.Absolute))
            {
                zeroInterface.UriString = textInterfaceURL.Text;
            }
            if (Uri.IsWellFormedUriString(textHomepage.Text, UriKind.Absolute))
            {
                zeroInterface.HomepageString = textHomepage.Text;
            }

            XmlStorage.Save(saveFileDialog.FileName, zeroInterface);
        }

        private void BtnIconPreviewClick(object sender, EventArgs e)
        {
            Uri iconUrl;
            Image icon;
            lblIconUrlError.ForeColor = Color.Red;
            // check url
            try
            {
                iconUrl = new Uri(textIconUrl.Text);
            } catch(UriFormatException) {
                lblIconUrlError.Text = "Invalid URL";
                return;
            }
            // check protocol
            if(!(iconUrl.Scheme == "http" || iconUrl.Scheme == "https")) {
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
            textName.Text = zeroInterface.Name;
            textSummary.Text = zeroInterface.Summary;
            textDescription.Text = zeroInterface.Description;
            textHomepage.Text = zeroInterface.HomepageString;
            textInterfaceURL.Text = zeroInterface.UriString;

            // fill icons list box
            listIconsUrls.BeginUpdate();
            listIconsUrls.Items.Clear();
            foreach (Model.Icon icon in zeroInterface.Icons)
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
         }

        // clears all form entries
        private void ResetForm()
        {
            textName.ResetText();
            textSummary.ResetText();
            textDescription.ResetText();
            textHomepage.ResetText();
            textInterfaceURL.ResetText();
            textIconUrl.ResetText();
            listIconsUrls.Items.Clear();
            foreach(int categoryIndex in checkedListCategory.CheckedIndices)
            {
                checkedListCategory.SetItemChecked(categoryIndex, false);
            }
        }

        private void btnIconListAdd_Click(object sender, EventArgs e)
        {
            var icon = new Model.Icon {LocationString = textIconUrl.Text};
            // set mime type
            switch(comboIconType.Text) {
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
            if (listIconsUrls.SelectedItem != null)
            {
                listIconsUrls.Items.Remove((Model.Icon)listIconsUrls.SelectedItem);
            }
        }

        private void listIconsUrls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listIconsUrls.SelectedItem == null) return;

            var icon = (Icon) listIconsUrls.SelectedItem;
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
    }
}
