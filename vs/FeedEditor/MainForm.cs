using System;
using System.Windows.Forms;
using Common.Storage;
using ZeroInstall.Backend.Model;
using System.Drawing;
using System.Net;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;

namespace ZeroInstall.FeedEditor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            //propertyGridInterface.SelectedObject = new Interface();
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
            //propertyGridInterface.SelectedObject = XmlStorage.Load<Interface>(openFileDialog.FileName);
        }

        private void saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //XmlStorage.Save<Interface>(saveFileDialog.FileName, (Interface)propertyGridInterface.SelectedObject);
        }

        private void btnIconPreview_Click(object sender, EventArgs e)
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
                icon = getImageFromURL(iconUrl);
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

        private static Image getImageFromURL(Uri url)
        {
            HttpWebRequest fileRequest = (HttpWebRequest) HttpWebRequest.Create(url);
            HttpWebResponse fileReponse = (HttpWebResponse) fileRequest.GetResponse();
            Stream stream = fileReponse.GetResponseStream();
            return Image.FromStream(stream);
        }
    }
}
