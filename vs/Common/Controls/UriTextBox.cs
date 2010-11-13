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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Common.Controls
{
    /// <summary>
    /// A <see cref="HintTextBox"/> designed specifically for entering URIs.
    /// </summary>
    /// <remarks>Will turn red for invalid input and green for valid input. Will not allow focus to be lost for invalid input.</remarks>
    [Description("A HintTextBox designed specifically for entering URIs.")]
    public class UriTextBox : HintTextBox
    {
        #region Events
        protected override void OnValidating(CancelEventArgs e)
        {
            #region Sanity checks
            if (e == null) throw new ArgumentNullException("e");
            #endregion

            e.Cancel = !ValidateUri();

            base.OnValidating(e);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="Uri"/> represented by this text box.
        /// </summary>
        /// <exception cref="UriFormatException">Thrown when trying to read while <see cref="TextBox.Text"/> is not a well-formed <see cref="Uri"/>.</exception>
        /// <remarks>It is always safe to set this property. It is safe to read this property after validation has been performed.</remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Uri Uri
        {
            get { return string.IsNullOrEmpty(Text) ? null : new Uri(Text, UriKind.Absolute); }
            set { Text = (value == null) ? null : value.ToString(); }
        }

        /// <summary>
        /// When set to <see langword="true"/> only URIs starting with "http:" or "https:" will be considered valid.
        /// </summary>
        [DefaultValue(false), Description("When set to true only URIs starting with \"http:\" or \"https:\" will be considered valid."), Category("Behavior")]
        public bool HttpOnly { get; set; }
        #endregion

        #region Constructor
        public UriTextBox()
        {
            // Use event instead of method override to ensure special handling of HintText works
            TextChanged += delegate { ForeColor = ValidateUri() ? Color.Green : Color.Red; };
        }
        #endregion

        //--------------------//

        #region Helpers
        /// <summary>
        /// Checks if <see cref="TextBox.Text"/> currently represents a valid <see cref="Uri"/>.
        /// </summary>
        protected virtual bool ValidateUri()
        {
            // Allow empty input
            if (string.IsNullOrEmpty(Text)) return true;

            Uri temp;
            // Check URI is well-formed
            if (!Uri.TryCreate(Text, UriKind.Absolute, out temp)) return false;

            // Check URI is HTTP(S) if that was requested
            if (HttpOnly) return Text.StartsWith("http:") || Text.StartsWith("https:");

            return true;
        }
        #endregion
    }
}
