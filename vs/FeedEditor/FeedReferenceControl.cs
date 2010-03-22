using System;
using System.Drawing;
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.FeedEditor
{
    public partial class FeedReferenceControl : UserControl
    {
        private FeedReference _feedReference = new FeedReference();

        public FeedReference GetFeedReference()
        {
            return _feedReference;
        }

        public void SetFeedReference(FeedReference value)
        {
            if (value == null)
            {
                _feedReference = new FeedReference();
                textBoxExtFeedURL.Text = String.Empty;
            }
            else
            {
                _feedReference = value;
                textBoxExtFeedURL.Text = _feedReference.TargetString;
            }
            targetBaseControl.TargetBase = _feedReference;
        }

        public FeedReferenceControl()
        {
            InitializeComponent();
            targetBaseControl.TargetBase = _feedReference;
        }

        private void textBoxExtFeedURL_Enter(object sender, EventArgs e)
        {
            textBoxExtFeedURL.Text = _feedReference == null ? String.Empty : _feedReference.TargetString;
        }

        private void textBoxExtFeedURL_TextChanged(object sender, EventArgs e)
        {
            if (_feedReference == null) return;
            Uri uri;
            if (IsValidFeedURL(textBoxExtFeedURL.Text, out uri))
            {
                _feedReference.Target = uri;
                labelURLError.Text = "valid URL";
                labelURLError.ForeColor = Color.Green;
            }
            else
            {
                labelURLError.Text = "invalid URL";
                labelURLError.ForeColor = Color.Red;
            }
        }

        // check if url is a valid url (begins with http or https and has the right format)
        // and shows a message if not.
        private bool IsValidFeedURL(string url, out Uri uri)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            return false;
        }
    }
}