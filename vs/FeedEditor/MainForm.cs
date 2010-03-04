using System;
using System.Windows.Forms;
using Common.Storage;
using ZeroInstall.Model;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;

namespace ZeroInstall.FeedEditor
{
    public partial class MainForm : Form
    {
        private Interface _xmlInterface;

        public MainForm()
        {
            InitializeComponent();
        }
        
        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            _xmlInterface = new Interface();
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog(this);
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.ShowDialog(this);
        }

        private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _xmlInterface = XmlStorage.Load<Interface>(openFileDialog.FileName);
            FillForm();
        }

        private void saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //XmlStorage.Save<Interface>(saveFileDialog.FileName, (Interface)propertyGridInterface.SelectedObject);
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

        private void FillForm()
        {
            textName.Text = _xmlInterface.Name;
            textSummary.Text = _xmlInterface.Summary;
            //fill icons list box
            listIconsUrls.BeginUpdate();
            listIconsUrls.Items.Clear();
            foreach (Model.Icon icon in _xmlInterface.Icons)
            {
                listIconsUrls.Items.Add(icon);
            }
            listIconsUrls.EndUpdate();

            textDescription.Text = _xmlInterface.Description;
            textHomepage.Text = _xmlInterface.HomepageString;
         }

        private void btnIconListAdd_Click(object sender, EventArgs e)
        {
            var icon = new Model.Icon();
            icon.LocationString = textIconUrl.Text;
            // set mime type
            switch(comboIconType.Text) {
                case "PNG":
                    icon.MimeType = "image/png";
                    break;
                case "ICO":
                    icon.MimeType = "image/vnd-microsoft-icon";
                    break;
                default: throw new InvalidOperationException("Wrong MIME-Type");
            }

            // add icon object to list box
            if(!listIconsUrls.Items.Contains(icon)) {
                listIconsUrls.Items.Add(icon);
            }
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
            if (listIconsUrls.SelectedItem != null)
            {
                var icon = (Model.Icon)listIconsUrls.SelectedItem;
                textIconUrl.Text = icon.LocationString;
                if (icon.MimeType.Equals("image/png"))
                {
                    comboIconType.Text = "PNG";
                }
                else if (icon.MimeType.Equals("image/vnd-microsoft-icon"))
                {
                    comboIconType.Text = "ICO";
                }
            }
        }
    }
}
