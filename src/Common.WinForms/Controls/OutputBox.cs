/*
 * Copyright 2006-2014 Bastian Eicher
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
using NanoByte.Common.Utils;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// A simple dialog displaying selectable multi-line text.
    /// </summary>
    public partial class OutputBox : Form
    {
        #region Constructor
        private OutputBox()
        {
            InitializeComponent();
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays an ouput box with some text.
        /// </summary>
        /// <param name="title">The text to display above the <paramref name="message"/>.</param>
        /// <param name="message">The selectable multi-line text to display to the user.</param>
        /// <returns>The text the user entered if he pressed OK; otherwise <see langword="null"/>.</returns>
        public static void Show(string title, string message)
        {
            #region Sanity checks
            if (title == null) throw new ArgumentNullException("title");
            if (message == null) throw new ArgumentNullException("message");
            #endregion

            using (var outputBox = new OutputBox
            {
                Text = Application.ProductName,
                labelTitle = {Text = title},
                textMessage = {Text = message.Replace("\n", Environment.NewLine)}
            })
            {
                outputBox.toolTip.SetToolTip(outputBox.labelTitle, outputBox.labelTitle.Text);
                // ReSharper disable once AccessToDisposedClosure
                outputBox.Shown += delegate { WindowsUtils.SetForegroundWindow(outputBox); };
                outputBox.ShowDialog();
            }
        }
        #endregion
    }
}
