/*
 * Copyright 2006-2011 Bastian Eicher, Simon E. Silva Lauinger
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
using Common.Collections;

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
        /// <inheritdoc/>
        protected override void OnValidating(CancelEventArgs e)
        {
            #region Sanity checks
            if (e == null) throw new ArgumentNullException("e");
            #endregion

            e.Cancel = !ValidateUri(Text);

            base.OnValidating(e);
        }

        /// <inheritdoc/>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            #region Sanity checks
            if (drgevent == null) throw new ArgumentNullException("drgevent");
            #endregion

            drgevent.Effect = ValidateUri(GetDropText(drgevent))
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            base.OnDragEnter(drgevent);
        }

        /// <inheritdoc/>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            Text = GetDropText(drgevent);

            base.OnDragDrop(drgevent);
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
            TextChanged += delegate { ForeColor = ValidateUri(Text) ? Color.Green : Color.Red; };
            AllowDrop = true;
        }
        #endregion

        //--------------------//

        #region Helpers
        /// <summary>
        /// Checks if a text represents a valid <see cref="Uri"/>.
        /// </summary>
        /// <param name="text">Text to check.</param>
        protected virtual bool ValidateUri(string text)
        {
            // Allow empty input
            if (string.IsNullOrEmpty(text)) return true;

            Uri temp;
            // Check URI is well-formed
            if (!Uri.TryCreate(text, UriKind.Absolute, out temp)) return false;

            // Check URI is HTTP(S) if that was requested
            if (HttpOnly) return text.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https:", StringComparison.OrdinalIgnoreCase);

            return true;
        }

        /// <summary>
        /// Returns the text dropped on a control.
        /// </summary>
        /// <param name="dragEventArgs">Argument of a dragging operation.</param>
        /// <returns>The dropped text.</returns>
        private static string GetDropText(DragEventArgs dragEventArgs)
        {
            if (dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = dragEventArgs.Data.GetData(DataFormats.FileDrop) as string[];
                return EnumUtils.GetFirst(files);
            }
            if (dragEventArgs.Data.GetDataPresent(DataFormats.Text))
            {
                return (string)dragEventArgs.Data.GetData(DataFormats.Text);
            }

            return null;
        }
        #endregion
    }
}
