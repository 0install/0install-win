using System;
using System.Drawing;
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.FeedEditor
{
    public partial class FeedReferenceControl : UserControl
    {
        private FeedReference _feedReference = new FeedReference();

        public FeedReference FeedReference
        {
            get { return _feedReference; }
            set
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
            if (ControlCommon.IsValidFeedUrl(textBoxExtFeedURL.Text, out uri))
            {
                _feedReference.Target = uri;
                textBoxExtFeedURL.ForeColor = Color.Green;
            }
            else
            {
                textBoxExtFeedURL.ForeColor = Color.Red;
            }
        }
    }
}