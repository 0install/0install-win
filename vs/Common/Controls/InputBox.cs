/*
 * Copyright 2006-2010 Bastian Eicher
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

namespace Common.Controls
{
    /// <summary>
    /// Shows a simple dialog asking the user to input some text.
    /// </summary>
    public partial class InputBox : Form
    {
        #region Constructor
        public InputBox()
        {
            InitializeComponent();
        }
        #endregion

        /// <summary>
        /// Displays an input box asking the the user to input some text.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="title">The window title to use.</param>
        /// <param name="defaultText">The default text to show pre-entered in the input field.</param>
        /// <returns>The text the user entered if he pressed OK; otherwise <see langword="null"/>.</returns>
        public static string Show(string prompt, string title, string defaultText)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(prompt)) throw new ArgumentNullException("prompt");
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");
            #endregion

            var inputBox = new InputBox
            {
                Text = title,
                labelPrompt = {Text = prompt},
                textInput = {Text = defaultText}
            };

            return (inputBox.ShowDialog() == DialogResult.OK) ? inputBox.textInput.Text : null;
        }
        
        /// <summary>
        /// Displays an input box asking the the user to input some text.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="title">The window title to use.</param>
        /// <returns>The text the user entered if he pressed OK; otherwise <see langword="null"/>.</returns>
        public static string Show(string prompt, string title)
        {
            return Show(prompt, title, "");
        }
    }
}