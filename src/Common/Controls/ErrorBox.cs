/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Windows.Forms;
using Common.Properties;
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// A dialog displaying an error message and details.
    /// </summary>
    public partial class ErrorBox : Form
    {
        private readonly int _detailsHeight;

        #region Constructor
        private ErrorBox()
        {
            InitializeComponent();

            // Store text box size before it becomes invisible
            _detailsHeight = textDetails.Height;

            Load += delegate { ToggleDetails(); };
            buttonDetails.Click += delegate { ToggleDetails(); };
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays an error box with a message and details.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        /// <param name="detailsRtf">The details formatted as RTF.</param>
        /// <returns>The text the user entered if he pressed OK; otherwise <see langword="null"/>.</returns>
        public static void Show(string message, string detailsRtf)
        {
            #region Sanity checks
            if (message == null) throw new ArgumentNullException("message");
            if (detailsRtf == null) throw new ArgumentNullException("detailsRtf");
            #endregion

            Log.Error(message);

            using (var errorBox = new ErrorBox
            {
                Text = Application.ProductName,
                labelMessage = {Text = message},
                textDetails = {Rtf = detailsRtf}
            })
            {
                errorBox.toolTip.SetToolTip(errorBox.labelMessage, errorBox.labelMessage.Text);
                // ReSharper disable AccessToDisposedClosure
                errorBox.Shown += delegate { WindowsUtils.SetForegroundWindow(errorBox); };
                // ReSharper restore AccessToDisposedClosure
                errorBox.ShowDialog();
            }
        }
        #endregion

        #region Details button
        /// <summary>
        /// Shows or hides the details field.
        /// </summary>
        private void ToggleDetails()
        {
            textDetails.Visible = !textDetails.Visible;
            if (textDetails.Visible)
            {
                Height += _detailsHeight;
                buttonDetails.Text = Resources.HideDetails;
            }
            else
            {
                Height -= _detailsHeight;
                buttonDetails.Text = Resources.ShowDetails;
            }
        }
        #endregion
    }
}
