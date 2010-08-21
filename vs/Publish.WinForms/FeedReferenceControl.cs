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
                textBoxExtFeedURL.Text = (value == null ? String.Empty : _feedReference.Source);
                _feedReference = value;
                targetBaseControl.TargetBase = value;
            }
        }

        public FeedReferenceControl()
        {
            InitializeComponent();
            targetBaseControl.TargetBase = _feedReference;
        }

        private void textBoxExtFeedURL_TextChanged(object sender, EventArgs e)
        {
            if (_feedReference == null) return;
            _feedReference.Source = textBoxExtFeedURL.Text;
        }
    }
}
