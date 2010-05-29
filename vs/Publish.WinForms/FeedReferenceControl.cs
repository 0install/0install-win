/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms
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
            if (ControlHelpers.IsValidFeedUrl(textBoxExtFeedURL.Text, out uri))
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
